//----------------------------------------------------------------------------------------------
// <copyright file="CertificateHandling.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <content>
    /// .net 4.x implementation of device certificate handling methods.
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// A manually provided certificate for trust validation.
        /// </summary>
        private X509Certificate2 manualCertificate = null;

        /// <summary>
        /// Gets or sets handler for untrusted certificate handling
        /// </summary>
        public event UnvalidatedCertEventHandler UnvalidatedCert;

        /// <summary>
        /// Gets the root certificate from the device.
        /// </summary>
        /// <returns>The device certificate.</returns>
        public async Task<X509Certificate2> GetRootDeviceCertificateAsync()
        {
            X509Certificate2 certificate = null;

            Uri uri = Utilities.BuildEndpoint(this.deviceConnection.Connection, RootCertificateEndpoint);

            using (Stream stream = await this.GetAsync(uri))
            {
                using (MemoryStream outStream = new MemoryStream())
                {
                    await stream.CopyToAsync(outStream);
                    using (BinaryReader reader = new BinaryReader(outStream))
                    {
                        byte[] certData = reader.ReadBytes((int)outStream.Length);
                        certificate = new X509Certificate2(certData);
                    }
                }
            }

            return certificate;
        }

        /// <summary>
        /// Sets the manual certificate.
        /// </summary>
        /// <param name="cert">Manual certificate</param>
        private void SetManualCertificate(X509Certificate2 cert)
        {
            this.manualCertificate = cert;
        }

        /// <summary>
        /// Validate the server certificate
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="certificate">The server's certificate</param>
        /// <param name="chain">The cert chain</param>
        /// <param name="sslPolicyErrors">Policy Errors</param>
        /// <returns>whether the cert passes validation</returns>
        private bool ServerCertificateValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (this.manualCertificate != null)
            {
                chain.ChainPolicy.ExtraStore.Add(this.manualCertificate);
            }

            X509Certificate2 certv2 = new X509Certificate2(certificate);
            bool isValid = chain.Build(certv2);

            // If chain validation failed but we have a manual cert, we can still
            // check the chain to see if the server cert chains up to our manual cert
            // (or matches it) in which case this is valid.
            if (!isValid && this.manualCertificate != null)
            {
                foreach (X509ChainElement element in chain.ChainElements)
                {
                    foreach (X509ChainStatus status in element.ChainElementStatus)
                    {
                        // Check if this is a failure that should cause the chain to be rejected
                        if (status.Status != X509ChainStatusFlags.NoError &&
                            status.Status != X509ChainStatusFlags.UntrustedRoot &&
                            status.Status != X509ChainStatusFlags.RevocationStatusUnknown)
                        {
                            return false;
                        }
                    }

                    // This cert chained to our provided cert. Continue walking
                    // the chain to ensure we don't hit a failure that would
                    // cause our chain to be rejected.
                    if (element.Certificate.Issuer == this.manualCertificate.Issuer &&
                        element.Certificate.Thumbprint == this.manualCertificate.Thumbprint)
                    {
                        isValid = true;
                        break;
                    }
                }
            }

            // If this still appears invalid, we give the app a chance via a handler
            // to override the trust decision.
            if (!isValid)
            {
                bool? overridenIsValid = this.UnvalidatedCert?.Invoke(this, certificate, chain, sslPolicyErrors);

                if (overridenIsValid != null && overridenIsValid == true)
                {
                    isValid = true;
                }
            }

            return isValid;
        }
    }
}
