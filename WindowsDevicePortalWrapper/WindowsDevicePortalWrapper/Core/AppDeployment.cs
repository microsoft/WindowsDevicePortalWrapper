// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE.TXT in the project root license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal
{
    public partial class DevicePortal
    {
        private static readonly String _installedPackagesApi = "api/app/packagemanager/packages";
        private static readonly String _installStateApi = "api/app/packagemanager/state";
        private static readonly String _packageManagerApi = "api/app/packagemanager/package";

        public ApplicationInstallStatusEventHandler AppInstallStatus;

        private void CopyInstallationFileToStream(FileInfo file,
                                                Stream stream)
        {
            Byte[] data;
            String contentDisposition = String.Format("Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\n", file.Name, file.Name);
            String contentType = "Content-Type: application/octet-stream\r\n\r\n";
            
            data = Encoding.ASCII.GetBytes(contentDisposition);
            stream.Write(data, 0, data.Length);

            data = Encoding.ASCII.GetBytes(contentType);
            stream.Write(data, 0, data.Length);

            using (FileStream fs = File.OpenRead(file.FullName))
            {
                fs.CopyTo(stream);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<ApplicationInstallStatus> GetInstallStatus()
        {
            ApplicationInstallStatus status = ApplicationInstallStatus.None;

            try
            {
                InstallState state = await Get<InstallState>(_installStateApi);
                status = (state.WasSuccessful) ? ApplicationInstallStatus.Completed : ApplicationInstallStatus.Failed;
            }
            catch(Exception e)
            {
                if (e is WebException)
                {
                    status = ApplicationInstallStatus.None;
                }
                else if (e is NullReferenceException)
                {
                    status = ApplicationInstallStatus.InProgress;
                }
                else
                {
                    status = ApplicationInstallStatus.Failed;
                }
            }

            return status;
        }

        /// <summary>
        /// Gets the collection of applications installed on the device.
        /// </summary>
        /// <returns>AppPackages object containing the list of installed application packages.</returns>
        public async Task<AppPackages> GetInstalledAppPackages()
        {
            return await Get<AppPackages>(_installedPackagesApi);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appName"></param>
        /// <param name="packageFileName"></param>
        /// <param name="dependencyFileNames"></param>
        /// <param name="stateCheckIntervalMs"></param>
        /// <remarks>InstallApplication sends ApplicationInstallStatus events to indicate the current progress in the installation process.
        /// Some applications may opt to not register for the AppInstallStatus event and await on InstallApplication.</remarks>
        public async Task InstallApplication(String appName,
                                            String packageFileName, 
                                            List<String> dependencyFileNames,
                                            Int16 stateCheckIntervalMs = 500)
        {
            String installPhaseDescription = String.Empty;

            try
            {
                // Uninstall the application's previous version, if one exists.
                installPhaseDescription = "Uninstalling previous app version";
                SendAppInstallStatus(ApplicationInstallStatus.InProgress,
                                    ApplicationInstallPhase.UninstallingPreviousVersion,
                                    installPhaseDescription);
                AppPackages installedApps = await GetInstalledAppPackages();
                foreach (PackageInfo package in installedApps.Packages)
                {
                    if (package.Name == appName)
                    {
                        await UninstallApplication(package.FullName);
                        break;
                    }
                }

                // Create the API endpoint and generate a unique boundary string.
                FileInfo fi = new FileInfo(packageFileName);
                Uri uri = Utilities.BuildEndpoint(_deviceConnection.Connection,
                                                _packageManagerApi,
                                                String.Format("package={0}", fi.Name));
                String boundaryString = Guid.NewGuid().ToString();

                // Create the install request.
                HttpWebRequest request = WebRequest.Create(uri) as HttpWebRequest;
                request.AllowWriteStreamBuffering = false;
                request.ContentType = String.Format("multipart/form-data; boundary={0}", boundaryString);
                request.Credentials = _deviceConnection.Credentials;
                request.Headers[HttpRequestHeader.Authorization] = String.Format("Basic {0}",
                                                                    Utilities.Hex64Encode(String.Format("{0}:{1}",
                                                                        _deviceConnection.Credentials.UserName,
                                                                        _deviceConnection.Credentials.Password)));
                request.KeepAlive = true;
                request.Method = "POST";
                request.SendChunked = true;
                request.ServerCertificateValidationCallback = ServerCertificateValidation;
                request.Timeout = 15 * 60 * 1000;   // Fifteen minutes.

                using (Stream requestStream = request.GetRequestStream())
                {
                    Byte[] data;

                    // Upload the application package.
                    installPhaseDescription = String.Format("Uploading: {0}", fi.Name);
                    SendAppInstallStatus(ApplicationInstallStatus.InProgress,
                                        ApplicationInstallPhase.CopyingFile,
                                        installPhaseDescription);
                    data = Encoding.ASCII.GetBytes(String.Format("--{0}\r\n", boundaryString));
                    requestStream.Write(data, 0, data.Length);
                    CopyInstallationFileToStream(fi, requestStream);

                    // Upload the dependency file(s).
                    foreach (String dependencyFile in dependencyFileNames)
                    {
                        fi = new FileInfo(dependencyFile);
                        installPhaseDescription = String.Format("Uploading: {0}", fi.Name);
                        SendAppInstallStatus(ApplicationInstallStatus.InProgress,
                                            ApplicationInstallPhase.CopyingFile,
                                            installPhaseDescription);
                        data = Encoding.ASCII.GetBytes(String.Format("\r\n--{0}\r\n", boundaryString));
                        requestStream.Write(data, 0, data.Length);
                        CopyInstallationFileToStream(fi, requestStream);
                    }

                    // Close the installation request data.
                    data = Encoding.ASCII.GetBytes(String.Format("\r\n--{0}--\r\n", boundaryString));
                    requestStream.Write(data, 0, data.Length);
                }

                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        // This ensures the response stream is disposed properly.
                    }

                    if (response.StatusCode != HttpStatusCode.Accepted)
                    {
                        throw new DevicePortalException(response.StatusCode,
                                                    response.StatusCode.ToString(),
                                                    uri,
                                                    "Failed to upload installation package");
                    }
                }

                // Poll the status until complete.
                ApplicationInstallStatus status = ApplicationInstallStatus.InProgress;
                do
                {
                    installPhaseDescription = String.Format("Installing {0}", appName);
                    SendAppInstallStatus(ApplicationInstallStatus.InProgress,
                                        ApplicationInstallPhase.Installing,
                                        installPhaseDescription);

                    System.Threading.Thread.Sleep(stateCheckIntervalMs);

                    status = await GetInstallStatus();
                }
                while (status == ApplicationInstallStatus.InProgress);

                installPhaseDescription = String.Format("{0} installed successfully", appName);
                SendAppInstallStatus(ApplicationInstallStatus.Completed,
                                    ApplicationInstallPhase.Idle,
                                    installPhaseDescription);

            }
            catch(Exception e)
            {
                DevicePortalException dpe = e as DevicePortalException;

                HttpStatusCode status = (HttpStatusCode)0;
                Uri request = null;
                if (dpe != null)
                {
                    status = dpe.StatusCode;
                    request = dpe.RequestUri;
                }

                SendConnectionStatus(DeviceConnectionStatus.Failed,
                                    DeviceConnectionPhase.Idle,
                                    String.Format("Device connection failed: {0}", installPhaseDescription));
            }
        }

        /// <summary>
        /// Uninstalls the specified application.
        /// </summary>
        /// <param name="packageName">The name of the application package to uninstall.</param>
        public async Task UninstallApplication(String packageName)
        {
            
            await Delete(_packageManagerApi,
                                // NOTE: When uninstalling an app package, the package name is not Hex64 encoded.
                                String.Format("package={0}", packageName));
        }

        private void SendAppInstallStatus(ApplicationInstallStatus status,
                                        ApplicationInstallPhase phase,
                                        String message = "")
        {
            AppInstallStatus?.Invoke(this,
                                    new ApplicationInstallStatusEventArgs(status, phase, message));
        }
    }

#region Data contract
    [DataContract]
    public class AppPackages
    {
        [DataMember(Name="InstalledPackages")]
        public List<PackageInfo> Packages { get; set; }
    }

    [DataContract]
    public class InstallState
    {
        [DataMember(Name="Code")]
        public Int32 Code;

        [DataMember(Name="CodeText")]
        public String CodeText;

        [DataMember(Name ="Reason")]
        public String Reason;

        [DataMember(Name="Success")]
        public Boolean WasSuccessful;
    }

    [DataContract]
    public class PackageInfo
    {
        [DataMember(Name="Name")]
        public String Name { get; set; }

        [DataMember(Name="PackageFamilyName")]
        public String FamilyName { get; set; }

        [DataMember(Name="PackageFullName")]
        public String FullName { get; set; }

        [DataMember(Name="PackageRelativeId")]
        public String AppId { get; set; }

        [DataMember(Name="Publisher")]
        public String Publisher { get; set; }

        [DataMember(Name="Version")]
        public PackageVersion Version { get; set; }

        public override string ToString()
        {
            return String.Format("{0} ({1})", Name, Version);
        }
    }

    [DataContract]
    public class PackageVersion
    {
        [DataMember(Name="Build")]
        public Int32 Build { get; set; }

        [DataMember(Name="Major")]
        public Int32 Major { get; set; }

        [DataMember(Name="Minor")]
        public Int32 Minor { get; set; }

        [DataMember(Name="Revision")]
        public Int32 Revision { get; set; }

        public Version Version
        {
            get { return new Version(Major, Minor, Build, Revision); }
        }

        public override string ToString()
        {
            return Version.ToString();
        }
    }
#endregion // Data contract
}
