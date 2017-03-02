//----------------------------------------------------------------------------------------------
// <copyright file="RestDelete.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Tools.WindowsDevicePortal.Tests;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <content>
    /// MOCK implementation of HTTP DeleteAsync
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// Submits the http delete request to the specified uri.
        /// </summary>
        /// <param name="uri">The uri to which the delete request will be issued.</param>
        /// <returns>Task tracking HTTP completion</returns>
        private async Task<Stream> DeleteAsync(Uri uri)
        {
            MemoryStream dataStream = null;

            WebRequestHandler requestSettings = new WebRequestHandler();
            requestSettings.UseDefaultCredentials = false;
            requestSettings.Credentials = this.deviceConnection.Credentials;
            requestSettings.ServerCertificateValidationCallback = this.ServerCertificateValidation;

            using (HttpClient client = new HttpClient(requestSettings))
            {
                this.ApplyHttpHeaders(client, HttpMethods.Delete);

                Task<HttpResponseMessage> deleteTask = TestHelpers.MockHttpResponder.DeleteAsync(uri);
                await deleteTask.ConfigureAwait(false);
                deleteTask.Wait();

                using (HttpResponseMessage response = deleteTask.Result)
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        throw await DevicePortalException.CreateAsync(response);
                    }

                    if (response.Content != null)
                    {
                        using (HttpContent content = response.Content)
                        {
                            dataStream = new MemoryStream();

                            Task copyTask = content.CopyToAsync(dataStream);
                            await copyTask.ConfigureAwait(false);
                            copyTask.Wait();

                            // Ensure we return with the stream pointed at the origin.
                            dataStream.Position = 0;
                        }
                    }
                }
            }

            return dataStream;
        }
    }
}
