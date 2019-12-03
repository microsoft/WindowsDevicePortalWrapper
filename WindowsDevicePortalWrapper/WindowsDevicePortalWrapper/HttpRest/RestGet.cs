//----------------------------------------------------------------------------------------------
// <copyright file="RestGet.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <content>
    /// .net 4.x implementation of HTTP GetAsync
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// Submits the http get request to the specified uri.
        /// </summary>
        /// <param name="uri">The uri to which the get request will be issued.</param>
        /// <returns>Response data as a stream.</returns>
        public async Task<Stream> GetAsync(
            Uri uri)
        {
            HttpClient client = null;
            HttpResponseMessage response = null;
            try
            {
                WebRequestHandler handler = new WebRequestHandler();
                handler.UseDefaultCredentials = false;
                handler.Credentials = this.deviceConnection.Credentials;
                handler.ServerCertificateValidationCallback = this.ServerCertificateValidation;

                client = new HttpClient(handler);
                this.ApplyHttpHeaders(client, HttpMethods.Get);

                response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    throw await DevicePortalException.CreateAsync(response);
                }

                this.RetrieveCsrfToken(response);

                if (response.Content == null)
                {
                    throw new DevicePortalException(System.Net.HttpStatusCode.NoContent, "", uri);
                }

                return await response.Content.ReadAsStreamAsync();
            }
            catch (Exception)
            {
                response?.Dispose();
                client?.Dispose();
                throw;
            }
        }
    }
}
