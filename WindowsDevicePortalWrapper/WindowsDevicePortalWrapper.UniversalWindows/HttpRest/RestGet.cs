//----------------------------------------------------------------------------------------------
// <copyright file="RestGet.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.IO;
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
            requestSettings.ServerCredential = new PasswordCredential();
            requestSettings.ServerCredential.UserName = this.deviceConnection.Credentials.UserName;
            requestSettings.ServerCredential.Password = this.deviceConnection.Credentials.Password;

            using (HttpClient client = new HttpClient(requestSettings))
            {
                this.SetCrsfToken(client, "GET");

                IAsyncOperationWithProgress<HttpResponseMessage, HttpProgress> responseOperation = client.GetAsync(uri);
                while (responseOperation.Status != AsyncStatus.Completed)
                { 
                }

                using (HttpResponseMessage response = responseOperation.GetResults())
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new DevicePortalException(response);
                    }

                    // If the response sets a CSRF token, store that for future requests
                    string cookie;
                    bool hasCookie = response.Headers.TryGetValue("Set-Cookie", out cookie);

                    if (hasCookie)
                    {
                        string csrfTokenNameWithEquals = CsrfTokenName + "=";
                        if (cookie.StartsWith(csrfTokenNameWithEquals))
                        {
                            this.csrfToken = cookie.Substring(csrfTokenNameWithEquals.Length);
                        }
                    }

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

            return null;
        }
#pragma warning restore 1998
    }
}