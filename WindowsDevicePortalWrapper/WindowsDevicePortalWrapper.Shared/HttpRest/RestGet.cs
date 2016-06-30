//----------------------------------------------------------------------------------------------
// <copyright file="RestGet.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <content>
    /// HTTP GET Wrapper
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// Submits the http get request to the specified uri.
        /// </summary>
        /// <param name="uri">The uri to which the get request will be issued.</param>
        /// /// <param name="validateCertificate">Whether the certificate should be validated.</param>
        /// <returns>Response data as a stream.</returns>
        private async Task<Stream> Get(Uri uri, bool validateCertificate = true)
        {
            MemoryStream dataStream = null;

            WebRequestHandler handler = new WebRequestHandler();
            handler.UseDefaultCredentials = false;
            handler.Credentials = this.deviceConnection.Credentials;
            if (validateCertificate)
            {
                handler.ServerCertificateValidationCallback = this.ServerCertificateValidation;
            }
            else
            {
                handler.ServerCertificateValidationCallback = this.ServerCertificateNonValidation;
            }

            using (HttpClient client = new HttpClient(handler))
            {
                HttpRequestHeaders headers = client.DefaultRequestHeaders;

                if (string.IsNullOrEmpty(this.csrfToken))
                {
                    headers.Add(CsrfTokenName, "Fetch");
                }
                else
                {
                    headers.Add(CsrfTokenName, this.csrfToken);
                }

                Task<HttpResponseMessage> getTask = client.GetAsync(uri);
                await getTask.ConfigureAwait(false);
                getTask.Wait();

                using (HttpResponseMessage response = getTask.Result)
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new DevicePortalException(response);
                    }

                    // If the response sets a CSRF token, store that for future requests
                    IEnumerable<string> cookies;
                    bool hasCookies = response.Headers.TryGetValues("Set-Cookie", out cookies);

                    if (hasCookies)
                    {
                        foreach (string cookie in cookies)
                        {
                            string csrfTokenNameWithEquals = CsrfTokenName + "=";
                            if (cookie.StartsWith(csrfTokenNameWithEquals))
                            {
                                this.csrfToken = cookie.Substring(csrfTokenNameWithEquals.Length);
                            }
                        }
                    }

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

            return dataStream;
        }

        /// <summary>
        /// Calls the specified API with the provided payload.
        /// </summary>
        /// <typeparam name="T">Return type for the GET call</typeparam>
        /// <param name="apiPath">The relative portion of the uri path that specifies the API to call.</param>
        /// <param name="payload">The query string portion of the uri path that provides the parameterized data.</param>
        /// <returns>An object of the specified type containing the data returned by the request.</returns>
        private async Task<T> Get<T>(
            string apiPath,
            string payload = null) where T : new()
        {
            T data = default(T);
            
            Uri uri = Utilities.BuildEndpoint(
                this.deviceConnection.Connection,
                apiPath, 
                payload);

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));

            using (Stream dataStream = await this.Get(uri))
            {
                if (dataStream != null)
                {
                    object response = serializer.ReadObject(dataStream);
                    data = (T)response;
                }
            }

            return data;
        }
    }
}
