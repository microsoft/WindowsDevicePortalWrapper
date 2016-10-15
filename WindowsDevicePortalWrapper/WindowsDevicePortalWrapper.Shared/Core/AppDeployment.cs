﻿//----------------------------------------------------------------------------------------------
// <copyright file="AppDeployment.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
#if !WINDOWS_UWP
using System.Net;
using System.Net.Http;
#endif // !WINDOWS_UWP
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
#if WINDOWS_UWP
using Windows.Foundation;
using Windows.Security.Credentials;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using Windows.Web.Http.Headers;
#endif

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <content>
    /// Wrappers for App Deployment methods.
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// API to retrieve list of installed packages.
        /// </summary>
        public static readonly string InstalledPackagesApi = "api/app/packagemanager/packages";

        /// <summary>
        /// Install state API.
        /// </summary>
        public static readonly string InstallStateApi = "api/app/packagemanager/state";

        /// <summary>
        /// API for package management.
        /// </summary>
        public static readonly string PackageManagerApi = "api/app/packagemanager/package";

        /// <summary>
        /// App Install Status handler.
        /// </summary>
        public event ApplicationInstallStatusEventHandler AppInstallStatus;

        /// <summary>
        /// Gets the collection of applications installed on the device.
        /// </summary>
        /// <returns>AppPackages object containing the list of installed application packages.</returns>
        public async Task<AppPackages> GetInstalledAppPackagesAsync()
        {
            return await this.GetAsync<AppPackages>(InstalledPackagesApi);
        }

        /// <summary>
        /// Installs an application
        /// </summary>
        /// <param name="appName">Friendly name (ex: Hello World) of the application. If this parameter is not provided, the name of the package is assumed to be the app name.</param>
        /// <param name="packageFileName">Full name of the application package file.</param>
        /// <param name="dependencyFileNames">List containing the full names of any required dependency files.</param>
        /// <param name="certificateFileName">Full name of the optional certificate file.</param>
        /// <param name="stateCheckIntervalMs">How frequently we should check the installation state.</param>
        /// <param name="timeoutInMinutes">Operation timeout.</param>
        /// <param name="uninstallPreviousVersion">Indicate whether or not the previous app version should be uninstalled prior to installing.</param>
        /// <remarks>InstallApplication sends ApplicationInstallStatus events to indicate the current progress in the installation process.
        /// Some applications may opt to not register for the AppInstallStatus event and await on InstallApplication.</remarks>
        /// <returns>Task for tracking completion of install initialization.</returns>
        public async Task InstallApplicationAsync(
            string appName,
            string packageFileName, 
            List<string> dependencyFileNames,
            string certificateFileName = null,
            short stateCheckIntervalMs = 500,
            short timeoutInMinutes = 15,
            bool uninstallPreviousVersion = true)
        {
            string installPhaseDescription = string.Empty;

            try
            {
                FileInfo packageFile = new FileInfo(packageFileName);

                // If appName was not provided, use the package file name
                if (string.IsNullOrEmpty(appName))
                {
                    appName = packageFile.Name;
                }

                // Uninstall the application's previous version, if one exists.
                if (uninstallPreviousVersion)
                {
                    installPhaseDescription = string.Format("Uninstalling any previous version of {0}", appName);
                    this.SendAppInstallStatus(
                        ApplicationInstallStatus.InProgress,
                        ApplicationInstallPhase.UninstallingPreviousVersion,
                        installPhaseDescription);
                    AppPackages installedApps = await this.GetInstalledAppPackagesAsync();
                    foreach (PackageInfo package in installedApps.Packages)
                    {
                        if (package.Name == appName)
                        {
                            await this.UninstallApplicationAsync(package.FullName);
                            break;
                        }
                    }
                }

                // Create the API endpoint and generate a unique boundary string.
                Uri uri;
                string boundaryString;
                this.CreateAppInstallEndpointAndBoundaryString(
                    packageFile.Name,
                    out uri,
                    out boundaryString);

                using (MemoryStream dataStream = new MemoryStream())
                {
                    byte[] data;

                    // Copy the application package.
                    installPhaseDescription = string.Format("Copying: {0}", packageFile.Name);
                    this.SendAppInstallStatus(
                        ApplicationInstallStatus.InProgress,
                        ApplicationInstallPhase.CopyingFile,
                        installPhaseDescription);
                    data = Encoding.ASCII.GetBytes(string.Format("--{0}\r\n", boundaryString));
                    dataStream.Write(data, 0, data.Length);
                    CopyFileToRequestStream(packageFile, dataStream);

                    // Copy dependency files, if any.
                    foreach (string dependencyFile in dependencyFileNames)
                    {
                        FileInfo fi = new FileInfo(dependencyFile);
                        installPhaseDescription = string.Format("Copying: {0}", fi.Name);
                        this.SendAppInstallStatus(
                            ApplicationInstallStatus.InProgress,
                            ApplicationInstallPhase.CopyingFile,
                            installPhaseDescription);
                        data = Encoding.ASCII.GetBytes(string.Format("\r\n--{0}\r\n", boundaryString));
                        dataStream.Write(data, 0, data.Length);
                        CopyFileToRequestStream(fi, dataStream);
                    }

                    // Copy the certificate file, if provided.
                    if (!string.IsNullOrEmpty(certificateFileName))
                    {
                        FileInfo fi = new FileInfo(certificateFileName);
                        installPhaseDescription = string.Format("Copying: {0}", fi.Name);
                        this.SendAppInstallStatus(
                            ApplicationInstallStatus.InProgress,
                            ApplicationInstallPhase.CopyingFile,
                            installPhaseDescription);
                        data = Encoding.ASCII.GetBytes(string.Format("\r\n--{0}\r\n", boundaryString));
                        dataStream.Write(data, 0, data.Length);
                        CopyFileToRequestStream(fi, dataStream);
                    }

                    // Close the installation request data.
                    data = Encoding.ASCII.GetBytes(string.Format("\r\n--{0}--\r\n", boundaryString));
                    dataStream.Write(data, 0, data.Length);

                    dataStream.Position = 0;

                    string contentType = string.Format("multipart/form-data; boundary={0}", boundaryString);

                    // Make the HTTP request.
                    await this.PostAsync(uri, dataStream, contentType);
                }

                // Poll the status until complete.
                ApplicationInstallStatus status = ApplicationInstallStatus.InProgress;
                do
                {
                    installPhaseDescription = string.Format("Installing {0}", appName);
                    this.SendAppInstallStatus(
                        ApplicationInstallStatus.InProgress,
                        ApplicationInstallPhase.Installing,
                        installPhaseDescription);

                    await Task.Delay(TimeSpan.FromMilliseconds(stateCheckIntervalMs));

                    status = await this.GetInstallStatusAsync();
                }
                while (status == ApplicationInstallStatus.InProgress);

                installPhaseDescription = string.Format("{0} installed successfully", appName);
                this.SendAppInstallStatus(
                    ApplicationInstallStatus.Completed,
                    ApplicationInstallPhase.Idle,
                    installPhaseDescription);
            }
            catch (Exception e)
            {
                DevicePortalException dpe = e as DevicePortalException;

                if (dpe != null)
                {
                    this.SendAppInstallStatus(
                        ApplicationInstallStatus.Failed,
                        ApplicationInstallPhase.Idle,
                        string.Format("Failed to install {0}: {1}", appName, dpe.Reason));
                }
                else
                {
                    this.SendAppInstallStatus(
                        ApplicationInstallStatus.Failed,
                        ApplicationInstallPhase.Idle,
                        string.Format("Failed to install {0}: {1}", appName, installPhaseDescription));
                }
            }
        }

        /// <summary>
        /// Uninstalls the specified application.
        /// </summary>
        /// <param name="packageName">The name of the application package to uninstall.</param>
        /// <returns>Task tracking the uninstall operation.</returns>
        public async Task UninstallApplicationAsync(string packageName)
        {
            await this.DeleteAsync(
                PackageManagerApi,
                //// NOTE: When uninstalling an app package, the package name is not Hex64 encoded.
                string.Format("package={0}", packageName));
        }

        /// <summary>
        /// Builds the application installation Uri and generates a unique boundary string for the multipart form data.
        /// </summary>
        /// <param name="packageName">The name of the application package.</param>
        /// <param name="uri">The endpoint for the install request.</param>
        /// <param name="boundaryString">Unique string used to separate the parts of the multipart form data.</param>
        private void CreateAppInstallEndpointAndBoundaryString(
            string packageName,
            out Uri uri,
            out string boundaryString)           
        {
            uri = Utilities.BuildEndpoint(
                this.deviceConnection.Connection,
                PackageManagerApi,
                string.Format("package={0}", packageName));

            boundaryString = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Sends application install status.
        /// </summary>
        /// <param name="status">Status of the installation.</param>
        /// <param name="phase">Current installation phase (ex: Uninstalling previous version)</param>
        /// <param name="message">Optional error message describing the install status.</param>
        private void SendAppInstallStatus(
            ApplicationInstallStatus status,
            ApplicationInstallPhase phase,
            string message = "")
        {
            this.AppInstallStatus?.Invoke(
                this,
                new ApplicationInstallStatusEventArgs(status, phase, message));
        }

        #region Data contract
        /// <summary>
        /// Object representing a list of Application Packages
        /// </summary>
        [DataContract]
        public class AppPackages
        {
            /// <summary>
            /// Gets a list of the packages
            /// </summary>
            [DataMember(Name = "InstalledPackages")]
            public List<PackageInfo> Packages { get; private set; }

            /// <summary>
            /// Presents a user readable representation of a list of AppPackages
            /// </summary>
            /// <returns>User readable list of AppPackages.</returns>
            public override string ToString()
            {
                string output = "Packages:\n";
                foreach (PackageInfo package in this.Packages)
                {
                    output += package;
                }

                return output;
            }
        }

        /// <summary>
        /// Object representing the install state
        /// </summary>
        [DataContract]
        public class InstallState
        {
            /// <summary>
            /// Gets install state code
            /// </summary>
            [DataMember(Name = "Code")]
            public int Code { get; private set; }

            /// <summary>
            /// Gets message text
            /// </summary>
            [DataMember(Name = "CodeText")]
            public string CodeText { get; private set; }

            /// <summary>
            /// Gets reason for state
            /// </summary>
            [DataMember(Name = "Reason")]
            public string Reason { get; private set; }

            /// <summary>
            /// Gets a value indicating whether this was successful
            /// </summary>
            [DataMember(Name = "Success")]
            public bool WasSuccessful { get; private set; }
        }

        /// <summary>
        /// object representing the package information
        /// </summary>
        [DataContract]
        public class PackageInfo
        {
            /// <summary>
            /// Gets package name
            /// </summary>
            [DataMember(Name = "Name")]
            public string Name { get; private set; }

            /// <summary>
            /// Gets package family name
            /// </summary>
            [DataMember(Name = "PackageFamilyName")]
            public string FamilyName { get; private set; }

            /// <summary>
            /// Gets package full name
            /// </summary>
            [DataMember(Name = "PackageFullName")]
            public string FullName { get; private set; }

            /// <summary>
            /// Gets package relative Id
            /// </summary>
            [DataMember(Name = "PackageRelativeId")]
            public string AppId { get; private set; }

            /// <summary>
            /// Gets package publisher
            /// </summary>
            [DataMember(Name = "Publisher")]
            public string Publisher { get; private set; }

            /// <summary>
            /// Gets package version
            /// </summary>
            [DataMember(Name = "Version")]
            public PackageVersion Version { get; private set; }

            /// <summary>
            /// Get a string representation of the package
            /// </summary>
            /// <returns>String representation</returns>
            public override string ToString()
            {
                return string.Format("\t{0}\n\t\t{1}\n", this.FullName, this.AppId);
            }
        }

        /// <summary>
        /// Object representing a package version
        /// </summary>
        [DataContract]
        public class PackageVersion
        {
            /// <summary>
            ///  Gets version build
            /// </summary>
            [DataMember(Name = "Build")]
            public int Build { get; private set; }

            /// <summary>
            /// Gets package Major number
            /// </summary>
            [DataMember(Name = "Major")]
            public int Major { get; private set; }

            /// <summary>
            /// Gets package minor number
            /// </summary>
            [DataMember(Name = "Minor")]
            public int Minor { get; private set; }

            /// <summary>
            /// Gets package revision
            /// </summary>
            [DataMember(Name = "Revision")]
            public int Revision { get; private set; }

            /// <summary>
            /// Gets package version
            /// </summary>
            public Version Version
            {
                get { return new Version(this.Major, this.Minor, this.Build, this.Revision); }
            }

            /// <summary>
            /// Get a string representation of a version
            /// </summary>
            /// <returns>String representation</returns>
            public override string ToString()
            {
                return Version.ToString();
            }
        }
        #endregion // Data contract
    }
}
