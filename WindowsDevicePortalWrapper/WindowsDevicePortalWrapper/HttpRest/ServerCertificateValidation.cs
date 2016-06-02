using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal
{
    public partial class DevicePortal
    {
        private Boolean ServerCertificateValidation(Object sender,
                                                    X509Certificate cert,
                                                    X509Chain chain,
                                                    SslPolicyErrors policyErrors)
        {
            // BUGBUG - really need a GOOD (read: secure) way to do this for .net. uwp already handles nicely

            Byte[] deviceCertData = _deviceConnection.GetDeviceCertificateData();

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

            // BUGBUG - need good validation...... 
            // Would be nice to allow Fiddler via an override as well--Issuer will show up as something like the following:
            // "cert.Issuer = "CN=DO_NOT_TRUST_FiddlerRoot, O=DO_NOT_TRUST, OU=Created by http://www.fiddler2.com"

            return true;
        }

        private Boolean ServerCertificateNonValidation(Object sender,
                                                    X509Certificate cert,
                                                    X509Chain chain,
                                                    SslPolicyErrors policyErrors)
        {
            return true;
        }
    }
}
