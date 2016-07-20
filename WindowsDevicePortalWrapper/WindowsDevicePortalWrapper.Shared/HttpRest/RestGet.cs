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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <content>
    /// HTTP GET Wrapper
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// The prefix for the <see cref="SystemPerformanceInformation" /> JSON formatting error.
        /// </summary>
        private static readonly string SysPerfInfoErrorPrefix = "{\"Reason\" : \"";

        /// <summary>
        /// The postfix for the <see cref="SystemPerformanceInformation" /> JSON formatting error.
        /// </summary>
        private static readonly string SysPerfInfoErrorPostfix = "\"}";

        /// <summary>
        /// Checks the JSON for any known formatting errors and fixes them.
        /// </summary>
        /// <typeparam name="T">Return type for the JSON message</typeparam>
        /// <param name="jsonStream">The stream that contains the JSON message to be checked.</param>
        private static void JsonFormatCheck<T>(Stream jsonStream)
        {
            if (typeof(T) == typeof(SystemPerformanceInformation))
            {
                StreamReader read = new StreamReader(jsonStream);
                string rawJsonString = read.ReadToEnd();

                // Recover from an error in which SystemPerformanceInformation is returned with an incorrect prefix, postfix and the message converted into JSON a second time.
                if (rawJsonString.StartsWith(SysPerfInfoErrorPrefix, StringComparison.OrdinalIgnoreCase) && rawJsonString.EndsWith(SysPerfInfoErrorPostfix, StringComparison.OrdinalIgnoreCase))
                {
                    // Remove the incorrect prefix and postfix from the JSON message.
                    rawJsonString = rawJsonString.Substring(SysPerfInfoErrorPrefix.Length, rawJsonString.Length - SysPerfInfoErrorPrefix.Length - SysPerfInfoErrorPostfix.Length);

                    // Undo the second JSON conversion.
                    rawJsonString = Regex.Replace(rawJsonString, "\\\\\"", "\"", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                    rawJsonString = Regex.Replace(rawJsonString, "\\\\\\\\", "\\", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

                    // Overwrite the stream with the fixed JSON.
                    jsonStream.SetLength(0);
                    var sw = new StreamWriter(jsonStream);
                    sw.Write(rawJsonString);
                    sw.Flush();
                }

                jsonStream.Seek(0, SeekOrigin.Begin);
            }
        }

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
                if ((dataStream != null) &&
                    (dataStream.Length != 0))
                {
                    JsonFormatCheck<T>(dataStream);
 
                    object response = serializer.ReadObject(dataStream);
                    data = (T)response;
                }
            }

            return data;
        }
    }
}
