//----------------------------------------------------------------------------------------------
// <copyright file="RestPost.cs" company="Microsoft Corporation">
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
    /// Universal Windows Platform implementation of HTTP Post
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// Submits the http post request to the specified uri.
        /// </summary>
        /// <param name="uri">The uri to which the post request will be issued.</param>
        /// <returns>Task tracking the completion of the POST request</returns>
#pragma warning disable 1998
        private async Task Post(Uri uri)
        {
            HttpBaseProtocolFilter httpFilter = new HttpBaseProtocolFilter();
            httpFilter.AllowUI = false;
            httpFilter.ServerCredential = new PasswordCredential();
            httpFilter.ServerCredential.UserName = this.deviceConnection.Credentials.UserName;
            httpFilter.ServerCredential.Password = this.deviceConnection.Credentials.Password;

            using (HttpClient client = new HttpClient(httpFilter))
            {
                this.SetCrsfToken(client, "POST");

                IAsyncOperationWithProgress<HttpResponseMessage, HttpProgress> responseOperation = client.PostAsync(uri, null);
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
