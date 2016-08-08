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
using Windows.Foundation;
using Windows.Security.Credentials;
using Windows.Storage.Streams;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using Windows.Web.Http.Headers;
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
        public async Task<ApplicationInstallStatus> GetInstallStatus()
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

                IAsyncOperationWithProgress<HttpResponseMessage, HttpProgress> responseOperation = client.GetAsync(uri);
                while (responseOperation.Status != AsyncStatus.Completed)
                { 
                }

                using (HttpResponseMessage response = responseOperation.GetResults())
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
                                    IAsyncOperationWithProgress<IBuffer, ulong> bufferOperation = messageContent.ReadAsBufferAsync();
                                    while (bufferOperation.Status != AsyncStatus.Completed)
                                    {
                                    }

                                    dataBuffer = bufferOperation.GetResults();

                                    if (dataBuffer != null)
                                    {
                                        dataStream = dataBuffer.AsStream();
                                    }
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
                        throw new DevicePortalException(response);
                    }
                }
            }
    
            return status;
        }
#pragma warning restore 1998
    }
}
