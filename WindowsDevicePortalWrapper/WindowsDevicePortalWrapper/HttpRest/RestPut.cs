//----------------------------------------------------------------------------------------------
// <copyright file="RestPut.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <content>
    /// HTTP PUT Wrapper
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// Calls the specified API with the provided body. This signature leaves
        /// off the optional response so callers who don't need a response body
        /// don't need to specify a type for it, which also would force them
        /// to explicitly declare their bodyData type instead of letting it
        /// be implied implicitly.
        /// </summary>
        /// <typeparam name="K">The type of the data for the HTTP request body.</typeparam>
        /// <param name="apiPath">The relative portion of the uri path that specifies the API to call.</param>
        /// <param name="bodyData">The data to be used for the HTTP request body.</param>
        /// <param name="payload">The query string portion of the uri path that provides the parameterized data.</param>
        /// <returns>Task tracking the PUT completion.</returns>
        public async Task PutAsync<K>(
            string apiPath,
            K bodyData,
            string payload = null) where K : class
        {
            await this.PutAsync<NullResponse, K>(apiPath, bodyData, payload);
        }

        /// <summary>
        /// Calls the specified API with the provided body.
        /// </summary>
        /// <typeparam name="T">The type of the data for the HTTP response body (if present).</typeparam>
        /// <typeparam name="K">The type of the data for the HTTP request body.</typeparam>
        /// <param name="apiPath">The relative portion of the uri path that specifies the API to call.</param>
        /// <param name="bodyData">The data to be used for the HTTP request body.</param>
        /// <param name="payload">The query string portion of the uri path that provides the parameterized data.</param>
        /// <returns>Task tracking the PUT completion, optional response body.</returns>
        public async Task<T> PutAsync<T, K>(
            string apiPath,
            K bodyData = null,
            string payload = null) where T : new()
                                   where K : class
        {
            Uri uri = Utilities.BuildEndpoint(
                this.deviceConnection.Connection,
                apiPath,
                payload);

            StreamContent streamContent = null;
            if (bodyData != null)
            {
                // Serialize the body to a JSON stream
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(K));
                Stream stream = new MemoryStream();
                serializer.WriteObject(stream, bodyData);

                stream.Seek(0, SeekOrigin.Begin);
                streamContent = new StreamContent(stream);
                streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            }

            return (await this.PutAsync(uri, streamContent)).ReadJson<T>();
        }

        /// <summary>
        /// Submits the http put request to the specified uri.
        /// </summary>
        /// <param name="uri">The uri to which the put request will be issued.</param>
        /// <param name="body">The HTTP content comprising the body of the request.</param>
        /// <returns>Task tracking the PUT completion.</returns>
        public Task<Stream> PutAsync(
            Uri uri,
            HttpContent body = null) => HttpRest.PutAsync(this.HttpClient, uri, body);
    }
}
