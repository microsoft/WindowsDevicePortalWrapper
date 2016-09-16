//----------------------------------------------------------------------------------------------
// <copyright file="Utilities.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using static Microsoft.Tools.WindowsDevicePortal.DevicePortal;

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
        /// Builds a query string from key value pairs.
        /// </summary>
        /// <param name="payload">The key value pairs containing the query parameters.</param>
        /// <returns>Properly formatted query string.</returns>
        public static string BuildQueryString(Dictionary<string, string> payload)
        {
            string query = string.Empty;

            foreach (KeyValuePair<string, string> pair in payload)
            {
                query += pair.Key + "=" + pair.Value + "&";
            }

            // Trim off the final ampersand.
            query = query.Trim('&');

            return query;
        }

        /// <summary>
        /// Encodes the specified string as a base64 value.
        /// </summary>
        /// <param name="str">The string to encode.</param>
        /// <returns>Base64 encoded version of the string data.</returns>
        public static string Hex64Encode(string str)
        {
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(str));
        }

        /// <summary>
        /// Checks if this device is a hololens.
        /// </summary>
        /// <param name="platform">The platform.</param>
        /// <param name="deviceFamily">The device family.</param>
        /// <returns>Whether this is a hololens.</returns>
        public static bool IsHoloLens(
            DevicePortalPlatforms platform,
            string deviceFamily)
        {
            bool isHoloLens = false;

            if ((platform == DevicePortalPlatforms.HoloLens) ||
                ((platform == DevicePortalPlatforms.VirtualMachine) && (deviceFamily == "Windows.Holographic")))
            {
                isHoloLens = true;
            }

            return isHoloLens;
        }

        /// <summary>
        /// Modifies an endpoint to match the way we store it in a file.
        /// This involves replacing a number of characters with underscores.
        /// </summary>
        /// <param name="endpoint">The endpoint that is being modified.</param>
        public static void ModifyEndpointForFilename(ref string endpoint)
        {
            char[] invalidChars = Path.GetInvalidFileNameChars();

            foreach (char character in invalidChars)
            {
                endpoint = endpoint.Replace(character, '_');
            }

            endpoint = endpoint.Replace('-', '_');
            endpoint = endpoint.Replace('.', '_');
            endpoint = endpoint.Replace('=', '_');
            endpoint = endpoint.Replace('&', '_');
        }
    }
}