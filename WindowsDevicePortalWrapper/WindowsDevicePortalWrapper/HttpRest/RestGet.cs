//----------------------------------------------------------------------------------------------
// <copyright file="RestGet.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <content>
    /// HTTP GET Wrapper
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// Calls the specified API with the provided payload.
        /// </summary>
        /// <typeparam name="T">Return type for the GET call</typeparam>
        /// <param name="apiPath">The relative portion of the uri path that specifies the API to call.</param>
        /// <param name="payload">The query string portion of the uri path that provides the parameterized data.</param>
        /// <returns>An object of the specified type containing the data returned by the request.</returns>
        public async Task<T> GetAsync<T>(
            string apiPath,
            string payload = null) where T : new()
        {
            Uri uri = Utilities.BuildEndpoint(
                this.deviceConnection.Connection,
                apiPath,
                payload);

            return (await this.GetAsync(uri).ConfigureAwait(false)).ReadJson<T>();
        }

        /// <summary>
        /// Submits the http get request to the specified uri.
        /// </summary>
        /// <param name="uri">The uri to which the get request will be issued.</param>
        /// <returns>Response data as a stream.</returns>
        public Task<Stream> GetAsync(Uri uri) => HttpRest.GetAsync(this.HttpClient, uri);
    }
}
