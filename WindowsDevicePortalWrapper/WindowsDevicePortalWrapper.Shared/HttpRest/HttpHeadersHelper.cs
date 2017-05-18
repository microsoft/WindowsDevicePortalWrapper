//----------------------------------------------------------------------------------------------
// <copyright file="HttpHeadersHelper.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

#if !WINDOWS_UWP
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
#else
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Web.Http;
using Windows.Web.Http.Headers;
#endif // !WINDOWS_UWP

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <content>
    /// Methods for working with Http headers.
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// Header name for Content Type of a request body.
        /// </summary>
        private static readonly string ContentTypeHeaderName = "Content-Type";

        /// <summary>
        /// Header name for a CSRF-Token.
        /// </summary>
        private static readonly string CsrfTokenName = "CSRF-Token";

        /// <summary>
        /// Header name for a User-Agent.
        /// </summary>
        private static readonly string UserAgentName = "User-Agent";

        /// <summary>
        /// Header value for User-Agent for the WDPW Open Source project.
        /// </summary>
        private static readonly string UserAgentValue = "WindowsDevicePortalWrapper";

        /// <summary>
        /// CSRF token retrieved by GET calls and used on subsequent POST/DELETE/PUT calls.
        /// This token is intended to prevent a security vulnerability from cross site forgery.
        /// </summary>
        private string csrfToken = string.Empty;

        /// <summary>
        /// Applies the CSRF token to the HTTP client.
        /// </summary>
        /// <param name="client">The HTTP client on which to have the header set.</param>
        /// <param name="method">The HTTP method (ex: POST) that will be called on the client.</param>
        private void ApplyCSRFHeader(
            HttpClient client,
            HttpMethods method)
        {
            string headerName = "X-" + CsrfTokenName;
            string headerValue = this.csrfToken;

            if (string.Compare(method.ToString(), "get", true) == 0)
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

        /// <summary>
        /// Applies any needed headers to the HTTP client.
        /// </summary>
        /// <param name="client">The HTTP client on which to have the headers set.</param>
        /// <param name="method">The HTTP method (ex: POST) that will be called on the client.</param>
        private void ApplyHttpHeaders(
            HttpClient client,
            HttpMethods method)
        {
            this.ApplyUserAgentHeader(client);
            this.ApplyCSRFHeader(client, method);
        }

        /// <summary>
        /// Adds the User-Agent string to a request to identify it
        /// as coming from the WDPW Open Source project.
        /// </summary>
        /// <param name="client">The HTTP client on which to have the header set.</param>
        private void ApplyUserAgentHeader(HttpClient client)
        {
            string userAgentValue = UserAgentValue;

#if WINDOWS_UWP
            Assembly asm = this.GetType().GetTypeInfo().Assembly;
            userAgentValue += "-v" + asm.GetName().Version.ToString();
            userAgentValue += "-UWP";
            HttpRequestHeaderCollection headers = client.DefaultRequestHeaders;
#else
            Assembly asm = Assembly.GetExecutingAssembly();
            userAgentValue += "-v" + asm.GetName().Version.ToString();
            userAgentValue += "-dotnet";
            HttpRequestHeaders headers = client.DefaultRequestHeaders;
#endif // WINDOWS_UWP

            headers.Add(UserAgentName, userAgentValue);
        }

        /// <summary>
        /// Retrieves the CSRF token from the HTTP response and stores it.
        /// </summary>
        /// <param name="response">The HTTP response from which to retrieve the header.</param>
        private void RetrieveCsrfToken(HttpResponseMessage response)
        {
            // If the response sets a CSRF token, store that for future requests.
#if WINDOWS_UWP
            string cookie;
            if (response.Headers.TryGetValue("Set-Cookie", out cookie))
            {
                string csrfTokenNameWithEquals = CsrfTokenName + "=";
                if (cookie.StartsWith(csrfTokenNameWithEquals))
                {
                    this.csrfToken = cookie.Substring(csrfTokenNameWithEquals.Length);
                }
            }
#else
            IEnumerable<string> cookies;
            if (response.Headers.TryGetValues("Set-Cookie", out cookies))
            {
                foreach (string cookie in cookies)
                {
                    string csrfTokenNameWithEquals = CsrfTokenName + "=";
                    if (cookie.StartsWith(csrfTokenNameWithEquals))
                    {
                        this.csrfToken = cookie.Substring(csrfTokenNameWithEquals.Length);
                    }
                }
            }
#endif
        }
    }
}
