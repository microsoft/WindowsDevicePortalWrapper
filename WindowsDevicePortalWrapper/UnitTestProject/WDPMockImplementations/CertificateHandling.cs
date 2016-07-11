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
    /// MOCK implementation of device certificate handling methods.
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// Gets the root certificate from the device.
        /// </summary>
        /// <returns>The device certificate.</returns>
        private async Task<X509Certificate2> GetDeviceCertificate()
        {
            X509Certificate2 certificate = null;
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
                    using (Stream stream = await this.Get(uri, false))
                    {
                        using (BinaryReader reader = new BinaryReader(stream))
                        {
                            byte[] certData = reader.ReadBytes((int)stream.Length);

                            // Validate the issuer.
                            certificate = new X509Certificate2(certData);
                            if (!certificate.IssuerName.Name.StartsWith(DevicePortalCertificateIssuer))
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

        /// <summary>
        /// Validate the server certificate
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="cert">The server's certificate</param>
        /// <param name="chain">The cert chain</param>
        /// <param name="policyErrors">Policy Errors</param>
        /// <returns>whether the cert passes validation</returns>
        private bool ServerCertificateValidation(
            object sender,
            X509Certificate cert,
            X509Chain chain,
            SslPolicyErrors policyErrors)
        {
            //// TODO - really need a GOOD (read: secure) way to do this for .net. uwp already handles nicely

            byte[] deviceCertData = this.deviceConnection.GetDeviceCertificateData();

            if (deviceCertData == null)
            {
                // No certificate, fail validation.
                return false;
            }

            X509Certificate deviceCert = new X509Certificate(deviceCertData);

            // Check the certificate
            // * First, make sure we are in the date range
            DateTime now = DateTime.Now;
            if ((now < DateTime.Parse(cert.GetEffectiveDateString())) ||
                (now > DateTime.Parse(cert.GetExpirationDateString())))
            {
                // The current date is out of bounds, fail validation.
                return false;
            }

            // * Next, compare the issuer
            if (deviceCert.Issuer != cert.Issuer)
            {
                return false;
            }

            /*
            // Would be nice to allow Fiddler via an override as well--Issuer will show up as something like the following:
            // "cert.Issuer = "CN=DO_NOT_TRUST_FiddlerRoot, O=DO_NOT_TRUST, OU=Created by http://www.fiddler2.com"
            */

            return true;
        }

        /// <summary>
        /// No-op version of cert validation for skipping the validation
        /// </summary>
        /// <param name="sender">the sender</param>
        /// <param name="cert">the certificate</param>
        /// <param name="chain">cert chain</param>
        /// <param name="policyErrors">policy Errors</param>
        /// <returns>Always returns true since validation is skipped.</returns>
        private bool ServerCertificateNonValidation(
            object sender,
            X509Certificate cert,
            X509Chain chain,
            SslPolicyErrors policyErrors)
        {
            return true;
        }
    }
}
