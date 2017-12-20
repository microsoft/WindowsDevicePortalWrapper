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
    /// Universal Windows Platform implementation of HTTP GetAsync
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// Submits the http get request to the specified uri.
        /// </summary>
        /// <param name="uri">The uri to which the get request will be issued.</param>
        /// <returns>Response data as a stream.</returns>
#pragma warning disable 1998
        public async Task<Stream> GetAsync(Uri uri)
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
                using (HttpResponseMessage response = await client.GetAsync(uri))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        throw await DevicePortalException.CreateAsync(response);
                    }

                    this.RetrieveCsrfToken(response);

                    using (IHttpContent messageContent = response.Content)
                    {
                        dataBuffer = await messageContent.ReadAsBufferAsync();
                    }
                }
            }

            return (dataBuffer != null) ? dataBuffer.AsStream() : null;
        }
#pragma warning restore 1998
    }
}