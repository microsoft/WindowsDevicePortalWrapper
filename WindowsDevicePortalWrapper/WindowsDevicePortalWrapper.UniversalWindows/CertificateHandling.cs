//----------------------------------------------------------------------------------------------
// <copyright file="CertificateHandling.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Security.Cryptography.Certificates;
using Windows.Storage.Streams;
using Windows.Web.Http;

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
                    using (HttpClient client = new HttpClient())
                    {
                        IAsyncOperationWithProgress<HttpResponseMessage, HttpProgress> responseOperation = client.GetAsync(uri);
                        while (responseOperation.Status != AsyncStatus.Completed)
                        { 
                        }

                        using (HttpResponseMessage response = responseOperation.GetResults())
                        {
                            using (IHttpContent messageContent = response.Content)
                            {
                                IAsyncOperationWithProgress<IBuffer, ulong> bufferOperation = messageContent.ReadAsBufferAsync();
                                while (bufferOperation.Status != AsyncStatus.Completed)
                                { 
                                }

                                certificate = new Certificate(bufferOperation.GetResults());
                                if (!certificate.Issuer.StartsWith(DevicePortalCertificateIssuer))
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

        /// <summary>
        /// Sets the device's root certificate in the certificate store. 
        /// </summary>
        /// <param name="certificate">The device's root certificate.</param>
        private void SetDeviceCertificate(Certificate certificate)
        {
            // Verify that the certificate is one we recognize.
            if (!certificate.Issuer.StartsWith(DevicePortalCertificateIssuer))
            {
                certificate = null;
                throw new DevicePortalException(
                    (HttpStatusCode)0,
                    "Invalid certificate issuer",
                    null,
                    "Failed to set the device certificate");
            }

            // Install the certificate.
            CertificateStore trustedStore = CertificateStores.TrustedRootCertificationAuthorities;
            trustedStore.Add(certificate);
        }
    }
}
