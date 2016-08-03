//----------------------------------------------------------------------------------------------
// <copyright file="CertificateHandling.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Security.Cryptography.Certificates;
using Windows.Storage.Streams;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <content>
    /// Universal Windows Platform implementation of device certificate handling methods.
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// Gets the root certificate from the device.
        /// </summary>
        /// <returns>The device certificate.</returns>
#pragma warning disable 1998
        private async Task<Certificate> GetDeviceCertificate()
        {
            Certificate certificate = null;
            bool useHttps = true;

            // try https then http
            while (true)
            {
                Uri uri = null;

                if (useHttps)
                {
                    uri = Utilities.BuildEndpoint(this.deviceConnection.Connection, RootCertificateEndpoint);
                }
                else
                {
                    Uri baseUri = new Uri(string.Format("http://{0}", this.deviceConnection.Connection.Authority));
                    uri = Utilities.BuildEndpoint(baseUri, RootCertificateEndpoint);
                }

                try
                {
                    HttpBaseProtocolFilter requestSettings = new HttpBaseProtocolFilter();
                    requestSettings.IgnorableServerCertificateErrors.Add(ChainValidationResult.Untrusted);
                    requestSettings.AllowUI = false;

                    using (HttpClient client = new HttpClient(requestSettings))
                    {
                        this.ApplyHttpHeaders(client, HttpMethods.Get);

                        IAsyncOperationWithProgress<HttpResponseMessage, HttpProgress> responseOperation = client.GetAsync(uri);
                        TaskAwaiter<HttpResponseMessage> responseAwaiter = responseOperation.GetAwaiter();
                        while (!responseAwaiter.IsCompleted)
                        { 
                        }

                        using (HttpResponseMessage response = responseOperation.GetResults())
                        {
                            this.RetrieveCsrfToken(response);

                            using (IHttpContent messageContent = response.Content)
                            {
                                IAsyncOperationWithProgress<IBuffer, ulong> bufferOperation = messageContent.ReadAsBufferAsync();
                                TaskAwaiter<IBuffer> readBufferAwaiter = bufferOperation.GetAwaiter();
                                while (!readBufferAwaiter.IsCompleted)
                                { 
                                }

                                certificate = new Certificate(bufferOperation.GetResults());
                                if (!certificate.Issuer.Contains(DevicePortalCertificateIssuer))
                                {
                                    certificate = null;
                                    throw new DevicePortalException(
                                        (HttpStatusCode)0,
                                        "Invalid certificate issuer",
                                        uri,
                                        "Failed to get the device certificate");
                                }
                            }
                        }
                    }

                    return certificate;
                }
                catch (Exception e)
                {
                    if (useHttps)
                    {
                        useHttps = false;
                    }
                    else
                    {
                        throw e;
                    }
                }
            }
        }
#pragma warning restore 1998
    }
}
