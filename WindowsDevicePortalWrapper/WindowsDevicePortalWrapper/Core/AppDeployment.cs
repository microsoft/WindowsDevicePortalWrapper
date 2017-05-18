//----------------------------------------------------------------------------------------------
// <copyright file="AppDeployment.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using static Microsoft.Tools.WindowsDevicePortal.DevicePortalException;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <content>
    /// .net 4.x implementation of App Deployment methods.
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// Gets the status of a pending or most recent installation, if any. 
        /// </summary>
        /// <returns>The status</returns>
        public async Task<ApplicationInstallStatus> GetInstallStatusAsync()
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
                this.ApplyHttpHeaders(client, HttpMethods.Get);
                using (HttpResponseMessage response = await client.GetAsync(uri).ConfigureAwait(false))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            // Status code: 200
                            if (response.Content == null)
                            {
                                status = ApplicationInstallStatus.Completed;
                            }
                            else
                            {
                                // If we have a response body, it's possible this was an error
                                // (even though we got an HTTP 200).
                                Stream dataStream = null;
                                using (HttpContent content = response.Content)
                                {
                                    dataStream = new MemoryStream();

                                    await content.CopyToAsync(dataStream).ConfigureAwait(false);

                                    // Ensure we point the stream at the origin.
                                    dataStream.Position = 0;
                                }

                                if (dataStream != null)
                                {
                                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(HttpErrorResponse));

                                    HttpErrorResponse errorResponse = (HttpErrorResponse)serializer.ReadObject(dataStream);

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
                                    throw new DevicePortalException(response.StatusCode, "Failed to deserialize GetInstallStatus response.");
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
    }
}
