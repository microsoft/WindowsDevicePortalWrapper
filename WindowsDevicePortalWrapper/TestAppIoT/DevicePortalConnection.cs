//----------------------------------------------------------------------------------------------
// <copyright file="DevicePortalConnection.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.Tools.WindowsDevicePortal;
using static Microsoft.Tools.WindowsDevicePortal.DevicePortal;

namespace TestAppIoT
{
    /// <summary>
    /// IDevicePortalConnection implementation for the HoloLens test project
    /// </summary>
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

            this.Connection = new Uri(string.Format("http://{0}", address));
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
        /// Gets the raw device certificate.
        /// </summary>
        /// <returns>Byte array containing the raw certificate data.</returns>
        public byte[] GetDeviceCertificateData()
        {
            throw new NotSupportedException("The current version of TestAppIoT does not support device certificates");
        }

        /// <summary>
        /// Validates and sets the device certificate.
        /// </summary>
        /// <param name="certificate">The device's root certificate.</param>
        public void SetDeviceCertificate(System.Security.Cryptography.X509Certificates.X509Certificate2 certificate)
        {
            throw new NotSupportedException("The current version of TestAppIoT does not support device certificates");
        }

        /// <summary>
        /// Updates the device's connection Uri.
        /// </summary>
        /// <param name="requiresHttps">Indicates whether or not to always require a secure connection.</param>
        public void UpdateConnection(bool requiresHttps = false)
        {
            this.Connection = new Uri(
                string.Format(
                    "http://{0}", 
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
                        newConnection = new Uri(string.Format("{0}://{1}", "http", addressInfo.Address));
                        
                        // TODO qualified name
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
