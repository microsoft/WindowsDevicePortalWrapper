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
        /// Calls the specified API with the provided payload.
        /// </summary>
        /// <param name="apiPath">The relative portion of the uri path that specifies the API to call.</param>
        /// <param name="payload">The query string portion of the uri path that provides the parameterized data.</param>
        /// <returns>Task tracking the PUT completion.</returns>
        private async Task Put(
            string apiPath,
            string payload = null)
        {
            Uri uri = Utilities.BuildEndpoint(
                this.deviceConnection.Connection,
                apiPath, 
                payload);

            await this.Put(uri);
        }

        /// <summary>
        /// Calls the specified API with the provided body.
        /// </summary>
        /// <typeparam name="T">The type of the data for the HTTP request body.</typeparam>
        /// <param name="apiPath">The relative portion of the uri path that specifies the API to call.</param>
        /// <param name="bodyData">The data to be used for the HTTP request body.</param>
        /// <param name="payload">The query string portion of the uri path that provides the parameterized data.</param>
        /// <returns>Task tracking the PUT completion.</returns>
        private async Task Put<T>(
            string apiPath,
            T bodyData,
            string payload = null)
        {
            Uri uri = Utilities.BuildEndpoint(
                this.deviceConnection.Connection,
                apiPath, 
                payload);

            // Serialize the body to a JSON stream
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
            Stream stream = new MemoryStream();
            serializer.WriteObject(stream, bodyData);

            stream.Seek(0, SeekOrigin.Begin);
#if WINDOWS_UWP
            HttpStreamContent streamContent = new HttpStreamContent(stream.AsInputStream());
            streamContent.Headers.ContentType = new HttpMediaTypeHeaderValue("application/json");
#else
            StreamContent streamContent = new StreamContent(stream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
#endif // WINDOWS_UWP

            await this.Put(uri, streamContent);
        }
    }
}
