//----------------------------------------------------------------------------------------------
// <copyright file="RestPut.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.IO;
#if !WINDOWS_UWP
using System.Net.Http;
using System.Net.Http.Headers;
#endif // !WINDOWS_UWP
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
#if WINDOWS_UWP
using Windows.Foundation;
using Windows.Networking;
using Windows.Security.Credentials;
using Windows.Storage.Streams;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using Windows.Web.Http.Headers;
#endif // WINDOWS_UWP

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

#if WINDOWS_UWP
            HttpStreamContent streamContent = null;
#else
            StreamContent streamContent = null;
#endif // WINDOWS_UWP

            if (bodyData != null)
            {
                // Serialize the body to a JSON stream
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(K));
                Stream stream = new MemoryStream();
                serializer.WriteObject(stream, bodyData);

                stream.Seek(0, SeekOrigin.Begin);
#if WINDOWS_UWP
                streamContent = new HttpStreamContent(stream.AsInputStream());
                streamContent.Headers.ContentType = new HttpMediaTypeHeaderValue("application/json");
#else
                streamContent = new StreamContent(stream);
                streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
#endif // WINDOWS_UWP
            }

            return ReadJsonStream<T>(await this.PutAsync(uri, streamContent));
        }
    }
}
