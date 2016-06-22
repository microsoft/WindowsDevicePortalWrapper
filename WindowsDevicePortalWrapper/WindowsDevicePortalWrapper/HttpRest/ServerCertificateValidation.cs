//----------------------------------------------------------------------------------------------
// <copyright file="ServerCertificateValidation.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsDevicePortal
{
    using System;
    using System.Net.Security;
    using System.Security.Cryptography.X509Certificates;

    /// <content>
    /// Cert validation implementation
    /// </content>
    public partial class DevicePortal
    {
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

            byte[] deviceCertData = deviceConnection.GetDeviceCertificateData();

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
            // TODO - need good validation...... 
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
