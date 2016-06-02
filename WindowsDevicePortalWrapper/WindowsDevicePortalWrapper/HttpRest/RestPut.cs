using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal
{
    public partial class DevicePortal
    {
        /// <summary>
        /// Submits the http put request to the specified uri.
        /// </summary>
        /// <param name="uri">The uri to which the put request will be issued.</param>
        private async Task Put(Uri uri, 
                               HttpContent body = null)
        {
            WebRequestHandler handler = new WebRequestHandler();
            handler.UseDefaultCredentials = false;
            handler.Credentials = _deviceConnection.Credentials;
            handler.ServerCertificateValidationCallback = ServerCertificateValidation;

            using (HttpClient client = new HttpClient(handler))
            {
                // Send the request
                Task <HttpResponseMessage> putTask = client.PutAsync(uri, body);
                await putTask.ConfigureAwait(false);
                putTask.Wait();

                using (HttpResponseMessage response = putTask.Result)
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
        private async Task Put(String apiPath,
                               String payload = null
                               )
        {
            Uri uri = Utilities.BuildEndpoint(_deviceConnection.Connection,
                                            apiPath, payload);
            await Put(uri);
        }

        /// <summary>
        /// Calls the specified api with the provided body.
        /// </summary>
        /// <param name="apiPath">The relative portion of the uri path that specifies the API to call.</param>
        /// <param name="payload">The query string portion of the uri path that provides the parameterized data.</param>
        private async Task Put<T>(String apiPath,
                                  T bodyData,
                                  String payload = null
                                 )
        {
            Uri uri = Utilities.BuildEndpoint(_deviceConnection.Connection,
                                            apiPath, payload);

            // Serialize the body to a JSON stream
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
            Stream stream = new MemoryStream();
            serializer.WriteObject(stream, bodyData);

            stream.Seek(0, SeekOrigin.Begin);
            StreamContent streamContent = new StreamContent(stream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            await Put(uri, streamContent);
        }
    }
}
