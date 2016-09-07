//----------------------------------------------------------------------------------------------
// <copyright file="RestPut.cs" company="Microsoft Corporation">
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
    /// Universal Windows Platform implementation of HTTP Put
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// Submits the http put request to the specified uri.
        /// </summary>
        /// <param name="uri">The uri to which the put request will be issued.</param>
        /// <param name="body">The HTTP content comprising the body of the request.</param>
        /// <param name="allowRetry">Allow the Post to be retried after issuing a Get call. Currently used for CSRF failures.</param>
        /// <returns>Task tracking the PUT completion.</returns>
#pragma warning disable 1998
        private async Task<Stream> Put(
            Uri uri,
            IHttpContent body = null,
            bool allowRetry = true)
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
                this.ApplyHttpHeaders(client, HttpMethods.Put);

                // Send the request
                IAsyncOperationWithProgress<HttpResponseMessage, HttpProgress> responseOperation = client.PutAsync(uri, null);
                TaskAwaiter<HttpResponseMessage> responseAwaiter = responseOperation.GetAwaiter();
                while (!responseAwaiter.IsCompleted)
                { 
                }

                using (HttpResponseMessage response = responseOperation.GetResults())
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        // If this isn't a retry and it failed due to a bad CSRF
                        // token, issue a GET to refresh the token and then retry.
                        if (allowRetry && this.IsBadCsrfToken(response))
                        {
                            await this.RefreshCsrfToken();
                            return await this.Put(uri, body, false);
                        }

                        throw new DevicePortalException(response);
                    }

                    this.RetrieveCsrfToken(response);

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
