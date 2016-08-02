//----------------------------------------------------------------------------------------------
// <copyright file="DevicePortal.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.IO;
#if !WINDOWS_UWP
using System.Net;
#endif // !WINDOWS_UWP
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
#if WINDOWS_UWP
using Windows.Web.Http;
#endif // WINDOWS_UWP

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <summary>
    /// This is the main DevicePortal object. It contains methods for making HTTP REST calls against
    /// all of the WDP endpoints covered by the wrapper project. Different endpoints have their
    /// implementation separated out into individual files.
    /// </summary>
    public partial class DevicePortal
    {
        /// <summary>
        /// Issuer for the device certificate.
        /// </summary>
        public static readonly string DevicePortalCertificateIssuer = "Microsoft Windows Web Management";

        /// <summary>
        /// Endpoint used to access the certificate.
        /// </summary>
        private static readonly string RootCertificateEndpoint = "config/rootcertificate";

        /// <summary>
        /// Expected number of OS version sections once the OS version is split by period characters
        /// </summary>
        private static readonly uint ExpectedOSVersionSections = 5;

        /// <summary>
        /// The target OS version section index once the OS version is split by periods 
        /// </summary>
        private static readonly uint TargetOSVersionSection = 3;

        /// <summary>
        /// Device connection object.
        /// </summary>
        private IDevicePortalConnection deviceConnection;

        /// <summary>
        /// Initializes a new instance of the <see cref="DevicePortal" /> class.
        /// </summary>
        /// <param name="connection">Implementation of a connection object.</param>
        public DevicePortal(IDevicePortalConnection connection)
        {
            this.deviceConnection = connection;
        }

        /// <summary>
        /// HTTP Methods
        /// </summary>
        public enum HttpMethods
        {
            /// <summary>
            /// The HTTP Get method.
            /// </summary>
            Get,

            /// <summary>
            /// The HTTP Put method.
            /// </summary>
            Put,

            /// <summary>
            /// The HTTP Post method.
            /// </summary>
            Post,

            /// <summary>
            /// The HTTP Delete method.
            /// </summary>
            Delete,

            /// <summary>
            /// The HTTP WebSocket method.
            /// </summary>
            WebSocket
        }

        /// <summary>
        /// Gets the device address.
        /// </summary>
        public string Address 
        {
            get { return this.deviceConnection.Connection.Authority; }
        }
        
        /// <summary>
        /// Gets or sets handler for reporting connection status.
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
        /// Gets the device operating system family.
        /// </summary>
        public string DeviceFamily
        {
            get
            {
                return (this.deviceConnection.Family != null) ? this.deviceConnection.Family : string.Empty;
            }
        }

        /// <summary>
        /// Gets the operating system version.
        /// </summary>
        public string OperatingSystemVersion
        {
            get
            {
                return (this.deviceConnection.OsInfo != null) ? this.deviceConnection.OsInfo.OsVersionString : string.Empty;
            }
        }

        /// <summary>
        /// Gets the device platform.
        /// </summary>
        public DevicePortalPlatforms Platform
        {
            get
            {
                return (this.deviceConnection.OsInfo != null) ? this.deviceConnection.OsInfo.Platform : DevicePortalPlatforms.Unknown;
            }
        }

        /// <summary>
        /// Gets the device platform name.
        /// </summary>
        public string PlatformName
        {
            get
            {
                return (this.deviceConnection.OsInfo != null) ? this.deviceConnection.OsInfo.PlatformName : "Unknown";
            }
        }

        /// <summary>
        /// Connects to the device pointed to by IDevicePortalConnection provided in the constructor.
        /// </summary>
        /// <param name="ssid">Optional network SSID.</param>
        /// <param name="ssidKey">Optional network key.</param>
        /// <param name="updateConnection">Indicates whether we should update this connection's IP address after connecting.</param>
        /// <remarks>Connect sends ConnectionStatus events to indicate the current progress in the connection process.
        /// Some applications may opt to not register for the ConnectionStatus event and await on Connect.</remarks>
        /// <returns>Task for tracking the connect.</returns>
        public async Task Connect(
            string ssid = null,
            string ssidKey = null,
            bool updateConnection = true)
        {
#if WINDOWS_UWP
            this.ConnectionHttpStatusCode = HttpStatusCode.Ok;
#else
            this.ConnectionHttpStatusCode = HttpStatusCode.OK;
#endif // WINDOWS_UWP
            string connectionPhaseDescription = string.Empty;

            try 
            {
                // Get the device certificate
                bool certificateAcquired = false;
                try
                {
                    connectionPhaseDescription = "Acquiring device certificate";
                    this.SendConnectionStatus(
                        DeviceConnectionStatus.Connecting,
                        DeviceConnectionPhase.AcquiringCertificate,
                        connectionPhaseDescription);                  

                    this.deviceConnection.SetDeviceCertificate(await this.GetDeviceCertificate());

                    certificateAcquired = true;
                }
                catch
                {
                    // This device does not support the root certificate endpoint.
                    this.SendConnectionStatus(
                        DeviceConnectionStatus.Connecting,
                        DeviceConnectionPhase.AcquiringCertificate,
                        "No device certificate available");
                }

                // Get the device family and operating system information.
                connectionPhaseDescription = "Requesting operating system information";
                this.SendConnectionStatus(
                    DeviceConnectionStatus.Connecting,
                    DeviceConnectionPhase.RequestingOperatingSystemInformation,
                    connectionPhaseDescription);
                this.deviceConnection.Family = await this.GetDeviceFamily();
                this.deviceConnection.OsInfo = await this.GetOperatingSystemInformation();

                // Default to using HTTPS if we were successful in acquiring the device's root certificate.
                bool requiresHttps = certificateAcquired;

                // HoloLens is the only device that supports the GetIsHttpsRequired method.
                if (this.deviceConnection.OsInfo.Platform == DevicePortalPlatforms.HoloLens)
                {
                    // Check to see if HTTPS is required to communicate with this device.
                    connectionPhaseDescription = "Checking secure connection requirements";
                    this.SendConnectionStatus(
                        DeviceConnectionStatus.Connecting,
                        DeviceConnectionPhase.DeterminingConnectionRequirements,
                        connectionPhaseDescription);
                    requiresHttps = await this.GetIsHttpsRequired();
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
        /// Helper method used for saving the content of a response to a file.
        /// This allows unittests to easily generate real data to use as mock responses.
        /// </summary>
        /// <param name="endpoint">API endpoint we are calling.</param>
        /// <param name="directory">Directory to store our file.</param>
        /// <param name="httpMethod">The http method to be performed.</param>
        /// <returns>Task waiting for HTTP call to return and file copy to complete.</returns>
        public async Task SaveEndpointResponseToFile(string endpoint, string directory, HttpMethods httpMethod)
        {
            Uri uri = new Uri(this.deviceConnection.Connection, endpoint);

            // Convert the OS version, such as 14385.1002.amd64fre.rs1_xbox_rel_1608.160709-1700, into a friendly OS version, such as rs1_xbox_rel_1608
            string friendlyOSVersion = this.OperatingSystemVersion;
            string[] versionParts = friendlyOSVersion.Split('.');
            if (versionParts.Length == ExpectedOSVersionSections)
            {
                friendlyOSVersion = versionParts[TargetOSVersionSection];
            }

            // Create the filename as DeviceFamily_OSVersion.dat, replacing '/', '.', and '-' with '_' so
            // we can create a class with the same name as this Device/OS pair for tests.
            string filename = endpoint + "_" + this.Platform.ToString() + "_" + friendlyOSVersion;

            if (httpMethod != HttpMethods.Get)
            {
                filename = httpMethod.ToString() + "_" + filename;
            }

            Utilities.ModifyEndpointForFilename(ref filename);

            filename += ".dat";
            string filepath = Path.Combine(directory, filename);

            if (HttpMethods.WebSocket == httpMethod)
            {
#if WINDOWS_UWP
                WebSocket<object> websocket = new WebSocket<object>(this.deviceConnection, true);
#else
                WebSocket<object> websocket = new WebSocket<object>(this.deviceConnection, this.ServerCertificateValidation, true);
#endif // WINDOWS_UWP

                ManualResetEvent streamReceived = new ManualResetEvent(false);
                Stream stream = null;

                WindowsDevicePortal.WebSocketMessageReceivedEventHandler<Stream> streamReceivedHandler =
                    delegate(object sender, WebSocketMessageReceivedEventArgs<Stream> args)
                {
                    if (args.Message != null)
                    {
                        stream = args.Message;
                        streamReceived.Set();
                    }
                };

                websocket.WebSocketStreamReceived += streamReceivedHandler;

                Task startListeningForStreamTask = websocket.StartListeningForMessages(endpoint);
                startListeningForStreamTask.Wait();

                streamReceived.WaitOne();

                Task stopListeningForStreamTask = websocket.StopListeningForMessages();
                stopListeningForStreamTask.Wait();

                websocket.WebSocketStreamReceived -= streamReceivedHandler;

                using (stream)
                {
                    using (var fileStream = File.Create(filepath))
                    {
                        stream.Seek(0, SeekOrigin.Begin);
                        stream.CopyTo(fileStream);
                    }
                }
            }
            else if (HttpMethods.Put == httpMethod)
            {
                using (Stream dataStream = await this.Put(uri))
                {
                    using (var fileStream = File.Create(filepath))
                    {
                        dataStream.Seek(0, SeekOrigin.Begin);
                        dataStream.CopyTo(fileStream);
                    }
                }
            }
            else if (HttpMethods.Post == httpMethod)
            {
                using (Stream dataStream = await this.Post(uri))
                {
                    using (var fileStream = File.Create(filepath))
                    {
                        dataStream.Seek(0, SeekOrigin.Begin);
                        dataStream.CopyTo(fileStream);
                    }
                }
            }
            else if (HttpMethods.Delete == httpMethod)
            {
                using (Stream dataStream = await this.Delete(uri))
                {
                    using (var fileStream = File.Create(filepath))
                    {
                        dataStream.Seek(0, SeekOrigin.Begin);
                        dataStream.CopyTo(fileStream);
                    }
                }
            }
            else if (HttpMethods.Get == httpMethod)
            {
                using (Stream dataStream = await this.Get(uri))
                {
                    using (var fileStream = File.Create(filepath))
                    {
                        dataStream.Seek(0, SeekOrigin.Begin);
                        dataStream.CopyTo(fileStream);
                    }
                }
            }
            else
            {
                throw new NotImplementedException(string.Format("Unsupported HttpMethod {0}", httpMethod.ToString()));
            }
        }

        /// <summary>
        /// Sends the connection status back to the caller
        /// </summary>
        /// <param name="status">Status of the connect attempt.</param>
        /// <param name="phase">Current phase of the connection attempt.</param>
        /// <param name="message">Optional message describing the connection status.</param>
        private void SendConnectionStatus(
            DeviceConnectionStatus status,
            DeviceConnectionPhase phase,
            string message = "")
        {
            this.ConnectionStatus?.Invoke(this, new DeviceConnectionStatusEventArgs(status, phase, message));
        }
    }
}
