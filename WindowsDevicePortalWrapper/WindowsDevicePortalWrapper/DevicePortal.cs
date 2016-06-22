//----------------------------------------------------------------------------------------------
// <copyright file="DevicePortal.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsDevicePortal
{
    using System;
    using System.IO;
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;

    /// <summary>
    /// DevicePortal object
    /// </summary>
    public partial class DevicePortal
    {
        /// <summary>
        /// Issuer for our Certificate
        /// </summary>
        public static readonly string DevicePortalCertificateIssuer = "Microsoft Windows Web Management";

        /// <summary>
        /// Endpoint for the certificate
        /// </summary>
        private static readonly string RootCertificateEndpoint = "config/rootcertificate";

        /// <summary>
        /// Device connection object
        /// </summary>
        private IDevicePortalConnection deviceConnection;

        /// <summary>
        /// Initializes a new instance of the <see cref="DevicePortal" /> class.
        /// </summary>
        /// <param name="connection">Implementation of a connection object</param>
        public DevicePortal(IDevicePortalConnection connection)
        {
            this.deviceConnection = connection;
        }

        /// <summary>
        /// Gets or sets handler for reporting connection status
        /// </summary>
        public DeviceConnectionStatusEventHandler ConnectionStatus
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the status code for establishing our connection.
        /// </summary>
        public HttpStatusCode ConnectionHttpStatusCode
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the device address
        /// </summary>
        public string Address 
        {
            get { return this.deviceConnection.Connection.Authority; }
        }
        
        /// <summary>
        /// Gets the Operating System Version
        /// </summary>
        public string OperatingSystemVersion
        {
            get
            {
                return (this.deviceConnection.OsInfo != null) ? this.deviceConnection.OsInfo.OsVersionString : string.Empty;
            }
        }

        /// <summary>
        /// Gets the platform
        /// </summary>
        public DevicePortalPlatforms Platform
        {
            get
            {
                return (this.deviceConnection.OsInfo != null) ? this.deviceConnection.OsInfo.Platform : DevicePortalPlatforms.Unknown;
            }
        }

        /// <summary>
        /// Connects to the device pointed to by IDevicePortalConnection provided in the constructor.
        /// </summary>
        /// <param name="ssid">Network SSID if desired</param>
        /// <param name="ssidKey">Network key if desired</param>
        /// <param name="updateConnection">Whether we should update this connection with SSID info</param>
        /// <remarks>Connect sends ConnectionStatus events to indicate the current progress in the connection process.
        /// Some applications may opt to not register for the ConnectionStatus event and await on Connect.</remarks>
        /// <returns>Task for tracking the connect.</returns>
        public async Task Connect(
            string ssid = null,
            string ssidKey = null,
            bool updateConnection = true)
        {
            this.ConnectionHttpStatusCode = HttpStatusCode.OK;
            string connectionPhaseDescription = string.Empty;

            // TODO - add status event. this can take a LONG time
            try 
            {
                // Get the device certificate
                connectionPhaseDescription = "Acquiring device certificate";
                this.SendConnectionStatus(
                    DeviceConnectionStatus.Connecting,
                    DeviceConnectionPhase.AcquiringCertificate,
                    connectionPhaseDescription);
                this.deviceConnection.SetDeviceCertificate(await this.GetDeviceCertificate());

                // Get the operating system information.
                connectionPhaseDescription = "Requesting operating system information";
                this.SendConnectionStatus(
                    DeviceConnectionStatus.Connecting,
                    DeviceConnectionPhase.RequestingOperatingSystemInformation,
                    connectionPhaseDescription);
                this.deviceConnection.OsInfo = await this.GetOperatingSystemInformation();

                bool requiresHttps = true;  // TODO - is this the correct default?

                // TODO: need a better check.
                if (this.deviceConnection.OsInfo.Platform != DevicePortalPlatforms.XboxOne)
                {
                    // Check to see if HTTPS is required to communicate with this device.
                    connectionPhaseDescription = "Checking secure connection requirements";
                    
                    try
                    {
                        requiresHttps = await this.GetIsHttpsRequired();
                    }
                    catch (NotSupportedException)
                    {
                    }
                }

                // Connect the device to the specified network.
                if (!string.IsNullOrWhiteSpace(ssid))
                {
                    connectionPhaseDescription = string.Format("Connecting to {0} network", ssid);
                    this.SendConnectionStatus(
                        DeviceConnectionStatus.Connecting,
                        DeviceConnectionPhase.ConnectingToTargetNetwork,
                        connectionPhaseDescription);
                    WifiInterfaces wifiInterfaces = await this.GetWifiInterfaces();

                    // TODO - consider what to do if there is more than one wifi interface on a device
                    await this.ConnectToWifiNetwork(wifiInterfaces.Interfaces[0].Guid, ssid, ssidKey);

                    // TODO - note that in some instances, the hololens was receiving a KeepAlive exception, yet the network connection succeeded. 
                    // this COULD have been an RTM bug that is now fixed, or it could have been the fault of the access point
                    // some investigation and defensive measures should be implemented here to avoid excessive noise / false failures
                }

                // Get the device's IP configuration and update the connection as appropriate.
                if (updateConnection)
                {
                    connectionPhaseDescription = "Updating device connection";
                    this.SendConnectionStatus(
                        DeviceConnectionStatus.Connecting,
                        DeviceConnectionPhase.UpdatingDeviceAddress,
                        connectionPhaseDescription);
                    this.deviceConnection.UpdateConnection(await this.GetIpConfig(), requiresHttps);
                }

                this.SendConnectionStatus(
                    DeviceConnectionStatus.Connected,
                    DeviceConnectionPhase.Idle,
                    "Device connection established");
            }
            catch (Exception e)
            {
                DevicePortalException dpe = e as DevicePortalException;

                if (dpe != null)
                {
                    this.ConnectionHttpStatusCode = dpe.StatusCode;
                }
                else
                {
                    this.ConnectionHttpStatusCode = (HttpStatusCode)0;
                }

                this.SendConnectionStatus(
                    DeviceConnectionStatus.Failed,
                    DeviceConnectionPhase.Idle,
                    string.Format("Device connection failed: {0}", connectionPhaseDescription));
            }
        }

        /// <summary>
        /// Gets the device certificate as a byte array
        /// </summary>
        /// <returns>Raw device certificate</returns>
        private async Task<byte[]> GetDeviceCertificate()
        {
            byte[] certificateData = null;
            bool useHttps = true;

            // try https then http
            while (true)
            {
                Uri uri = null;

                if (useHttps)
                {
                    uri = Utilities.BuildEndpoint(this.deviceConnection.Connection, RootCertificateEndpoint);
                }
                else
                {
                    Uri baseUri = new Uri(string.Format("http://{0}", this.deviceConnection.Connection.Authority));
                    uri = Utilities.BuildEndpoint(baseUri, RootCertificateEndpoint);
                }

                try
                {
                    using (Stream stream = await this.Get(uri, false))
                    {
                        using (BinaryReader reader = new BinaryReader(stream))
                        {
                            byte[] certData = reader.ReadBytes((int)stream.Length);

                            // Validate the issuer.
                            X509Certificate2 cert = new X509Certificate2(certData);
                            if (!cert.IssuerName.Name.Contains(DevicePortalCertificateIssuer))
                            {
                                throw new DevicePortalException(
                                    (HttpStatusCode)0,
                                    "Invalid certificate issuer",
                                    uri,
                                    "Failed to get device certificate");
                            }

                            certificateData = certData;
                        }
                    }

                    return certificateData;
                }
                catch (Exception e)
                {
                    if (useHttps)
                    {
                        useHttps = false;
                    }
                    else
                    {
                        throw e;
                    }
                }
            }
        }

        /// <summary>
        /// Sends the connection status back to the caller
        /// </summary>
        /// <param name="status">Status of the operation</param>
        /// <param name="phase">What phase the operation is on</param>
        /// <param name="message">Optional message</param>
        private void SendConnectionStatus(
            DeviceConnectionStatus status,
            DeviceConnectionPhase phase,
            string message = "")
        {
            this.ConnectionStatus?.Invoke(this, new DeviceConnectionStatusEventArgs(status, phase, message));
        }
    }
}
