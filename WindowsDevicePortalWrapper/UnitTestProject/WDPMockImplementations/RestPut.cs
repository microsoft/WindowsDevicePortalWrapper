//----------------------------------------------------------------------------------------------
// <copyright file="RestPut.cs" company="Microsoft Corporation">
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
    /// MOCK implementation of HTTP PUT
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// Submits the http put request to the specified uri.
        /// </summary>
        /// <param name="uri">The uri to which the put request will be issued.</param>
        /// <param name="body">The HTTP content comprising the body of the request.</param>
        /// <returns>Task tracking the PUT completion.</returns>
        private async Task<Stream> PutAsync(
            Uri uri,
            HttpContent body = null)
        {
            MemoryStream dataStream = null;

            WebRequestHandler requestSettings = new WebRequestHandler();
            requestSettings.UseDefaultCredentials = false;
            requestSettings.Credentials = this.deviceConnection.Credentials;
            requestSettings.ServerCertificateValidationCallback = this.ServerCertificateValidation;

            using (HttpClient client = new HttpClient(requestSettings))
            {
                this.ApplyHttpHeaders(client, HttpMethods.Put);

                // Send the request
                Task<HttpResponseMessage> putTask = TestHelpers.MockHttpResponder.PutAsync(uri, body);
                await putTask.ConfigureAwait(false);
                putTask.Wait();

                using (HttpResponseMessage response = putTask.Result)
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
