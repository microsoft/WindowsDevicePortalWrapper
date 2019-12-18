//----------------------------------------------------------------------------------------------
// <copyright file="HttpClientWrapper.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using static Microsoft.Tools.WindowsDevicePortal.DevicePortal;

namespace Microsoft.Tools.WindowsDevicePortal
{
    internal class HttpClientWrapper : IHttpClient
    {
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

        private readonly ICredentials credentials;
        private readonly Func<HttpRequestMessage, X509Certificate2, X509Chain, SslPolicyErrors, bool> serverCertificateValidation;

        /// <summary>
        /// CSRF token retrieved by GET calls and used on subsequent POST/DELETE/PUT calls.
        /// This token is intended to prevent a security vulnerability from cross site forgery.
        /// </summary>
        private string csrfToken = string.Empty;

        public HttpClientWrapper(ICredentials credentials = null, Func<HttpRequestMessage, X509Certificate2, X509Chain, SslPolicyErrors, bool> ServerCertificateValidation = null)
        {
            this.credentials = credentials;
            this.serverCertificateValidation = ServerCertificateValidation;
        }

        public Task<HttpResponseMessage> GetAsync(Uri uri) =>
            this.Wrap(HttpMethods.Get, client => client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead));

        public Task<HttpResponseMessage> PostAsync(Uri uri, HttpContent content) =>
            this.Wrap(HttpMethods.Post, client => client.PostAsync(uri, content));

        public Task<HttpResponseMessage> PutAsync(Uri uri, HttpContent content) =>
            this.Wrap(HttpMethods.Put, client => client.PutAsync(uri, content));

        public Task<HttpResponseMessage> DeleteAsync(Uri uri) =>
            this.Wrap(HttpMethods.Delete, client => client.DeleteAsync(uri));

        private async Task<HttpResponseMessage> Wrap(HttpMethods method, Func<HttpClient, Task<HttpResponseMessage>> func)
        {
            var handler = new HttpClientHandler
            {
                UseDefaultCredentials = false,
                Credentials = this.credentials,
                ServerCertificateCustomValidationCallback = this.serverCertificateValidation
            };

            using (HttpClient client = new HttpClient(handler))
            {
                this.ApplyHttpHeaders(client, method);

                HttpResponseMessage response = await func(client).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    throw await DevicePortalException.CreateAsync(response);
                }

                this.RetrieveCsrfToken(response);

                return response;
            }
        }

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

            HttpRequestHeaders headers = client.DefaultRequestHeaders;
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

            Assembly asm = Assembly.GetExecutingAssembly();
            userAgentValue += "-v" + asm.GetName().Version.ToString();
            HttpRequestHeaders headers = client.DefaultRequestHeaders;
            headers.Add(UserAgentName, userAgentValue);
        }

        /// <summary>
        /// Retrieves the CSRF token from the HTTP response and stores it.
        /// </summary>
        /// <param name="response">The HTTP response from which to retrieve the header.</param>
        private void RetrieveCsrfToken(HttpResponseMessage response)
        {
            // If the response sets a CSRF token, store that for future requests.
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
        }
    }
}
