//----------------------------------------------------------------------------------------------
// <copyright file="RestPut.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Security.Credentials;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <content>
    /// Universal Windows Platform implementation of HTTP Put
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// Submits the http put request to the specified uri.
        /// </summary>
        /// <param name="uri">The uri to which the put request will be issued.</param>
        /// <param name="body">The HTTP content comprising the body of the request.</param>
        /// <returns>Task tracking the PUT completion.</returns>
#pragma warning disable 1998
        private async Task Put(
            Uri uri,
            IHttpContent body = null)
        {
            HttpBaseProtocolFilter httpFilter = new HttpBaseProtocolFilter();
            httpFilter.AllowUI = false;
            httpFilter.ServerCredential = new PasswordCredential();
            httpFilter.ServerCredential.UserName = this.deviceConnection.Credentials.UserName;
            httpFilter.ServerCredential.Password = this.deviceConnection.Credentials.Password;

            using (HttpClient client = new HttpClient(httpFilter))
            {
                this.SetCrsfToken(client, "PUT");

                // Send the request
                IAsyncOperationWithProgress<HttpResponseMessage, HttpProgress> responseOperation = client.PutAsync(uri, null);
                while (responseOperation.Status != AsyncStatus.Completed)
                { 
                }

                using (HttpResponseMessage response = responseOperation.GetResults())
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new DevicePortalException(response);
                    }
                }
            }
        }
#pragma warning restore 1998
    }
}
