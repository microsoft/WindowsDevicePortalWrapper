//----------------------------------------------------------------------------------------------
// <copyright file="AppDeployment.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsDevicePortal
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Threading.Tasks;

    /// <content>
    /// Wrappers for App Deployment methods
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// Packages GET API
        /// </summary>
        private static readonly string InstalledPackagesApi = "api/app/packagemanager/packages";

        /// <summary>
        /// Install state API
        /// </summary>
        private static readonly string InstallStateApi = "api/app/packagemanager/state";

        /// <summary>
        /// API for package management
        /// </summary>
        private static readonly string PackageManagerApi = "api/app/packagemanager/package";

        /// <summary>
        /// Gets or sets install status handler
        /// </summary>
        public ApplicationInstallStatusEventHandler AppInstallStatus
        {
            get;
            set;
        }

        /// <summary>
        /// API for getting installation status
        /// </summary>
        /// <returns>The status</returns>
        public async Task<ApplicationInstallStatus> GetInstallStatus()
        {
            ApplicationInstallStatus status = ApplicationInstallStatus.None;

            Uri uri = Utilities.BuildEndpoint(
                this.deviceConnection.Connection,
                InstallStateApi);

            WebRequestHandler handler = new WebRequestHandler();
            handler.UseDefaultCredentials = false;
            handler.Credentials = this.deviceConnection.Credentials;
            handler.ServerCertificateValidationCallback = this.ServerCertificateValidation;

            using (HttpClient client = new HttpClient(handler))
            {
                Task<HttpResponseMessage> getTask = client.GetAsync(uri);
                await getTask.ConfigureAwait(false);
                getTask.Wait();

                using (HttpResponseMessage response = getTask.Result)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            // Status code: 200
                            status = ApplicationInstallStatus.Completed;
                        }
                        else if (response.StatusCode == HttpStatusCode.NoContent)
                        {
                            // Status code: 204
                            status = ApplicationInstallStatus.InProgress;
                        }
                    }
                    else
                    {
                        status = ApplicationInstallStatus.Failed; 
                    }
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
            return await this.Get<AppPackages>(InstalledPackagesApi);
        }

        /// <summary>
        /// Installs an application
        /// </summary>
        /// <param name="appName">PFN of the application</param>
        /// <param name="packageFileName">Name of the file</param>
        /// <param name="dependencyFileNames">List of any dependency files</param>
        /// <param name="stateCheckIntervalMs">How frequently we should check the installation state</param>
        /// <param name="timeoutInMinutes">Operation timeout</param>
        /// <remarks>InstallApplication sends ApplicationInstallStatus events to indicate the current progress in the installation process.
        /// Some applications may opt to not register for the AppInstallStatus event and await on InstallApplication.</remarks>
        /// <returns>Task for tracking completion of install initialization.</returns>
        public async Task InstallApplication(
            string appName,
            string packageFileName, 
            List<string> dependencyFileNames,
            short stateCheckIntervalMs = 500,
            short timeoutInMinutes = 15)
        {
            string installPhaseDescription = string.Empty;

            try
            {
                // Uninstall the application's previous version, if one exists.
                installPhaseDescription = string.Format("Uninstalling previous version of {0}", appName);
                this.SendAppInstallStatus(
                    ApplicationInstallStatus.InProgress,
                    ApplicationInstallPhase.UninstallingPreviousVersion,
                    installPhaseDescription);
                AppPackages installedApps = await this.GetInstalledAppPackages();
                foreach (PackageInfo package in installedApps.Packages)
                {
                    if (package.Name == appName)
                    {
                        await this.UninstallApplication(package.FullName);
                        break;
                    }
                }

                // Create the API endpoint and generate a unique boundary string.
                FileInfo fi = new FileInfo(packageFileName);
                Uri uri = Utilities.BuildEndpoint(
                    this.deviceConnection.Connection,
                    PackageManagerApi,
                    string.Format("package={0}", fi.Name));
                string boundaryString = Guid.NewGuid().ToString();

                // Create the install request.
                HttpWebRequest request = WebRequest.Create(uri) as HttpWebRequest;
                request.AllowWriteStreamBuffering = false;
                request.ContentType = string.Format("multipart/form-data; boundary={0}", boundaryString);
                request.Credentials = this.deviceConnection.Credentials;
                request.Headers[HttpRequestHeader.Authorization] = string.Format(
                    "Basic {0}",
                    Utilities.Hex64Encode(string.Format("{0}:{1}", this.deviceConnection.Credentials.UserName, this.deviceConnection.Credentials.Password)));
                request.KeepAlive = true;
                request.Method = "POST";
                request.SendChunked = true;
                request.ServerCertificateValidationCallback = this.ServerCertificateValidation;
                request.Timeout = timeoutInMinutes * 60 * 1000;

                using (Stream requestStream = request.GetRequestStream())
                {
                    byte[] data;

                    // Upload the application package.
                    installPhaseDescription = string.Format("Uploading: {0}", fi.Name);
                    this.SendAppInstallStatus(
                        ApplicationInstallStatus.InProgress,
                        ApplicationInstallPhase.CopyingFile,
                        installPhaseDescription);
                    data = Encoding.ASCII.GetBytes(string.Format("--{0}\r\n", boundaryString));
                    requestStream.Write(data, 0, data.Length);
                    this.CopyInstallationFileToStream(fi, requestStream);

                    // Upload the dependency file(s).
                    foreach (string dependencyFile in dependencyFileNames)
                    {
                        fi = new FileInfo(dependencyFile);
                        installPhaseDescription = string.Format("Uploading: {0}", fi.Name);
                        this.SendAppInstallStatus(
                            ApplicationInstallStatus.InProgress,
                            ApplicationInstallPhase.CopyingFile,
                            installPhaseDescription);
                        data = Encoding.ASCII.GetBytes(string.Format("\r\n--{0}\r\n", boundaryString));
                        requestStream.Write(data, 0, data.Length);
                        this.CopyInstallationFileToStream(fi, requestStream);
                    }

                    // Close the installation request data.
                    data = Encoding.ASCII.GetBytes(string.Format("\r\n--{0}--\r\n", boundaryString));
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
                        throw new DevicePortalException(
                            response.StatusCode,
                            response.StatusCode.ToString(),
                            uri,
                            "Failed to upload installation package");
                    }
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

                    System.Threading.Thread.Sleep(stateCheckIntervalMs);

                    status = await this.GetInstallStatus();
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

                HttpStatusCode status = (HttpStatusCode)0;
                Uri request = null;
                if (dpe != null)
                {
                    status = dpe.StatusCode;
                    request = dpe.RequestUri;
                }

                this.SendConnectionStatus(
                    DeviceConnectionStatus.Failed,
                    DeviceConnectionPhase.Idle,
                    string.Format("Device connection failed: {0}", installPhaseDescription));
            }
        }

        /// <summary>
        /// Uninstalls the specified application.
        /// </summary>
        /// <param name="packageName">The name of the application package to uninstall.</param>
        /// <returns>Task tracking the uninstall operation.</returns>
        public async Task UninstallApplication(string packageName)
        {
            await this.Delete(
                PackageManagerApi,
                //// NOTE: When uninstalling an app package, the package name is not Hex64 encoded.
                string.Format("package={0}", packageName));
        }

        /// <summary>
        /// Sends app status (invokes the handler)
        /// </summary>
        /// <param name="status">status to send</param>
        /// <param name="phase">current phase of installation</param>
        /// <param name="message">an optional message</param>
        private void SendAppInstallStatus(
            ApplicationInstallStatus status,
            ApplicationInstallPhase phase,
            string message = "")
        {
            this.AppInstallStatus?.Invoke(
                this,
                new ApplicationInstallStatusEventArgs(status, phase, message));
        }

        /// <summary>
        /// Helper to copy a file to a stream
        /// </summary>
        /// <param name="file">file object</param>
        /// <param name="stream">destination stream</param>
        private void CopyInstallationFileToStream(
            FileInfo file,
            Stream stream)
        {
            byte[] data;
            string contentDisposition = string.Format("Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\n", file.Name, file.Name);
            string contentType = "Content-Type: application/octet-stream\r\n\r\n";

            data = Encoding.ASCII.GetBytes(contentDisposition);
            stream.Write(data, 0, data.Length);

            data = Encoding.ASCII.GetBytes(contentType);
            stream.Write(data, 0, data.Length);

            using (FileStream fs = File.OpenRead(file.FullName))
            {
                fs.CopyTo(stream);
            }
        }

        #region Data contract
        /// <summary>
        /// Object representing a list of Application Packages
        /// </summary>
        [DataContract]
        public class AppPackages
        {
            /// <summary>
            /// Gets or sets a list of the packages
            /// </summary>
            [DataMember(Name = "InstalledPackages")]
            public List<PackageInfo> Packages { get; set; }
        }

        /// <summary>
        /// Object representing the install state
        /// </summary>
        [DataContract]
        public class InstallState
        {
            /// <summary>
            /// Gets or sets install state code
            /// </summary>
            [DataMember(Name = "Code")]
            public int Code { get; set; }

            /// <summary>
            /// Gets or sets message text
            /// </summary>
            [DataMember(Name = "CodeText")]
            public string CodeText { get; set; }

            /// <summary>
            /// Gets or sets reason for state
            /// </summary>
            [DataMember(Name = "Reason")]
            public string Reason { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this was successful
            /// </summary>
            [DataMember(Name = "Success")]
            public bool WasSuccessful { get; set; }
        }

        /// <summary>
        /// object representing the package information
        /// </summary>
        [DataContract]
        public class PackageInfo
        {
            /// <summary>
            /// Gets or sets package name
            /// </summary>
            [DataMember(Name = "Name")]
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets package family name
            /// </summary>
            [DataMember(Name = "PackageFamilyName")]
            public string FamilyName { get; set; }

            /// <summary>
            /// Gets or sets package full name
            /// </summary>
            [DataMember(Name = "PackageFullName")]
            public string FullName { get; set; }

            /// <summary>
            /// Gets or sets package relative Id
            /// </summary>
            [DataMember(Name = "PackageRelativeId")]
            public string AppId { get; set; }

            /// <summary>
            /// Gets or sets package publisher
            /// </summary>
            [DataMember(Name = "Publisher")]
            public string Publisher { get; set; }

            /// <summary>
            /// Gets or sets package version
            /// </summary>
            [DataMember(Name = "Version")]
            public PackageVersion Version { get; set; }

            /// <summary>
            /// Get a string representation of the package
            /// </summary>
            /// <returns>String representation</returns>
            public override string ToString()
            {
                return string.Format("{0} ({1})", this.Name, this.Version);
            }
        }

        /// <summary>
        /// Object representing a package version
        /// </summary>
        [DataContract]
        public class PackageVersion
        {
            /// <summary>
            ///  Gets or sets version build
            /// </summary>
            [DataMember(Name = "Build")]
            public int Build { get; set; }

            /// <summary>
            /// Gets or sets package Major number
            /// </summary>
            [DataMember(Name = "Major")]
            public int Major { get; set; }

            /// <summary>
            /// Gets or sets package minor number
            /// </summary>
            [DataMember(Name = "Minor")]
            public int Minor { get; set; }

            /// <summary>
            /// Gets or sets package revision
            /// </summary>
            [DataMember(Name = "Revision")]
            public int Revision { get; set; }

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
