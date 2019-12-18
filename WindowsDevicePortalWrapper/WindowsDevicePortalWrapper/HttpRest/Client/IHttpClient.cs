//----------------------------------------------------------------------------------------------
// <copyright file="IHttpClient.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <summary>
    /// An interface corresponding to <see cref="HttpClient"/>.
    /// </summary>
    public interface IHttpClient
    {
        /// <summary>
        /// Submits the http get request to the specified uri.
        /// </summary>
        /// <param name="uri">The uri to which the get request will be issued.</param>
        /// <returns>Response data message.</returns>
        Task<HttpResponseMessage> GetAsync(Uri uri);

        /// <summary>
        /// Submits the http post request to the specified uri.
        /// </summary>
        /// <param name="uri">The uri to which the post request will be issued.</param>
        /// <param name="requestContent">Optional content containing data for the request body.</param>
        /// <returns>Task tracking the completion of the POST request</returns>
        Task<HttpResponseMessage> PostAsync(Uri uri, HttpContent content);

        /// <summary>
        /// Submits the http put request to the specified uri.
        /// </summary>
        /// <param name="uri">The uri to which the put request will be issued.</param>
        /// <param name="body">The HTTP content comprising the body of the request.</param>
        /// <returns>Task tracking the PUT completion.</returns>
        Task<HttpResponseMessage> PutAsync(Uri uri, HttpContent content);

        /// <summary>
        /// Submits the http delete request to the specified uri.
        /// </summary>
        /// <param name="uri">The uri to which the delete request will be issued.</param>
        /// <returns>Task tracking HTTP completion</returns>
        Task<HttpResponseMessage> DeleteAsync(Uri uri);
    }
}
