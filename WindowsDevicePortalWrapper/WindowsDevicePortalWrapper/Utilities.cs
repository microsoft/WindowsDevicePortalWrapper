using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal
{
    internal static class Utilities
    {
        /// <summary>
        /// Constructs a fully formed REST API endpoint uri.
        /// </summary>
        /// <param name="baseUri">The base uri (typically, just scheme and authority).</param>
        /// <param name="path">The path to the REST API method (ex: api/control/restart).</param>
        /// <param name="payload">Parameterized data required by the REST API.</param>
        /// <returns>Uri object containing the complete path and query string required to issue the REST API call.</returns>
        public static Uri BuildEndpoint(Uri baseUri,
                                        String path,
                                        String payload = null)
        {
            String relativePart = !String.IsNullOrWhiteSpace(payload) ?
                                    String.Format("{0}?{1}", path, payload) : path;
            return new Uri(baseUri, relativePart);
        }

        /// <summary>
        /// Encodes the specified string as a base64 value.
        /// </summary>
        /// <param name="str">The string to encode.</param>
        /// <returns>Base64 encoded version of the string data.</returns>
        internal static String Hex64Encode(String str)
        {
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(str));
        }
    }
}
