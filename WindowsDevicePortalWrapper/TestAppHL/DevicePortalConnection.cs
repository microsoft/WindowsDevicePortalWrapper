using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Tools.WindowsDevicePortal;
using static Microsoft.Tools.WindowsDevicePortal.DevicePortal;

namespace TestApp
{
    public class DevicePortalConnection : IDevicePortalConnection
    {
        private X509Certificate2 deviceCertificate = null;

        public Uri Connection
        { get; private set; }

        public NetworkCredential Credentials
        { get; private set; }

        public string Name
        { get; set; }

        public OperatingSystemInformation OsInfo
        { get; set; }

        public string QualifiedName
        { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public DevicePortalConnection(string address,
                                    string userName,
                                    string password)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                address = "localhost:10080";
            }

            Connection = new Uri(string.Format("{0}://{1}", GetUriScheme(address), address));
            Credentials = new NetworkCredential(userName, password);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Byte[] GetDeviceCertificateData()
        {
            return deviceCertificate.GetRawCertData();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="certificateData"></param>
        public void SetDeviceCertificate(Byte[] certificateData)
        {
            X509Certificate2 cert = new X509Certificate2(certificateData);
            if (!cert.IssuerName.Name.Contains(DevicePortalCertificateIssuer))
            {
                throw new DevicePortalException((HttpStatusCode)0,
                                                "Invalid certificate issuer",
                                                null,
                                                "Failed to download device certificate");
            }
            deviceCertificate = cert;
        }

        public void UpdateConnection(Boolean requiresHttps)
        {
            Connection = new Uri(string.Format("{0}://{1}", GetUriScheme(Connection.Authority, requiresHttps), Connection.Authority));
        }

        public void UpdateConnection(IpConfiguration ipConfig,
                                    Boolean requiresHttps = false)
        {
            Uri newConnection = null;

            foreach (NetworkAdapterInfo adapter in ipConfig.Adapters)
            {
                foreach (IpAddressInfo addressInfo in adapter.IpAddresses)
                {
                    // We take the first, non-169.x.x.x address we find that is not 0.0.0.0.
                    if ((addressInfo.Address != "0.0.0.0") && !addressInfo.Address.StartsWith("169."))
                    {
                        newConnection = new Uri(string.Format("{0}://{1}", GetUriScheme(addressInfo.Address, requiresHttps), addressInfo.Address));
                        // TODO qualified name
                        break;
                    }
                }

                if (newConnection != null)
                {
                    Connection = newConnection;
                    break;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="requiresHttps"></param>
        /// <returns></returns>
        private string GetUriScheme(string address,
                                    Boolean requiresHttps = true)
        {
            return (address.Contains("127.0.0.1") || 
                    address.Contains("localhost") || 
                    !requiresHttps) ? "http" : "https";
        }
    }
}
