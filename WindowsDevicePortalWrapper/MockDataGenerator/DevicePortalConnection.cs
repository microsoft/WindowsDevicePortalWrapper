//----------------------------------------------------------------------------------------------
// <copyright file="DevicePortalConnection.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using Microsoft.Tools.WindowsDevicePortal;
using static Microsoft.Tools.WindowsDevicePortal.DevicePortal;

namespace MockDataGenerator
{
    /// <summary>
    /// IDevicePortalConnection implementation for MockDataGenerator test project
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
            this.Connection = new Uri(string.Format("https://{0}", address));
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
        /// Gets Web Socket Connection property
        /// </summary>
        public Uri WebSocketConnection
        {
            get
            {
                if (this.Connection == null)
                {
                    return null;
                }

                string absoluteUri = this.Connection.AbsoluteUri;

                if (absoluteUri.StartsWith("https", StringComparison.InvariantCultureIgnoreCase))
                {
                    return new Uri(Regex.Replace(absoluteUri, "https", "wss", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant));
                }
                else
                {
                    return new Uri(Regex.Replace(absoluteUri, "http", "ws", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant));
                }
            }
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
        /// Gets or sets device family
        /// </summary>
        public string Family
        { 
            get; 
            set;
        }

        /// <summary>
        /// Gets or sets the device name
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
        /// Gets or sets the device's qualified name
        /// </summary>
        public string QualifiedName
        {
            get;
            set;
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
        /// Validates and sets the device certificate.
        /// </summary>
        /// <param name="certificate">The device's root certificate.</param>
        public void SetDeviceCertificate(X509Certificate2 certificate)
        {
            if (!certificate.IssuerName.Name.Contains(DevicePortalCertificateIssuer))
            {
                throw new DevicePortalException(
                    (HttpStatusCode)0,
                    "Invalid certificate issuer",
                    null,
                    "Failed to download device certificate");
            }

            this.deviceCertificate = certificate;
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
        /// MockDataGenerator will never update the connection.
        /// </summary>
        /// <param name="requiresHttps">https required</param>
        public void UpdateConnection(bool requiresHttps)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///  MockDataGenerator will never update the connection.
        /// </summary>
        /// <param name="ipConfig">IP info</param>
        /// <param name="requiresHttps">https required</param>
        public void UpdateConnection(IpConfiguration ipConfig, bool requiresHttps)
        {
            throw new NotImplementedException();
        }
    }
}
