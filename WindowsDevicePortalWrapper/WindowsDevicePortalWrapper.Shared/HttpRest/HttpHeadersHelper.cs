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
        /// Header name for a User-Agent.
        /// </summary>
        private static readonly string UserAgentName = "User-Agent";

        /// <summary>
        /// Header value for User-Agent for the WDPW Open Source project.
        /// </summary>
        private static readonly string UserAgentValue = "WindowsDevicePortalWrapper";

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
    }
}
