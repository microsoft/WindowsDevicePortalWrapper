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
        /// <param name="acceptUntrustedCerts">Whether or not we should accept untrusted certificates.</param>
        /// <returns>The device certificate.</returns>
#pragma warning disable 1998
        public async Task<Certificate> GetRootDeviceCertificateAsync(bool acceptUntrustedCerts = false)
        {
            Certificate certificate = null;

            Uri uri = Utilities.BuildEndpoint(this.deviceConnection.Connection, RootCertificateEndpoint);
                
            HttpBaseProtocolFilter requestSettings = new HttpBaseProtocolFilter();
            requestSettings.AllowUI = false;

            if (acceptUntrustedCerts)
            {
                requestSettings.IgnorableServerCertificateErrors.Add(ChainValidationResult.Untrusted);
            }

            using (HttpClient client = new HttpClient(requestSettings))
            {
                this.ApplyHttpHeaders(client, HttpMethods.Get);
                using (HttpResponseMessage response = await client.GetAsync(uri))
                {
                    using (IHttpContent messageContent = response.Content)
                    {
                        certificate = new Certificate(await messageContent.ReadAsBufferAsync());
                    }
                }
            }

            return certificate;
        }
#pragma warning restore 1998

        /// <summary>
        /// Sets the manual certificate.
        /// </summary>
        /// <param name="cert">Manual certificate</param>
        private void SetManualCertificate(Certificate cert)
        {
            CertificateStore store = CertificateStores.TrustedRootCertificationAuthorities;
            store.Add(cert);
        }
    }
}
