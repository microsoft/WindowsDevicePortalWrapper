//----------------------------------------------------------------------------------------------
// <copyright file="RestDelete.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <content>
    /// .net 4.x implementation of HTTP DeleteAsync
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// Submits the http delete request to the specified uri.
        /// </summary>
        /// <param name="uri">The uri to which the delete request will be issued.</param>
        /// <returns>Task tracking HTTP completion</returns>
        public async Task<Stream> DeleteAsync(Uri uri)
        {
            MemoryStream dataStream = null;

            WebRequestHandler requestSettings = new WebRequestHandler();
            requestSettings.UseDefaultCredentials = false;
            requestSettings.Credentials = this.deviceConnection.Credentials;
            requestSettings.ServerCertificateValidationCallback = this.ServerCertificateValidation;

            using (HttpClient client = new HttpClient(requestSettings))
            {
                this.ApplyHttpHeaders(client, HttpMethods.Delete);

                using (HttpResponseMessage response = await client.DeleteAsync(uri).ConfigureAwait(false))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        throw await DevicePortalException.CreateAsync(response);
                    }

                    this.RetrieveCsrfToken(response);

                    if (response.Content != null)
                    {
                        using (HttpContent content = response.Content)
                        {
                            dataStream = new MemoryStream();

                            await content.CopyToAsync(dataStream).ConfigureAwait(false);

                            // Ensure we return with the stream pointed at the origin.
                            dataStream.Position = 0;
                            if (dataStream.Length == 0)
                            {
                                dataStream = null;
                            }
                        }
                    }
                }
            }

            return dataStream;
        }
    }
}
