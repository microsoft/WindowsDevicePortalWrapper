﻿//----------------------------------------------------------------------------------------------
// <copyright file="DefaultDevicePortalConnection.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using static Microsoft.Tools.WindowsDevicePortal.DevicePortal;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <summary>
    /// Default implementation of the IDevicePortalConnection interface.
    /// This implementation is designed to be compatibile with all device families.
    /// </summary>
    public class DefaultDevicePortalConnection : IDevicePortalConnection
    {
        /// <summary>
        /// The device's root certificate.
        /// </summary>
        private X509Certificate2 deviceCertificate = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultDevicePortalConnection" /> class.
        /// </summary>
        /// <param name="address">The fully qualified (ex: "https:/1.2.3.4:4321") address of the device.</param>
        /// <param name="userName">The user name used in the connection credentials.</param>
        /// <param name="password">The password used in the connection credentials.</param>
        public DefaultDevicePortalConnection(
            string address,
            string userName,
            string password)
        {
            this.Connection = new Uri(address);

            if (!string.IsNullOrEmpty(userName) &&
                !string.IsNullOrEmpty(password))
            {
                // append auto- to the credentials to bypass CSRF token requirement on non-Get requests.
                this.Credentials = new NetworkCredential(string.Format("auto-{0}", userName), password);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XboxDevicePortalConnection"/> class, using a SecureString to secure the password.
        /// </summary>
        /// <param name="address">device identifier</param>
        /// <param name="userName">WDP username</param>
        /// <param name="password">WDP password</param>
        public DefaultDevicePortalConnection(
            string address,
            string userName,
            System.Security.SecureString password)
        {
            this.Connection = new Uri(address);


            if (!string.IsNullOrEmpty(userName) &&
                password != null &&
                password.Length > 0)
            {
                // append auto- to the credentials to bypass CSRF token requirement on non-Get requests.
                this.Credentials = new NetworkCredential(string.Format("auto-{0}", userName), password);
            }
        }

        /// <summary>
        /// Gets the URI used to connect to the device.
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

                // Convert the scheme from http[s] to ws[s].
                string scheme = this.Connection.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase) ? "wss" : "ws";

                return new Uri(
                    string.Format("{0}://{1}",
                        scheme,
                        this.Connection.Authority));
            }
        }

        /// <summary>
        /// Gets the credentials used to connect to the device.
        /// </summary>
        public NetworkCredential Credentials
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the device's operating system family.
        /// </summary>
        public string Family
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the operating system information.
        /// </summary>
        public OperatingSystemInformation OsInfo
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the provided device certificate.
        /// </summary>
        /// <returns>Stored device certificate.</returns>
        public X509Certificate2 GetDeviceCertificate()
        {
            return this.deviceCertificate;
        }

        /// <summary>
        /// Stores a manually provided device certificate.
        /// </summary>
        /// <param name="certificate">The device's root certificate.</param>
        public void SetDeviceCertificate(X509Certificate2 certificate)
        {
            this.deviceCertificate = certificate;
        }

        /// <summary>
        /// Updates the device's connection Uri.
        /// </summary>
        /// <param name="requiresHttps">Indicates whether or not to always require a secure connection.</param>
        public void UpdateConnection(bool requiresHttps)
        {
            this.Connection = new Uri(
                string.Format(
                    "{0}://{1}",
                    requiresHttps ? "https" : "http",
                    this.Connection.Authority));
        }
        
        /// <summary>
        /// Updates the device's connection Uri.
        /// </summary>
        /// <param name="ipConfig">The device's IP configuration data.</param>
        /// <param name="requiresHttps">Indicates whether or not to always require a secure connection.</param>
        public void UpdateConnection(
            IpConfiguration ipConfig,
            bool requiresHttps = false)
        {
            Uri newConnection = null;

            foreach (NetworkAdapterInfo adapter in ipConfig.Adapters)
            {
                foreach (IpAddressInfo addressInfo in adapter.IpAddresses)
                {
                    // We take the first, non-169.x.x.x address we find that is not 0.0.0.0.
                    if ((addressInfo.Address != "0.0.0.0") && !addressInfo.Address.StartsWith("169."))
                    {
                        string port = "";
                        string[] addressParts = this.Connection.Authority.Split(':');
                        if (addressParts.Length == 2)
                        {
                            port = string.Format(":{0}", addressParts[1]);
                        }

                        newConnection = new Uri(
                            string.Format(
                                "{0}://{1}{2}",
                                requiresHttps ? "https" : "http",
                                addressInfo.Address,
                                port));
                        break;
                    }
                }

                if (newConnection != null)
                {
                    this.Connection = newConnection;
                    break;
                }
            }
        }
    }
}
