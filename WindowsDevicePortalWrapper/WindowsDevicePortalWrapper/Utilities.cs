//----------------------------------------------------------------------------------------------
// <copyright file="Utilities.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <summary>
    /// Utility class for common functions
    /// </summary>
    internal static class Utilities
    {
        /// <summary>
        /// Constructs a fully formed REST API endpoint uri.
        /// </summary>
        /// <param name="baseUri">The base uri (typically, just scheme and authority).</param>
        /// <param name="path">The path to the REST API method (ex: api/control/restart).</param>
        /// <param name="payload">Parameterized data required by the REST API.</param>
        /// <returns>Uri object containing the complete path and query string required to issue the REST API call.</returns>
        public static Uri BuildEndpoint(
            Uri baseUri,
            string path,
            string payload = null)
        {
            string relativePart = !string.IsNullOrWhiteSpace(payload) ?
                                    string.Format("{0}?{1}", path, payload) : path;
            return new Uri(baseUri, relativePart);
        }

        /// <summary>
        /// Encodes the specified string as a base64 value.
        /// </summary>
        /// <param name="str">The string to encode.</param>
        /// <returns>Base64 encoded version of the string data.</returns>
        internal static string Hex64Encode(string str)
        {
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(str));
        }
    }
}
