﻿//----------------------------------------------------------------------------------------------
// <copyright file="RestDelete.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <content>
    /// HTTP DELETE Wrapper
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// Submits the http delete request to the specified uri.
        /// </summary>
        /// <param name="uri">The uri to which the delete request will be issued.</param>
        /// <returns>Task tracking HTTP completion</returns>
        private async Task Delete(Uri uri)
        {
            WebRequestHandler handler = new WebRequestHandler();
            handler.UseDefaultCredentials = false;
            handler.Credentials = this.deviceConnection.Credentials;
            handler.ServerCertificateValidationCallback = this.ServerCertificateValidation;

            using (HttpClient client = new HttpClient(handler))
            {
                // Set the CSRF-Token if we have one
                if (!string.IsNullOrEmpty(this.csrfToken))
                {
                    HttpRequestHeaders headers = client.DefaultRequestHeaders;
                    headers.Add("X-" + CsrfTokenName, this.csrfToken);
                }

                Task<HttpResponseMessage> deleteTask = HttpWrapper.DeleteAsync(client, uri);
                await deleteTask.ConfigureAwait(false);
                deleteTask.Wait();

                using (HttpResponseMessage response = deleteTask.Result)
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new DevicePortalException(response);
                    }
                }
            }
        }

        /// <summary>
        /// Calls the specified API with the provided payload.
        /// </summary>
        /// <param name="apiPath">The relative portion of the uri path that specifies the API to call.</param>
        /// <param name="payload">The query string portion of the uri path that provides the parameterized data.</param>
        /// <returns>Task tracking the HTTP completion.</returns>
        private async Task Delete(
            string apiPath,
            string payload = null)
        {
            Uri uri = Utilities.BuildEndpoint(
                this.deviceConnection.Connection,
                apiPath, 
                payload);

            await this.Delete(uri);
        }
    }
}