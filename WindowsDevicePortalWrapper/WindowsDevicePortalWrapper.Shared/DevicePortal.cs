//----------------------------------------------------------------------------------------------
// <copyright file="DevicePortal.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
#if !WINDOWS_UWP
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
#endif // !WINDOWS_UWP
#if WINDOWS_UWP
using System.Runtime.InteropServices.WindowsRuntime;
#endif // WINDOWS_UWP
#if !WINDOWS_UWP
using System.Security.Cryptography.X509Certificates;
#endif // !WINDOWS_UWP
using System.Threading;
using System.Threading.Tasks;
#if WINDOWS_UWP
using Windows.Security.Cryptography.Certificates;
using Windows.Web.Http;
using Windows.Web.Http.Headers;
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
        public static readonly string RootCertificateEndpoint = "config/rootcertificate";

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
#if !WINDOWS_UWP

        // Enable/disable useUnsafeHeaderParsing.
        // See https://social.msdn.microsoft.com/Forums/en-US/ff098248-551c-4da9-8ba5-358a9f8ccc57/how-do-i-enable-useunsafeheaderparsing-from-code-net-20?forum=netfxnetcom
        private static bool ToggleAllowUnsafeHeaderParsing(bool enable)
        {
            Type settingsSectionType = Assembly.GetAssembly(typeof(System.Net.Configuration.SettingsSection))?.GetType("System.Net.Configuration.SettingsSectionInternal");
            if (settingsSectionType == null) { return false; }

            object anInstance = settingsSectionType.InvokeMember("Section", BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.NonPublic, null, null, new object[] { });
            if (anInstance == null) { return false; }

            FieldInfo aUseUnsafeHeaderParsing = settingsSectionType.GetField("useUnsafeHeaderParsing", BindingFlags.NonPublic | BindingFlags.Instance);
            if (aUseUnsafeHeaderParsing == null) { return false; }

            aUseUnsafeHeaderParsing.SetValue(anInstance, enable);

            return true;
        }

        /// <summary>
        /// Initializes static members of the <see cref="DevicePortal" /> class.
        /// </summary>
        static DevicePortal()
        {
            System.Net.ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            if (!ToggleAllowUnsafeHeaderParsing(true))
            {
                Console.WriteLine("Failed to enable useUnsafeHeaderParsing");
            }
        }

#endif
        /// <summary>
        /// Initializes a new instance of the <see cref="DevicePortal" /> class.
        /// </summary>
        /// <param name="connection">Implementation of a connection object.</param>
        public DevicePortal(IDevicePortalConnection connection)
        {
            this.deviceConnection = connection;
        }

        /// <summary>
        /// Handler for reporting connection status.
        /// </summary>
        public event DeviceConnectionStatusEventHandler ConnectionStatus;

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
        /// Gets the status code for establishing our connection.
        /// </summary>
        public HttpStatusCode ConnectionHttpStatusCode
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a description of why the connection failed.
        /// </summary>
        public string ConnectionFailedDescription
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
        /// <param name="manualCertificate">A manually provided X509 Certificate for trust validation against this device.</param>
        /// <remarks>Connect sends ConnectionStatus events to indicate the current progress in the connection process.
        /// Some applications may opt to not register for the ConnectionStatus event and await on Connect.</remarks>
        /// <returns>Task for tracking the connect.</returns>
        [method: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:ParameterMustNotSpanMultipleLines", Justification = "manualCertificate param doesn't really span multiple lines, it just has a different type for UWP and .NET implementations.")]
        public async Task ConnectAsync(
            string ssid = null,
            string ssidKey = null,
            bool updateConnection = false,
#if WINDOWS_UWP
            Certificate manualCertificate = null)
#else
            X509Certificate2 manualCertificate = null)
