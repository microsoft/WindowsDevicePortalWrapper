//----------------------------------------------------------------------------------------------
// <copyright file="CsrfToken.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

#if !WINDOWS_UWP
using System.Net.Http;
using System.Net.Http.Headers;
#else
using Windows.Web.Http;
using Windows.Web.Http.Headers;
#endif // !WINDOWS_UWP

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <content>
    /// Methods for working with CSRF tokens
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// Sets the CSRF Token header
        /// </summary>
        /// <param name="client">The client on which to have the header set.</param>
        /// <param name="method">The HTTP method (ex: POST) that will be called on the client.</param>
        public void SetCrsfToken(
            HttpClient client,
            string method)
        {
            string headerName = "X-" + CsrfTokenName;
            string headerValue = this.csrfToken;

            if (string.Compare(method, "get", true) == 0)
            {
                headerName = CsrfTokenName;
                headerValue = string.IsNullOrEmpty(this.csrfToken) ? "Fetch" : headerValue;
            }

#if WINDOWS_UWP
            HttpRequestHeaderCollection headers = client.DefaultRequestHeaders;
#else
            HttpRequestHeaders headers = client.DefaultRequestHeaders;
#endif // WINDOWS_UWP

            headers.Add(headerName, headerValue);
        }
    }
}
