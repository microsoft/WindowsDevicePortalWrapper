//----------------------------------------------------------------------------------------------
// <copyright file="DefaultHttpWrapper.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <summary>
    /// Default HTTP Wrapper class passing through directly to the HttpClient.
    /// </summary>
    public class DefaultHttpWrapper : IHttpWrapper
    {
        /// <summary>
        /// Abstract method Implementation (pass-through to HttpClient)
        /// </summary>
        /// <param name="client">HTTP Client object.</param>
        /// <param name="uri">The target URI.</param>
        /// <returns>Async task returning the response.</returns>
        public async Task<HttpResponseMessage> GetAsync(HttpClient client, Uri uri)
        {
            return await client.GetAsync(uri);
        }

        /// <summary>
        /// Abstract method Implementation (pass-through to HttpClient)
        /// </summary>
        /// <param name="client">HTTP Client object.</param>
        /// <param name="uri">The target URI.</param>
        /// <param name="content">The HTTP body of the request.</param>
        /// <returns>Async task returning the response.</returns>
        public async Task<HttpResponseMessage> PostAsync(HttpClient client, Uri uri, HttpContent content)
        {
            return await client.PostAsync(uri, content);
        }

        /// <summary>
        /// Abstract method Implementation (pass-through to HttpClient)
        /// </summary>
        /// <param name="client">HTTP Client object.</param>
        /// <param name="uri">The target URI.</param>
        /// <param name="content">The HTTP body of the request.</param>
        /// <returns>Async task returning the response.</returns>
        public async Task<HttpResponseMessage> PutAsync(HttpClient client, Uri uri, HttpContent content)
        {
            return await client.PutAsync(uri, content);
        }

        /// <summary>
        /// Abstract method Implementation (pass-through to HttpClient)
        /// </summary>
        /// <param name="client">HTTP Client object.</param>
        /// <param name="uri">The target URI.</param>
        /// <returns>Async task returning the response.</returns>
        public async Task<HttpResponseMessage> DeleteAsync(HttpClient client, Uri uri)
        {
            return await client.DeleteAsync(uri);
        }
    }
}
