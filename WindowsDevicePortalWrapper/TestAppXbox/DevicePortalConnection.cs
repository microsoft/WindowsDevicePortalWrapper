//----------------------------------------------------------------------------------------------
// <copyright file="DevicePortalConnection.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Tools.WindowsDevicePortal;
using static Microsoft.Tools.WindowsDevicePortal.DevicePortal;

namespace TestApp
{
    /// <summary>
    /// IDevicePortalConnection implementation for Xbox test project
    /// </summary>
    public class DevicePortalConnection : IDevicePortalConnection
    {
        /// <summary>
        /// Device Certificate
        /// </summary>
        private X509Certificate2 deviceCertificate = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="DevicePortalConnection"/> class.
        /// </summary>
        /// <param name="address">device identifier</param>
        /// <param name="userName">WDP username</param>
        /// <param name="password">WDP password</param>
        public DevicePortalConnection(
            string address,
            string userName,
            string password)
        {
            this.Connection = new Uri(string.Format("https://{0}:11443", address));
            this.Credentials = new NetworkCredential(userName, password);
        }

        /// <summary>
        /// Gets Connection property
        /// </summary>
        public Uri Connection
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets Credentials property
        /// </summary>
        public NetworkCredential Credentials
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets device name
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets device OS Info
        /// </summary>
        public OperatingSystemInformation OsInfo
        {
            get;
            set;
        }

        /// <summary>
        /// Gets Device Qualified Name
        /// </summary>
        public string QualifiedName
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns certificate data
        /// </summary>
        /// <returns>certificate data</returns>
        public byte[] GetDeviceCertificateData()
        {
            return this.deviceCertificate.GetRawCertData();
        }

        /// <summary>
        /// Sets certificate data
        /// </summary>
        /// <param name="certificateData">certificate data</param>
        public void SetDeviceCertificate(byte[] certificateData)
        {
            X509Certificate2 cert = new X509Certificate2(certificateData);
            if (!cert.IssuerName.Name.Contains(DevicePortalCertificateIssuer))
            {
                throw new DevicePortalException(
                    (HttpStatusCode)0,
                    "Invalid certificate issuer",
                    null,
                    "Failed to download device certificate");
            }

            this.deviceCertificate = cert;
        }

        /// <summary>
        /// Xbox will never update the connection.
        /// </summary>
        /// <param name="requiresHttps">https required</param>
        public void UpdateConnection(bool requiresHttps)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///  Xbox will never update the connection.
        /// </summary>
        /// <param name="ipConfig">IP info</param>
        /// <param name="requiresHttps">https required</param>
        public void UpdateConnection(IpConfiguration ipConfig, bool requiresHttps)
        {
            throw new NotImplementedException();
        }
    }
}
