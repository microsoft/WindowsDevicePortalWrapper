//----------------------------------------------------------------------------------------------
// <copyright file="RestDelete.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <content>
    /// HTTP DELETE Wrapper
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// Calls the specified API with the provided payload. This signature leaves
        /// off the optional response so callers who don't need a response body
        /// don't need to specify a type for it.
        /// </summary>
        /// <param name="apiPath">The relative portion of the uri path that specifies the API to call.</param>
        /// <param name="payload">The query string portion of the uri path that provides the parameterized data.</param>
        /// <returns>Task tracking the HTTP completion.</returns>
        private async Task Delete(
            string apiPath,
            string payload = null)
        {
            await this.Delete<NullResponse>(apiPath, payload);
        }

        /// <summary>
        /// Calls the specified API with the provided payload.
        /// </summary>
        /// <typeparam name="T">The type of the data for the HTTP response body (if present).</typeparam>
        /// <param name="apiPath">The relative portion of the uri path that specifies the API to call.</param>
        /// <param name="payload">The query string portion of the uri path that provides the parameterized data.</param>
        /// <param name="allowRetry">Allow the Delete to be retried after refreshing the CSRF token.</param>
        /// <returns>Task tracking the HTTP completion.</returns>
        private async Task<T> Delete<T>(
            string apiPath,
            string payload = null,
            bool allowRetry = true) where T : new()
        {
            T data = default(T);

            Uri uri = Utilities.BuildEndpoint(
                this.deviceConnection.Connection,
                apiPath, 
                payload);

            DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(T));

            try
            {
                using (Stream dataStream = await this.Delete(uri))
                {
                    if ((dataStream != null) &&
                        (dataStream.Length != 0))
                    {
                        JsonFormatCheck<T>(dataStream);

                        object response = deserializer.ReadObject(dataStream);
                        data = (T)response;
                    }
                }
            }
            catch (DevicePortalException e)
            {
                // If this isn't a retry and it failed due to a bad CSRF
                // token, refresh the token and then retry.
                if (allowRetry && this.IsBadCsrfToken(e))
                {
                    await this.RefreshCsrfToken();
                    return await this.Delete<T>(apiPath, payload, false);
                }
                else
                {
                    throw e;
                }
            }


            return data;
        }
    }
}
