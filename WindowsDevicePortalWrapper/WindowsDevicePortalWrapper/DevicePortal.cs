using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal
{
    public partial class DevicePortal
    {
        public static readonly String DevicePortalCertificateIssuer = "Microsoft Windows Web Management";

        private static readonly String _rootCertificateEndpoint = "config/rootcertificate";
        private IDevicePortalConnection _deviceConnection;

        public DeviceConnectionStatusEventHandler ConnectionStatus;

        public String Address 
        {
            get { return _deviceConnection.Connection.Authority; }
        }
        
        public String OperatingSystemVersion
        {
            get
            {
                return (_deviceConnection.OsInfo != null) ? _deviceConnection.OsInfo.OsVersionString : "";
            }
        }

        public DevicePortalPlatforms Platform
        {
            get
            {
                return (_deviceConnection.OsInfo != null) ? _deviceConnection.OsInfo.Platform : DevicePortalPlatforms.Unknown;
            }
        }
        
        public DevicePortal(IDevicePortalConnection connection)
        {
            _deviceConnection = connection;    
        }

        /// <summary>
        /// Connects to the device pointed to by IDevicePortalConnection provided in the constructor.
        /// </summary>
        /// <param name="ssid"></param>
        /// <param name="ssidKey"></param>
        /// <param name="updateConnection"></param>
        /// <remarks>Connect sends ConnectionStatus events to indicate the current progress in the connection process.
        /// Some applications may opt to not register for the ConnectionStatus event and await on Connect.</remarks>
        public async Task Connect(String ssid = null,
                                String ssidKey = null,
                                Boolean updateConnection = true)
        {
            String connectionPhaseDescription = String.Empty;

            // TODO - add status event. this can take a LONG time
            try 
            {
                // Get the device certificate
                connectionPhaseDescription = "Acquiring device certificate";
                SendConnectionStatus(DeviceConnectionStatus.Connecting,
                                    DeviceConnectionPhase.AcquiringCertificate,
                                    connectionPhaseDescription);
                _deviceConnection.SetDeviceCertificate(await GetDeviceCertificate());

                // Get the operating system information.
                connectionPhaseDescription = "Requesting operating system information";
                SendConnectionStatus(DeviceConnectionStatus.Connecting,
                                    DeviceConnectionPhase.RequestingOperatingSystemInformation,
                                    connectionPhaseDescription);
                _deviceConnection.OsInfo = await GetOperatingSystemInformation();

                Boolean requiresHttps = true;  // TODO - is this the correct default?
                if (_deviceConnection.OsInfo.Platform != DevicePortalPlatforms.XboxOne) // TODO: need a better check.
                {
                    // Check to see if HTTPS is required to communicate with this device.
                    connectionPhaseDescription = "Checking secure connection requirements";
                    
                    try
                    {
                        requiresHttps = await GetIsHttpsRequired();
                    }
                    catch (NotSupportedException)
                    { }
                }

                // Connect the device to the specified network.
                if (!String.IsNullOrWhiteSpace(ssid))
                {
                    connectionPhaseDescription = String.Format("Connecting to {0} network", ssid);
                    SendConnectionStatus(DeviceConnectionStatus.Connecting,
                                        DeviceConnectionPhase.ConnectingToTargetNetwork,
                                        connectionPhaseDescription);
                    WifiInterfaces wifiInterfaces = await GetWifiInterfaces();
                    // TODO - consider what to do if there is more than one wifi interface on a device
                    await ConnectToWifiNetwork(wifiInterfaces.Interfaces[0].Guid, ssid, ssidKey);

                    // TODO - note that in some instances, the hololens was receiving a KeepAlive exception, yet the network connection succeeded. 
                    // this COULD have been an RTM bug that is now fixed, or it could have been the fault of the access point
                    // some investigation and defensive measures should be implemented here to avoid excessive noise / false failures
                }

                // Get the device's IP configuration and update the connection as appropriate.
                if (updateConnection)
                {
                    connectionPhaseDescription = "Updating device connection";
                    SendConnectionStatus(DeviceConnectionStatus.Connecting,
                                        DeviceConnectionPhase.UpdatingDeviceAddress,
                                        connectionPhaseDescription);
                    _deviceConnection.UpdateConnection(await GetIpConfig(), requiresHttps);
                }

                SendConnectionStatus(DeviceConnectionStatus.Connected,
                                    DeviceConnectionPhase.Idle,
                                    "Device connection established");
            }
            catch(Exception e)
            {
                DevicePortalException dpe = e as DevicePortalException;

                HttpStatusCode status = (HttpStatusCode)0;
                Uri request = null;
                if (dpe != null)
                {
                    status = dpe.StatusCode;
                    request = dpe.RequestUri;
                }

                SendConnectionStatus(DeviceConnectionStatus.Failed,
                                    DeviceConnectionPhase.Idle,
                                    String.Format("Device connection failed: {0}", connectionPhaseDescription));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task<Byte[]> GetDeviceCertificate()
        {
            Byte[] certificateData = null;
            bool useHttps = true;

            // try https then http
            while (true)
            {
                Uri uri = null;

                if (useHttps)
                {
                    uri = Utilities.BuildEndpoint(_deviceConnection.Connection, _rootCertificateEndpoint);
                }
                else
                {
                    Uri baseUri = new Uri(String.Format("http://{0}", _deviceConnection.Connection.Authority));
                    uri = Utilities.BuildEndpoint(baseUri, _rootCertificateEndpoint);
                }

                try
                {
                    using (Stream stream = await Get(uri, false))
                    {
                        using (BinaryReader reader = new BinaryReader(stream))
                        {
                            Byte[] certData = reader.ReadBytes((Int32)stream.Length);

                            // Validate the issuer.
                            X509Certificate2 cert = new X509Certificate2(certData);
                            if (!cert.IssuerName.Name.Contains(DevicePortalCertificateIssuer))
                            {
                                throw new DevicePortalException((HttpStatusCode)0,
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
                    if(useHttps)
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
        /// 
        /// </summary>
        /// <param name="status"></param>
        /// <param name="phase"></param>
        /// <param name="message"></param>
        private void SendConnectionStatus(DeviceConnectionStatus status,
                                        DeviceConnectionPhase phase,
                                        String message = "")
        {
            ConnectionStatus?.Invoke(this, 
                                    new DeviceConnectionStatusEventArgs(status, phase, message));
        }
    }
}
