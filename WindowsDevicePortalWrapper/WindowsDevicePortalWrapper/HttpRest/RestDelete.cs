using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal
{
    public partial class DevicePortal
    {
        /// <summary>
        /// Submits the http delete request to the specified uri.
        /// </summary>
        /// <param name="uri">The uri to which the delete request will be issued.</param>
        private async Task Delete(Uri uri)
        {
            WebRequestHandler handler = new WebRequestHandler();
            handler.UseDefaultCredentials = false;
            handler.Credentials = _deviceConnection.Credentials;
            handler.ServerCertificateValidationCallback = ServerCertificateValidation;

            using (HttpClient client = new HttpClient(handler))
            {
                Task<HttpResponseMessage> deleteTask = client.DeleteAsync(uri);
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
        /// Calls the specified api with the provided payload.
        /// </summary>
        /// <param name="apiPath">The relative portion of the uri path that specifies the API to call.</param>
        /// <param name="payload">The query string portion of the uri path that provides the parameterized data.</param>
        private async Task Delete(String apiPath,
                                String payload = null)
        {
            Uri uri = Utilities.BuildEndpoint(_deviceConnection.Connection,
                                            apiPath, payload);
            await Delete(uri);
        }
    }
}
