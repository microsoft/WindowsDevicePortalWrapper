//----------------------------------------------------------------------------------------------
// <copyright file="RestPost.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Tools.WindowsDevicePortal.Tests;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <content>
    /// MOCK implementation of HTTP PostAsync
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
        private Task<Stream> PostAsync(
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

            return this.PostAsync(uri, requestContent);
        }

        /// <summary>
        /// Submits the http post request to the specified uri.
        /// </summary>
        /// <param name="uri">The uri to which the post request will be issued.</param>
        /// <param name="requestContent">Optional content for the request body.</param>
        /// <returns>Task tracking the completion of the POST request</returns>
        private async Task<Stream> PostAsync(
            Uri uri,
            HttpContent requestContent)
        {
            MemoryStream dataStream = null;
            WebRequestHandler requestSettings = new WebRequestHandler();
            requestSettings.UseDefaultCredentials = false;
            requestSettings.Credentials = this.deviceConnection.Credentials;
            requestSettings.ServerCertificateValidationCallback = this.ServerCertificateValidation;

            using (HttpClient client = new HttpClient(requestSettings))
            {
                this.ApplyHttpHeaders(client, HttpMethods.Post);

                Task<HttpResponseMessage> postTask = TestHelpers.MockHttpResponder.PostAsync(uri, requestContent);
                await postTask.ConfigureAwait(false);
                postTask.Wait();

                using (HttpResponseMessage response = postTask.Result)
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        throw await DevicePortalException.CreateAsync(response);
                    }

                    if (response.Content != null)
                    {
                        using (HttpContent content = response.Content)
                        {
                            dataStream = new MemoryStream();

                            Task copyTask = content.CopyToAsync(dataStream);
                            await copyTask.ConfigureAwait(false);
                            copyTask.Wait();

                            // Ensure we return with the stream pointed at the origin.
                            dataStream.Position = 0;
                        }
                    }
                }
            }

            return dataStream;
        }
    }
}
