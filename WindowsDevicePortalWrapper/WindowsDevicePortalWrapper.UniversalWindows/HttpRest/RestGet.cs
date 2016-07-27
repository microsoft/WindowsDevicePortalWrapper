//----------------------------------------------------------------------------------------------
// <copyright file="RestGet.cs" company="Microsoft Corporation">
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
    /// Universal Windows Platform implementation of HTTP Get
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// Submits the http get request to the specified uri.
        /// </summary>
        /// <param name="uri">The uri to which the get request will be issued.</param>
        /// <returns>Response data as a stream.</returns>
#pragma warning disable 1998
        private async Task<Stream> Get(Uri uri)
        {
            IBuffer dataBuffer = null;

            HttpBaseProtocolFilter requestSettings = new HttpBaseProtocolFilter();
            requestSettings.AllowUI = false;

            if (this.deviceConnection.Credentials != null)
            {
                requestSettings.ServerCredential = new PasswordCredential();
                requestSettings.ServerCredential.UserName = this.deviceConnection.Credentials.UserName;
                requestSettings.ServerCredential.Password = this.deviceConnection.Credentials.Password;
            }

            using (HttpClient client = new HttpClient(requestSettings))
            {
                this.ApplyHttpHeaders(client, HttpMethods.Get);

                IAsyncOperationWithProgress<HttpResponseMessage, HttpProgress> responseOperation = client.GetAsync(uri);
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

                    this.RetrieveCsrfToken(response);

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

            return (dataBuffer != null) ? dataBuffer.AsStream() : null;
        }
#pragma warning restore 1998
    }
}