//----------------------------------------------------------------------------------------------
// <copyright file="CertificateHandling.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
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
        /// Sets the manual certificate.
        /// </summary>
        /// <param name="cert">Manual certificate</param>
        private void SetManualCertificate(X509Certificate2 cert)
        {
            // Do nothing.
        }

        // Disable warning about async method lacking 'await'
#pragma warning disable 1998
        /// <summary>
        /// Gets the root certificate from the device.
        /// </summary>
        /// <returns>The device certificate.</returns>
        private async Task<X509Certificate2> GetRootDeviceCertificate()
        {
            throw new NotSupportedException();
        }
#pragma warning restore 1998

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
            return false;
        }
    }
}
