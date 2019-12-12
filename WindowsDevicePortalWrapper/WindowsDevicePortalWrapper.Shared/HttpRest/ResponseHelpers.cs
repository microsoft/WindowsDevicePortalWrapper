//----------------------------------------------------------------------------------------------
// <copyright file="ResponseHelpers.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text.RegularExpressions;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <content>
    /// Methods for working with Http responses.
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
        /// Reads dataStream as T.
        /// </summary>
        /// <typeparam name="T">Return type for the JSON message</typeparam>
        /// <param name="dataStream">The stream that contains the JSON message to be checked.</param>
        /// <param name="settings">Optional settings for JSON serialization.</param>
        /// <returns>Read data</returns>
        public static T ReadJsonStream<T>(Stream dataStream, DataContractJsonSerializerSettings settings = null)
        {
            T data = default(T);
            object response = null;
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T), settings);

            using (dataStream)
            {
                if (dataStream != null)
                {
                    using (MemoryStream outStream = new MemoryStream())
                    {
                        dataStream.CopyTo(outStream);
                        if (outStream.Length != 0)
                        {
                            outStream.Seek(0, SeekOrigin.Begin);
                            JsonFormatCheck<T>(outStream);

                            try
                            {
                                response = serializer.ReadObject(outStream);
                            }
                            catch (SerializationException ex)
                            {
                                // Assert on serialization failure.
                                Debug.Assert(false, "Serialization failure encountered. Check DataContract types for a possible mismatch between expectations and reality");
                                throw;
                            }

                            data = (T)response;
                        }
                    }
                }
            }

            return data;
        }

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

        #region Data contract

        /// <summary>
        /// A null response class when we don't care about the response
        /// body.
        /// </summary>
        [DataContract]
        private class NullResponse
        {
        }

        #endregion
    }
}
