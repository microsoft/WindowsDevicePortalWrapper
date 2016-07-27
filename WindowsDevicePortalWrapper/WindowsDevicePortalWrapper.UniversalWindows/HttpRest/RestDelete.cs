//----------------------------------------------------------------------------------------------
// <copyright file="RestDelete.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Security.Credentials;
using Windows.Storage.Streams;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <content>
    /// Universal Windows Platform implementation of HTTP Delete
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// Submits the http delete request to the specified uri.
        /// </summary>
        /// <param name="uri">The uri to which the delete request will be issued.</param>
        /// <returns>Task tracking HTTP completion</returns>
#pragma warning disable 1998
        private async Task<Stream> Delete(Uri uri)
        {
            IBuffer dataBuffer = null;

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
                this.ApplyHttpHeaders(client, HttpMethods.Delete);

                IAsyncOperationWithProgress<HttpResponseMessage, HttpProgress> responseOperation = client.DeleteAsync(uri);
                TaskAwaiter<HttpResponseMessage> responseAwaiter = responseOperation.GetAwaiter();
                while (!responseAwaiter.IsCompleted)
                { 
                }

                using (HttpResponseMessage response = responseOperation.GetResults())
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new DevicePortalException(response);
                    }

                    if (response.Content != null)
                    {
                        using (IHttpContent messageContent = response.Content)
                        {
                            IAsyncOperationWithProgress<IBuffer, ulong> bufferOperation = messageContent.ReadAsBufferAsync();
                            while (bufferOperation.Status != AsyncStatus.Completed)
                            {
                            }

                            dataBuffer = bufferOperation.GetResults();
                        }
                    }
                }
            }

            return (dataBuffer != null) ? dataBuffer.AsStream() : null;
        }
#pragma warning restore 1998
    }
}
