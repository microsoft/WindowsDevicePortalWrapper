//----------------------------------------------------------------------------------------------
// <copyright file="RestPost.cs" company="Microsoft Corporation">
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
    /// Universal Windows Platform implementation of HTTP Post
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// Submits the http post request to the specified uri.
        /// </summary>
        /// <param name="uri">The uri to which the post request will be issued.</param>
        /// <param name="requestStream">Optional stream containing data for the request body.</param>
        /// <param name="requestStreamContentType">The type of that request body data.</param>
        /// <returns>Task tracking the completion of the POST request</returns>
#pragma warning disable 1998
        private async Task<Stream> Post(
            Uri uri,
            Stream requestStream = null,
            string requestStreamContentType = null)
        {
            HttpStreamContent requestContent = null;
            IBuffer dataBuffer = null;

            if (requestStream != null)
            {
                requestContent = new HttpStreamContent(requestStream.AsInputStream());
                requestContent.Headers.Remove(ContentTypeHeaderName);
                requestContent.Headers.TryAppendWithoutValidation(ContentTypeHeaderName, requestStreamContentType);
            }

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
                this.ApplyHttpHeaders(client, HttpMethods.Post);

                IAsyncOperationWithProgress<HttpResponseMessage, HttpProgress> responseOperation = client.PostAsync(uri, requestContent);
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
