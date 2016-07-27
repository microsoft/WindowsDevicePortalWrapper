//----------------------------------------------------------------------------------------------
// <copyright file="DevicePortalConnection.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Net;
using System.Text.RegularExpressions;
using Windows.Security.Cryptography.Certificates;
using Microsoft.Tools.WindowsDevicePortal;
using static Microsoft.Tools.WindowsDevicePortal.DevicePortal;

namespace TestAppHL.UniversalWindows
{
    public class DevicePortalConnection : IDevicePortalConnection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DevicePortalConnection" /> class.
        /// </summary>
        /// <param name="address">The address of the device.</param>
        /// <param name="userName">The user name used in the connection credentials.</param>
        /// <param name="password">The password used in the connection credentials.</param>
        public DevicePortalConnection(
            string address,
            string userName,
            string password)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                address = "localhost:10080";
            }

            this.Connection = new Uri(
                string.Format("{0}://{1}", 
                this.GetUriScheme(address), 
                address));
            this.Credentials = new NetworkCredential(
                userName, 
                password);
        }

        public Uri Connection
        {
            get;
            private set;
        }

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
        /// Gets or sets the device name.
        /// </summary>
        public string Name
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
        /// Gets the connection as a Web Socket
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
        /// Validates and sets the device certificate.
        /// </summary>
        /// <param name="certificate">The device's root certificate.</param>
        /// <summary>
        /// Sets the device's root certificate in the certificate store. 
        /// </summary>
        /// <param name="certificate">The device's root certificate.</param>
        public void SetDeviceCertificate(Certificate certificate)
        {
            // Verify that the certificate is one we recognize.
            if (!certificate.Issuer.Contains(DevicePortalCertificateIssuer))
            {
                certificate = null;
                throw new DevicePortalException(
                    (Windows.Web.Http.HttpStatusCode)0,
                    "Invalid certificate issuer",
                    null,
                    "Failed to set the device certificate");
            }

            // Install the certificate.
            CertificateStore trustedStore = CertificateStores.TrustedRootCertificationAuthorities;
            trustedStore.Add(certificate);
        }

        /// <summary>
        /// Updates the device's connection Uri.
        /// </summary>
        /// <param name="requiresHttps">Indicates whether or not to always require a secure connection.</param>
        public void UpdateConnection(bool requiresHttps)
        {
            string uriScheme = this.GetUriScheme(
                this.Connection.Authority,
                requiresHttps);

            this.Connection = new Uri(
                string.Format(
                    "{0}://{1}",
                    uriScheme,
                    this.Connection.Authority));
        }

        /// <summary>
        /// Updates the device's connection Uri.
        /// </summary>
        /// <param name="ipConfig">The device's IP configuration data.</param>
        /// <param name="requiresHttps">Indicates whether or not the connection should always be secure.</param>
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
                        newConnection = new Uri(string.Format("{0}://{1}", this.GetUriScheme(addressInfo.Address, requiresHttps), addressInfo.Address));
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

        /// <summary>
        /// Gets the URI scheme based on the specified address.
        /// </summary>
        /// <param name="address">The address of the device.</param>
        /// <param name="requiresHttps">True if a secure connection should always be required.</param>
        /// <returns>A string containing the URI scheme.</returns>
        private string GetUriScheme(
            string address,
            bool requiresHttps = true)
        {
            return (address.Contains("127.0.0.1") ||
                    address.Contains("localhost") ||
                    !requiresHttps) ? "http" : "https";
        }
    }
}
