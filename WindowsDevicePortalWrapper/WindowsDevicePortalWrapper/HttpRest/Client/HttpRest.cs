//----------------------------------------------------------------------------------------------
// <copyright file="HttpRest.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal
{
    internal static class HttpRest
    {
        /// <summary>
        /// Submits the http get request to the specified uri.
        /// </summary>
        /// <param name="client">The HTTP client to use.</param>
        /// <param name="uri">The uri to which the get request will be issued.</param>
        /// <returns>Response data as a stream.</returns>
        public static async Task<Stream> GetAsync(
            IHttpClient client,
            Uri uri)
        {
            MemoryStream dataStream = null;

            using (HttpResponseMessage response = await client.GetAsync(uri).ConfigureAwait(false))
            {
                if (!response.IsSuccessStatusCode)
                {
                    throw await DevicePortalException.CreateAsync(response);
                }

                if (response.Content == null)
                {
                    throw new DevicePortalException(System.Net.HttpStatusCode.NoContent, "", uri);
                }

                using (HttpContent content = response.Content)
                {
                    dataStream = new MemoryStream();

                    await content.CopyToAsync(dataStream).ConfigureAwait(false);

                    // Ensure we return with the stream pointed at the origin.
                    dataStream.Position = 0;
                }
            }

            return dataStream;
        }

        /// <summary>
        /// Submits the http post request to the specified uri.
        /// </summary>
        /// <param name="client">The HTTP client to use.</param>
        /// <param name="uri">The uri to which the post request will be issued.</param>
        /// <param name="requestContent">Optional content containing data for the request body.</param>
        /// <returns>Task tracking the completion of the POST request</returns>
        public static async Task<Stream> PostAsync(
            IHttpClient client,
            Uri uri,
            HttpContent requestContent)
        {
            MemoryStream responseDataStream = null;

            using (HttpResponseMessage response = await client.PostAsync(uri, requestContent).ConfigureAwait(false))
            {
                if (!response.IsSuccessStatusCode)
                {
                    throw await DevicePortalException.CreateAsync(response);
                }

                if (response.Content != null)
                {
                    using (HttpContent responseContent = response.Content)
                    {
                        responseDataStream = new MemoryStream();

                        await responseContent.CopyToAsync(responseDataStream).ConfigureAwait(false);

                        // Ensure we return with the stream pointed at the origin.
                        responseDataStream.Position = 0;
                    }
                }
            }

            return responseDataStream;
        }

        /// <summary>
        /// Submits the http put request to the specified uri.
        /// </summary>
        /// <param name="client">The HTTP client to use.</param>
        /// <param name="uri">The uri to which the put request will be issued.</param>
        /// <param name="body">The HTTP content comprising the body of the request.</param>
        /// <returns>Task tracking the PUT completion.</returns>
        public static async Task<Stream> PutAsync(
            IHttpClient client,
            Uri uri,
            HttpContent body = null)
        {
            MemoryStream dataStream = null;

            using (HttpResponseMessage response = await client.PutAsync(uri, body).ConfigureAwait(false))
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

                        await content.CopyToAsync(dataStream).ConfigureAwait(false);

                        // Ensure we return with the stream pointed at the origin.
                        dataStream.Position = 0;
                    }
                }
            }

            return dataStream;
        }

        /// <summary>
        /// Submits the http delete request to the specified uri.
        /// </summary>
        /// <param name="client">The HTTP client to use.</param>
        /// <param name="uri">The uri to which the delete request will be issued.</param>
        /// <returns>Task tracking HTTP completion</returns>
        public static async Task<Stream> DeleteAsync(
            IHttpClient client,
            Uri uri)
        {
            MemoryStream dataStream = null;

            using (HttpResponseMessage response = await client.DeleteAsync(uri).ConfigureAwait(false))
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

                        await content.CopyToAsync(dataStream).ConfigureAwait(false);

                        // Ensure we return with the stream pointed at the origin.
                        dataStream.Position = 0;
                    }
                }
            }

            return dataStream;
        }
    }
}
