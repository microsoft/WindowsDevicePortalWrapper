using Microsoft.Tools.WindowsDevicePortal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace TestApp
{
    public class DevicePortalConnection : IDevicePortalConnection
    {
        private X509Certificate2 _deviceCertificate = null;

        public Uri Connection
        { get; private set; }

        public NetworkCredential Credentials
        { get; private set; }

        public String Name
        { get; set; }

        public OperatingSystemInformation OsInfo
        { get; set; }

        public String QualifiedName
        { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public DevicePortalConnection(String address,
                                    String userName,
                                    String password)
        {
            Connection = new Uri(String.Format("https://{0}:11443", address));
            Credentials = new NetworkCredential(userName, password);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Byte[] GetDeviceCertificateData()
        {
            return _deviceCertificate.GetRawCertData();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="certificateData"></param>
        public void SetDeviceCertificate(Byte[] certificateData)
        {
            X509Certificate2 cert = new X509Certificate2(certificateData);
            if (!cert.IssuerName.Name.Contains(DevicePortal.DevicePortalCertificateIssuer))
            {
                throw new DevicePortalException((HttpStatusCode)0,
                                                "Invalid certificate issuer",
                                                null,
                                                "Failed to download device certificate");
            }
            _deviceCertificate = cert;
        }

        public void UpdateConnection(bool requiresHttps)
        {
            throw new NotImplementedException();
        }

        public void UpdateConnection(IpConfiguration ipConfig, bool requiresHttps)
        {
            throw new NotImplementedException();
        }
    }
}
