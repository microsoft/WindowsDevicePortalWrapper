//----------------------------------------------------------------------------------------------
// <copyright file="RestDelete.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Security.Credentials;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <content>
    /// Universal Windows Platform implementation of HTTP Delete
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// Submits the http delete request to the specified uri.
        /// </summary>
        /// <param name="uri">The uri to which the delete request will be issued.</param>
        /// <returns>Task tracking HTTP completion</returns>
#pragma warning disable 1998
        private async Task Delete(Uri uri)
        {
            HttpBaseProtocolFilter httpFilter = new HttpBaseProtocolFilter();
            httpFilter.AllowUI = false;
            httpFilter.ServerCredential = new PasswordCredential();
            httpFilter.ServerCredential.UserName = this.deviceConnection.Credentials.UserName;
            httpFilter.ServerCredential.Password = this.deviceConnection.Credentials.Password;

            using (HttpClient client = new HttpClient(httpFilter))
            {
                this.ApplyCsrfToken(client, "DELETE");

                IAsyncOperationWithProgress<HttpResponseMessage, HttpProgress> responseOperation = client.DeleteAsync(uri);
                TaskAwaiter<HttpResponseMessage> responseAwaiter = responseOperation.GetAwaiter();
                while (!responseAwaiter.IsCompleted)
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
