//----------------------------------------------------------------------------------------------
// <copyright file="UnvalidatedCert.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <summary>
    /// Handler for when an unvalidated cert is received.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="certificate">The server's certificate.</param>
    /// <param name="chain">The cert chain.</param>
    /// <param name="sslPolicyErrors">Policy Errors.</param>
    /// <returns>whether the cert should still pass validation.</returns>
    public delegate bool UnvalidatedCertEventHandler(DevicePortal sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors);
}
