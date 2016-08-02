//----------------------------------------------------------------------------------------------
// <copyright file="DefaultDevicePortalConnection.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Net;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.Security.Cryptography.Certificates;
using Windows.Storage.Streams;
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
            this.Credentials = new NetworkCredential(userName, password);
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
                        newConnection = new Uri(
                            string.Format(
                                "{0}://{1}", 
                                requiresHttps ? "https" : "http",
                                this.Connection.Authority));
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
