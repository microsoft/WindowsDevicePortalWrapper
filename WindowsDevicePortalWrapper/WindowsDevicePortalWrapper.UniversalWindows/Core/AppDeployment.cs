//----------------------------------------------------------------------------------------------
// <copyright file="AppDeployment.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Credentials;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using static Microsoft.Tools.WindowsDevicePortal.DevicePortalException;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <content>
    /// Universal Windows Platform implementation of App Deployment methods.
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// API for getting installation status.
        /// </summary>
        /// <returns>The status</returns>
#pragma warning disable 1998
        public async Task<ApplicationInstallStatus> GetInstallStatusAsync()
        {
            ApplicationInstallStatus status = ApplicationInstallStatus.None;

            Uri uri = Utilities.BuildEndpoint(
                this.deviceConnection.Connection,
                InstallStateApi);

            HttpBaseProtocolFilter httpFilter = new HttpBaseProtocolFilter();
            httpFilter.AllowUI = false;

            if (this.deviceConnection.Credentials != null)
            {
                httpFilter.ServerCredential = new PasswordCredential();
                httpFilter.ServerCredential.UserName = this.deviceConnection.Credentials.UserName;
                httpFilter.ServerCredential.Password = this.deviceConnection.Credentials.Password;
            }

            using (HttpClient client = new HttpClient(httpFilter))
            {
                this.ApplyHttpHeaders(client, HttpMethods.Get);

                using (HttpResponseMessage response = await client.GetAsync(uri))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        if (response.StatusCode == HttpStatusCode.Ok)
                        {
                            // Status code: 200
                            if (response.Content != null)
                            {
                                Stream dataStream = null;

                                IBuffer dataBuffer = null;
                                using (IHttpContent messageContent = response.Content)
                                {
                                    dataBuffer = await messageContent.ReadAsBufferAsync();

                                    if (dataBuffer != null)
                                    {
                                        dataStream = dataBuffer.AsStream();
                                    }
                                }

                                if (dataStream != null)
                                {
                                    HttpErrorResponse errorResponse = DevicePortal.ReadJsonStream<HttpErrorResponse>(dataStream);

                                    if (errorResponse.Success)
                                    {
                                        status = ApplicationInstallStatus.Completed;
                                    }
                                    else
                                    {
                                        throw new DevicePortalException(response.StatusCode, errorResponse, uri);
                                    }
                                }
                                else
                                {
                                    throw new DevicePortalException(HttpStatusCode.Conflict, "Failed to deserialize GetInstallStatus response.");
                                }
                            }
                        }
                        else if (response.StatusCode == HttpStatusCode.NoContent)
                        {
                            // Status code: 204
                            status = ApplicationInstallStatus.InProgress;
                        }
                    }
                    else
                    {
                        throw await DevicePortalException.CreateAsync(response);
                    }
                }
            }
    
            return status;
        }
#pragma warning restore 1998

        /// <summary>
        /// Installs an application
        /// </summary>
        /// <param name="appName">Friendly name (ex: Hello World) of the application. If this parameter is not provided, the name of the package is assumed to be the app name.</param>
        /// <param name="packageFile">The application package file.</param>
        /// <param name="dependencyFiles">List containing the required dependency files.</param>
        /// <param name="certificateFile">Optional certificate file.</param>
        /// <param name="stateCheckIntervalMs">How frequently we should check the installation state.</param>
        /// <param name="timeoutInMinutes">Operation timeout.</param>
        /// <param name="uninstallPreviousVersion">Indicate whether or not the previous app version should be uninstalled prior to installing.</param>
        /// <remarks>InstallApplication sends ApplicationInstallStatus events to indicate the current progress in the installation process.
        /// Some applications may opt to not register for the AppInstallStatus event and await on InstallApplication.</remarks>
        /// <returns>Task for tracking completion of install initialization.</returns>
        public async Task InstallApplicationAsync(
            string appName,
            StorageFile packageFile, 
            List<StorageFile> dependencyFiles,
            StorageFile certificateFile = null,
            short stateCheckIntervalMs = 500,
            short timeoutInMinutes = 15,
            bool uninstallPreviousVersion = true)
        {
            string installPhaseDescription = string.Empty;

            try
            {
                // If appName was not provided, use the package file name
                if (string.IsNullOrWhiteSpace(appName))
                {
                    appName = packageFile.DisplayName;
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
                    await CopyFileToRequestStream(
                        packageFile,
                        dataStream);

                    // Copy dependency files, if any.
                    foreach (StorageFile depFile in dependencyFiles)
                    {
                        installPhaseDescription = string.Format("Copying: {0}", depFile.Name);
                        this.SendAppInstallStatus(
                            ApplicationInstallStatus.InProgress,
                            ApplicationInstallPhase.CopyingFile,
                            installPhaseDescription);
                        data = Encoding.ASCII.GetBytes(string.Format("\r\n--{0}\r\n", boundaryString));
                        dataStream.Write(data, 0, data.Length);
                        await CopyFileToRequestStream(
                            depFile, 
                            dataStream);
                    }

                    // Copy the certificate file, if provided.
                    if (certificateFile != null)
                    {
                        installPhaseDescription = string.Format("Copying: {0}", certificateFile.Name);
                        this.SendAppInstallStatus(
                            ApplicationInstallStatus.InProgress,
                            ApplicationInstallPhase.CopyingFile,
                            installPhaseDescription);
                        data = Encoding.ASCII.GetBytes(string.Format("\r\n--{0}\r\n", boundaryString));
                        dataStream.Write(data, 0, data.Length);
                        await CopyFileToRequestStream(
                            certificateFile, 
                            dataStream);
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

                    status = await this.GetInstallStatusAsync().ConfigureAwait(false);
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
    }
}
