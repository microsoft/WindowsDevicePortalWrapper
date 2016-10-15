//----------------------------------------------------------------------------------------------
// <copyright file="DefaultDevicePortalConnection.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Net;
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
            // append auto- to the credentials to bypass CSRF token requirement on non-Get requests.
            this.Credentials = new NetworkCredential(string.Format("auto-{0}", userName), password);
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

                return new Uri(string.Format("{0}://{1}", scheme, this.Connection.Authority));
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
        /// <param name="ipConfig">Object that describes the current network configuration.</param>
        /// <param name="requiresHttps">True if an https connection is required, false otherwise.</param>
        /// <param name="preservePort">True if the previous connection's port is to continue to be used, false otherwise.</param>
        public void UpdateConnection(
            IpConfiguration ipConfig,
            bool requiresHttps,
            bool preservePort)
        {
            Uri newConnection = null;

            foreach (NetworkAdapterInfo adapter in ipConfig.Adapters)
            {
                foreach (IpAddressInfo addressInfo in adapter.IpAddresses)
                {
                    // We take the first, non-169.x.x.x address we find that is not 0.0.0.0.
                    if ((addressInfo.Address != "0.0.0.0") && !addressInfo.Address.StartsWith("169."))
                    {
                        string address = addressInfo.Address;
                        if (preservePort)
                        {
                            address = string.Format(
                                "{0}:{1}",
                                address,
                                this.Connection.Port);
                        }

                        newConnection = new Uri(
                            string.Format(
                                "{0}://{1}", 
                                requiresHttps ? "https" : "http",
                                address));
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
