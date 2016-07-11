//----------------------------------------------------------------------------------------------
// <copyright file="RestPost.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <content>
    /// HTTP POST Wrapper
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// Calls the specified API with the provided payload.
        /// </summary>
        /// <param name="apiPath">The relative portion of the uri path that specifies the API to call.</param>
        /// <param name="payload">The query string portion of the uri path that provides the parameterized data.</param>
        /// <returns>Task tracking the POST completion.</returns>
        private async Task Post(
            string apiPath,
            string payload = null)
        {
            Uri uri = Utilities.BuildEndpoint(
                this.deviceConnection.Connection,
                apiPath, 
                payload);

            await this.Post(uri);
        }
    }
}
