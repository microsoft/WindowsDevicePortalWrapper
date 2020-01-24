//----------------------------------------------------------------------------------------------
// <copyright file="RestPost.cs" company="Microsoft Corporation">
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
    /// .net 4.x implementation of HTTP PostAsync
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
        public async Task<Stream> PostAsync(
            Uri uri,
            Stream requestStream = null,
            string requestStreamContentType = null)
        {
            StreamContent requestContent = null;
            
            if (requestStream != null)
            {
                requestContent = new StreamContent(requestStream);
                requestContent.Headers.Remove(ContentTypeHeaderName);
                requestContent.Headers.TryAddWithoutValidation(ContentTypeHeaderName, requestStreamContentType);
            }

            return await this.PostAsync(uri, requestContent);
        }

        /// <summary>
        /// Submits the http post request to the specified uri.
        /// </summary>
        /// <param name="uri">The uri to which the post request will be issued.</param>
        /// <param name="requestContent">Optional content containing data for the request body.</param>
        /// <returns>Task tracking the completion of the POST request</returns>
        public async Task<Stream> PostAsync(
            Uri uri,
            HttpContent requestContent)
        {
            MemoryStream responseDataStream = null;

            WebRequestHandler requestSettings = new WebRequestHandler();
            requestSettings.UseDefaultCredentials = false;
            requestSettings.Credentials = this.deviceConnection.Credentials;
            requestSettings.ServerCertificateValidationCallback = this.ServerCertificateValidation;

            using (HttpClient client = new HttpClient(requestSettings))
            {
                client.Timeout = TimeSpan.FromMilliseconds(-1);

                this.ApplyHttpHeaders(client, HttpMethods.Post);

                using (HttpResponseMessage response = await client.PostAsync(uri, requestContent).ConfigureAwait(false))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        throw await DevicePortalException.CreateAsync(response);
                    }

                    this.RetrieveCsrfToken(response);

                    if (response.Content != null)
                    {
                        using (HttpContent responseContent = response.Content)
                        {
                            responseDataStream = new MemoryStream();

                            await responseContent.CopyToAsync(responseDataStream).ConfigureAwait(false);

                            // Ensure we return with the stream pointed at the origin.
                            responseDataStream.Position = 0;
                            if (responseDataStream.Length == 0)
                            {
                                responseDataStream = null;
                            }
                        }
                    }
                }
            }

            return responseDataStream;
        }
    }
}
