//----------------------------------------------------------------------------------------------
// <copyright file="RestPost.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <content>
    /// HTTP POST Wrapper
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// Calls the specified API with the provided body. This signature leaves
        /// off the optional response so callers who don't need a response body
        /// don't need to specify a type for it.
        /// </summary>
        /// <param name="apiPath">The relative portion of the uri path that specifies the API to call.</param>
        /// <param name="files">List of files that we want to include in the post request.</param>
        /// <param name="payload">The query string portion of the uri path that provides the parameterized data.</param>
        /// <returns>Task tracking the POST completion.</returns>
        private async Task Post(
            string apiPath,
            List<string> files,
            string payload = null)
        {
            string boundaryString = Guid.NewGuid().ToString();

            using (MemoryStream dataStream = new MemoryStream())
            {
                byte[] data;

                foreach (string file in files)
                {
                    FileInfo fi = new FileInfo(file);
                    data = Encoding.ASCII.GetBytes(string.Format("\r\n--{0}\r\n", boundaryString));
                    dataStream.Write(data, 0, data.Length);
                    CopyFileToRequestStream(fi, dataStream);
                }

                // Close the multipart request data.
                data = Encoding.ASCII.GetBytes(string.Format("\r\n--{0}--\r\n", boundaryString));
                dataStream.Write(data, 0, data.Length);

                dataStream.Position = 0;
                string contentType = string.Format("multipart/form-data; boundary={0}", boundaryString);

                await this.Post<NullResponse>(apiPath, payload, dataStream, contentType);
            }
        }

        /// <summary>
        /// Calls the specified API with the provided body. This signature leaves
        /// off the optional response so callers who don't need a response body
        /// don't need to specify a type for it.
        /// </summary>
        /// <param name="apiPath">The relative portion of the uri path that specifies the API to call.</param>
        /// <param name="payload">The query string portion of the uri path that provides the parameterized data.</param>
        /// <returns>Task tracking the POST completion.</returns>
        private async Task Post(
            string apiPath,
            string payload = null)
        {
            await this.Post<NullResponse>(apiPath, payload);
        }

        /// <summary>
        /// Calls the specified API with the provided payload.
        /// </summary>
        /// <typeparam name="T">The type of the data for the HTTP response body (if present).</typeparam>
        /// <param name="apiPath">The relative portion of the uri path that specifies the API to call.</param>
        /// <param name="payload">The query string portion of the uri path that provides the parameterized data.</param>
        /// <param name="requestStream">Optional stream containing data for the request body.</param>
        /// <param name="requestStreamContentType">The type of that request body data.</param>
        /// <returns>Task tracking the POST completion.</returns>
        private async Task<T> Post<T>(
            string apiPath,
            string payload = null,
            Stream requestStream = null,
            string requestStreamContentType = null) where T : new()
        {
            T data = default(T);

            Uri uri = Utilities.BuildEndpoint(
                this.deviceConnection.Connection,
                apiPath, 
                payload);

            DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(T));

            using (Stream dataStream = await this.Post(uri, requestStream, requestStreamContentType))
            {
                if ((dataStream != null) &&
                    (dataStream.Length != 0))
                {
                    JsonFormatCheck<T>(dataStream);

                    object response = deserializer.ReadObject(dataStream);
                    data = (T)response;
                }
            }

            return data;
        }
    }
}
