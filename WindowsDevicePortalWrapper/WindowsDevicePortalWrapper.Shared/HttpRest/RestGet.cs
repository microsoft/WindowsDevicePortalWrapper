//----------------------------------------------------------------------------------------------
// <copyright file="RestGet.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
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
        private async Task<T> Get<T>(
            string apiPath,
            string payload = null) where T : new()
        {
            T data = default(T);
            
            Uri uri = Utilities.BuildEndpoint(
                this.deviceConnection.Connection,
                apiPath, 
                payload);

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));

            using (Stream dataStream = await this.Get(uri))
            {
                if (dataStream != null)
                {
                    object response = serializer.ReadObject(dataStream);
                    data = (T)response;
                }
            }

            return data;
        }
    }
}
