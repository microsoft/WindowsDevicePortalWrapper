//----------------------------------------------------------------------------------------------
// <copyright file="DevicePortalConnection.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using Microsoft.Tools.WindowsDevicePortal;
using Windows.Security.Credentials;
using static Microsoft.Tools.WindowsDevicePortal.DevicePortal;

namespace XboxWdpDriver
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
        /// <param name="address">The ip address or hostname of the device we are connecting to.</param>
        /// <param name="userName">The WDP username.</param>
        /// <param name="password">The WDP password.</param>
        public DevicePortalConnection(
            string address,
            string userName,
            string password)
        {
            this.Connection = new Uri(string.Format("https://{0}:11443", address));
            this.Credentials = new NetworkCredential(userName, password);

            PasswordVault vault = new PasswordVault();

            try
            {
                // Remove any existing stored creds for this address and add these ones.
                foreach (var cred in vault.FindAllByResource(address))
                {
                    vault.Remove(cred);
                }
            }
            catch (Exception)
            {
                // Do nothing. This is expected if no credentials have been previously stored
            }

            vault.Add(new PasswordCredential(address, userName, password));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DevicePortalConnection"/> class.
        /// This version of the contructor can be used if WDP credentials are not provided,
        /// and should be used if they were previously persisted or are not needed.
        /// </summary>
        /// <param name="address">The ip address or hostname of the device we are connecting to.</param>
        public DevicePortalConnection(
            string address)
        {
            this.Connection = new Uri(string.Format("https://{0}:11443", address));

            try
            {
                PasswordVault vault = new PasswordVault();
                // Set the first stored cred as our network creds.
                IReadOnlyList<PasswordCredential> creds = vault.FindAllByResource(address);
                if (creds != null && creds.Count > 0)
                {
                    creds[0].RetrievePassword();
                    this.Credentials = new NetworkCredential(creds[0].UserName, creds[0].Password);
                }
            }
            catch (Exception)
            {
                // Do nothing. No credentials were stored. If they are needed, REST calls will fail with Unauthorized.
            }
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

                if (absoluteUri.StartsWith("https", StringComparison.OrdinalIgnoreCase))
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
