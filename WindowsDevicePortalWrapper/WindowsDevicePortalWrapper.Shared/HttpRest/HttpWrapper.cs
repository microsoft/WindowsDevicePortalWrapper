//----------------------------------------------------------------------------------------------
// <copyright file="HttpWrapper.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <summary>
    /// HTTP Wrapper class to enable mocks at the HTTP layer.
    /// </summary>
    public abstract class HttpWrapper
    {
        /// <summary>
        /// Wrapper for HTTP GetAsync method.
        /// </summary>
        /// <param name="client">HTTP Client object.</param>
        /// <param name="uri">The target URI.</param>
        /// <returns>Async task returning the response.</returns>
        public abstract Task<HttpResponseMessage> GetAsync(HttpClient client, Uri uri);

        /// <summary>
        /// Wrapper for HTTP PostAsync method.
        /// </summary>
        /// <param name="client">HTTP Client object.</param>
        /// <param name="uri">The target URI.</param>
        /// <param name="content">The HTTP body of the request.</param>
        /// <returns>Async task returning the response.</returns>
        public abstract Task<HttpResponseMessage> PostAsync(HttpClient client, Uri uri, HttpContent content);

        /// <summary>
        /// Wrapper for HTTP PutAsync method.
        /// </summary>
        /// <param name="client">HTTP Client object.</param>
        /// <param name="uri">The target URI.</param>
        /// <param name="content">The HTTP body of the request.</param>
        /// <returns>Async task returning the response.</returns>
        public abstract Task<HttpResponseMessage> PutAsync(HttpClient client, Uri uri, HttpContent content);

        /// <summary>
        /// Wrapper for HTTP DeleteAsync method.
        /// </summary>
        /// <param name="client">HTTP Client object.</param>
        /// <param name="uri">The target URI.</param>
        /// <returns>Async task returning the response.</returns>
        public abstract Task<HttpResponseMessage> DeleteAsync(HttpClient client, Uri uri);
    }
}