#endif
        {
#if WINDOWS_UWP
            this.ConnectionHttpStatusCode = HttpStatusCode.Ok;
#else
            this.ConnectionHttpStatusCode = HttpStatusCode.OK;
#endif // WINDOWS_UWP
            string connectionPhaseDescription = string.Empty;

            if (manualCertificate != null)
            {
                this.SetManualCertificate(manualCertificate);
            }

            try 
            {
                // Get the device family and operating system information.
                connectionPhaseDescription = "Requesting operating system information";
                this.SendConnectionStatus(
                    DeviceConnectionStatus.Connecting,
                    DeviceConnectionPhase.RequestingOperatingSystemInformation,
                    connectionPhaseDescription);
                this.deviceConnection.Family = await this.GetDeviceFamilyAsync().ConfigureAwait(false);
                this.deviceConnection.OsInfo = await this.GetOperatingSystemInformationAsync().ConfigureAwait(false);

                // Default to using whatever was specified in the connection.
                bool requiresHttps = this.IsUsingHttps();

                // HoloLens is the only device that supports the GetIsHttpsRequired method.
                if (this.deviceConnection.OsInfo.Platform == DevicePortalPlatforms.HoloLens)
                {
                    // Check to see if HTTPS is required to communicate with this device.
                    connectionPhaseDescription = "Checking secure connection requirements";
                    this.SendConnectionStatus(
                        DeviceConnectionStatus.Connecting,
                        DeviceConnectionPhase.DeterminingConnectionRequirements,
                        connectionPhaseDescription);
                    requiresHttps = await this.GetIsHttpsRequiredAsync().ConfigureAwait(false);
                }

                // Connect the device to the specified network.
                if (!string.IsNullOrWhiteSpace(ssid))
                {
                    connectionPhaseDescription = string.Format("Connecting to {0} network", ssid);
                    this.SendConnectionStatus(
                        DeviceConnectionStatus.Connecting,
                        DeviceConnectionPhase.ConnectingToTargetNetwork,
                        connectionPhaseDescription);
                    WifiInterfaces wifiInterfaces = await this.GetWifiInterfacesAsync().ConfigureAwait(false);

                    // TODO - consider what to do if there is more than one wifi interface on a device
                    await this.ConnectToWifiNetworkAsync(wifiInterfaces.Interfaces[0].Guid, ssid, ssidKey).ConfigureAwait(false);
                }

                // Get the device's IP configuration and update the connection as appropriate.
                if (updateConnection)
                {
                    connectionPhaseDescription = "Updating device connection";
                    this.SendConnectionStatus(
                        DeviceConnectionStatus.Connecting,
                        DeviceConnectionPhase.UpdatingDeviceAddress,
                        connectionPhaseDescription);
                    
                    bool preservePort = true;

                    // HoloLens and Mobile are the only devices that support USB.
                    // They require the port to be changed when the connection is updated
                    // to WiFi.
                    if ((this.Platform == DevicePortalPlatforms.HoloLens) ||
                        (this.Platform == DevicePortalPlatforms.Mobile))
                    {
                        preservePort = false;
                    }

                    this.deviceConnection.UpdateConnection(
                        await this.GetIpConfigAsync().ConfigureAwait(false), 
                        requiresHttps,
                        preservePort);
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
                    this.ConnectionFailedDescription = dpe.Message;
                }
                else
                {
                    this.ConnectionHttpStatusCode = HttpStatusCode.Conflict;

                    // Get to the innermost exception for our return message.
                    Exception innermostException = e;
                    while (innermostException.InnerException != null)
                    {
                        innermostException = innermostException.InnerException;
                        await Task.Yield();
                    }

                    this.ConnectionFailedDescription = innermostException.Message;
                }

                this.SendConnectionStatus(
                    DeviceConnectionStatus.Failed,
                    DeviceConnectionPhase.Idle,
                    string.Format("Device connection failed: {0}, {1}", connectionPhaseDescription, this.ConnectionFailedDescription));
            }
        }

        /// <summary>
        /// Helper method used for saving the content of a response to a file.
        /// This allows unittests to easily generate real data to use as mock responses.
        /// </summary>
        /// <param name="endpoint">API endpoint we are calling.</param>
        /// <param name="directory">Directory to store our file.</param>
        /// <param name="httpMethod">The http method to be performed.</param>
        /// <param name="requestBody">An optional stream to use for the request body content.</param>
        /// <param name="requestBodyContentType">The content type of the request stream.</param>
        /// <returns>Task waiting for HTTP call to return and file copy to complete.</returns>
        public async Task SaveEndpointResponseToFileAsync(
            string endpoint,
            string directory,
            HttpMethods httpMethod,
            Stream requestBody = null,
            string requestBodyContentType = null)
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

                WindowsDevicePortal.WebSocketStreamReceivedEventInternalHandler<object> streamReceivedHandler =
                    delegate(WebSocket<object> sender, WebSocketMessageReceivedEventArgs<Stream> args)
                {
                    if (args.Message != null)
                    {
                        stream = args.Message;
                        streamReceived.Set();
                    }
                };

                websocket.WebSocketStreamReceived += streamReceivedHandler;

                await websocket.ConnectAsync(endpoint);

                await websocket.ReceiveMessagesAsync();

                streamReceived.WaitOne();

                await websocket.CloseAsync();

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
#if WINDOWS_UWP
                HttpStreamContent streamContent = null;
#else
                StreamContent streamContent = null;
#endif // WINDOWS_UWP

                if (requestBody != null)
                {
#if WINDOWS_UWP
                streamContent = new HttpStreamContent(requestBody.AsInputStream());
                streamContent.Headers.ContentType = new HttpMediaTypeHeaderValue(requestBodyContentType);
#else
                    streamContent = new StreamContent(requestBody);
                    streamContent.Headers.ContentType = new MediaTypeHeaderValue(requestBodyContentType);
#endif // WINDOWS_UWP
                }

                using (Stream dataStream = await this.PutAsync(uri, streamContent))
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
                using (Stream dataStream = await this.PostAsync(uri, requestBody, requestBodyContentType))
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
                using (Stream dataStream = await this.DeleteAsync(uri))
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
                using (Stream dataStream = await this.GetAsync(uri))
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

        /// <summary>
        /// Helper method to determine if our connection is using HTTPS
        /// </summary>
        /// <returns>Whether we are using HTTPS</returns>
        private bool IsUsingHttps()
        {
            return this.deviceConnection.Connection.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase);
        }
    }
}
