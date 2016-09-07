﻿//----------------------------------------------------------------------------------------------
// <copyright file="RestPut.cs" company="Microsoft Corporation">
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
    /// .net 4.x implementation of HTTP Put
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// Submits the http put request to the specified uri.
        /// </summary>
        /// <param name="uri">The uri to which the put request will be issued.</param>
        /// <param name="body">The HTTP content comprising the body of the request.</param>
        /// <returns>Task tracking the PUT completion.</returns>
        private async Task<Stream> Put(
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
                Task<HttpResponseMessage> putTask = client.PutAsync(uri, body);
                await putTask.ConfigureAwait(false);
                putTask.Wait();

                using (HttpResponseMessage response = putTask.Result)
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new DevicePortalException(response);
                    }

                    this.RetrieveCsrfToken(response);

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
