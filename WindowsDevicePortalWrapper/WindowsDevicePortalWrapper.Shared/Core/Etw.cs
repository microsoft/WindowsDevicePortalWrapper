//----------------------------------------------------------------------------------------------
// <copyright file="Etw.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <content>
    /// Wrappers for ETW methods
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// API to create a realtime ETW session.
        /// </summary>
        public static readonly string RealtimeEtwSessionApi = "api/etw/session/realtime";

        /// <summary>
        /// API to get the list of registered custom ETW providers.
        /// </summary>
        public static readonly string CustomEtwProvidersApi = "api/etw/customproviders";

        /// <summary>
        /// API to get the list of registered ETW providers.
        /// </summary>
        public static readonly string EtwProvidersApi = "api/etw/providers";

        /// <summary>
        /// Web socket to get ETW events.
        /// </summary>
        private WebSocket<EtwEvents> realtimeEventsWebSocket;

        /// <summary>
        /// Determines if the event listener has been registered
        /// </summary>
        private bool isListeningForRealtimeEvents = false;

        /// <summary>
        /// The ETW event message received handler
        /// </summary>
        public event WebSocketMessageReceivedEventHandler<EtwEvents> RealtimeEventsMessageReceived;

        /// <summary>
        /// Returns the current set of registered custom ETW providers.
        /// </summary>
        /// <returns>EtwProviders object containing a list of providers, friendly name and GUID</returns>
        public async Task<EtwProviders> GetCustomEtwProvidersAsync()
        {
            return await this.GetAsync<EtwProviders>(CustomEtwProvidersApi);
        }

        /// <summary>
        /// Returns the current set of registered ETW providers.
        /// </summary>
        /// <returns>EtwProviders object containing a list of providers, friendly name and GUID</returns>
        public async Task<EtwProviders> GetEtwProvidersAsync()
        {
            return await this.GetAsync<EtwProviders>(EtwProvidersApi);
        }

        /// <summary>
        /// Toggles the listening state of a specific provider on the realtime events WebSocket.
        /// </summary>
        /// <param name="etwProvider">The provider to update the listening state of.</param>
        /// <param name="isEnabled">Determines whether the listening state should be enabled or disabled.</param>
        /// /// <param name="level">Verbosity level - 1 for least, 5 for most verbose. </param>
        /// <returns>Task for toggling the listening state of the specified provider.</returns>
        public async Task ToggleEtwProviderAsync(EtwProviderInfo etwProvider, bool isEnabled = true, int level = 5)
        {
            await this.ToggleEtwProviderAsync(etwProvider.GUID, isEnabled, level);
        }

        /// <summary>
        /// Toggles the listening state of a specific provider on the realtime events WebSocket.
        /// </summary>
        /// <param name="etwProvider">The GUID of the provider to update the listening state of.</param>
        /// <param name="isEnabled">Determines whether the listening state should be enabled or disabled.</param>
        /// <param name="level">Verbosity level - 1 for least, 5 for most verbose. </param>
        /// <returns>Task for toggling the listening state of the specified provider.</returns>
        public async Task ToggleEtwProviderAsync(Guid etwProvider, bool isEnabled = true, int level = 5)
        {
            string action = isEnabled ? "enable" : "disable";
            string message = $"provider {etwProvider} {action} {level}";

            await this.InitializeRealtimeEventsWebSocketAsync();
            await this.realtimeEventsWebSocket.SendMessageAsync(message);
        }

        /// <summary>
        /// Starts listening for ETW events with it being returned via the RealtimeEventsMessageReceived event handler.
        /// </summary>
        /// <returns>Task for connecting to the WebSocket but not for listening to it.</returns>
        public async Task StartListeningForEtwEventsAsync()
        {
            await this.InitializeRealtimeEventsWebSocketAsync();

            if (!this.isListeningForRealtimeEvents)
            {
                this.isListeningForRealtimeEvents = true;
                this.realtimeEventsWebSocket.WebSocketMessageReceived += this.EtwEventsReceivedHandler;
            }

            await this.realtimeEventsWebSocket.ReceiveMessagesAsync();
        }

        /// <summary>
        /// Stop listening for ETW events.
        /// </summary>
        /// <returns>Task for stop listening for ETW events and disconnecting from the WebSocket.</returns>
        public async Task StopListeningForEtwEventsAsync()
        {
            if (this.isListeningForRealtimeEvents)
            {
                this.isListeningForRealtimeEvents = false;
                this.realtimeEventsWebSocket.WebSocketMessageReceived -= this.EtwEventsReceivedHandler;
            }

            await this.realtimeEventsWebSocket.CloseAsync();
        }

        /// <summary>
        /// Creates a new <see cref="WebSocket{EtwEvents}"/> if it hasn't already been initialized.
        /// </summary>
        /// <returns>Task for connecting the ETW realtime event WebSocket.</returns>
        private async Task InitializeRealtimeEventsWebSocketAsync()
        {
            if (this.realtimeEventsWebSocket == null)
            {
#if WINDOWS_UWP
                this.realtimeEventsWebSocket = new WebSocket<EtwEvents>(this.deviceConnection);
#else
                this.realtimeEventsWebSocket = new WebSocket<EtwEvents>(this.deviceConnection, this.ServerCertificateValidation);
#endif
            }

            await this.realtimeEventsWebSocket.ConnectAsync(RealtimeEtwSessionApi);
        }

        /// <summary>
        /// Handler for the ETW received event that passes the event to the RealtimeEventsMessageReceived handler.
        /// </summary>
        /// <param name="sender">The <see cref="WebSocket{EtwEvents}"/> object sending the event.</param>
        /// <param name="args">The event data.</param>
        private void EtwEventsReceivedHandler(
            WebSocket<EtwEvents> sender,
            WebSocketMessageReceivedEventArgs<EtwEvents> args)
        {
            if (args.Message != null)
            {
                this.RealtimeEventsMessageReceived?.Invoke(
                    this,
                    args);
            }
        }

        #region Data contract

        /// <summary>
        /// ETW Events.
        /// </summary>
        [DataContract]
        public class EtwEvents
        {
            /// <summary>
            /// Saves the downconverted list of events 
            /// </summary>
            private List<EtwEventInfo> stashedList;

            /// <summary>
            /// Gets the list of ETW Events that occured in the last second. 
            /// </summary>
            public List<EtwEventInfo> Events
            {
                get
                {
                    if (this.stashedList != null)
                    {
                        return this.stashedList;
                    }

                    List<EtwEventInfo> events = new List<EtwEventInfo>();
                    foreach (Dictionary<string, string> dic in this.RawEvents)
                    {
                        events.Add(new EtwEventInfo(dic));
                    }

                    this.stashedList = events;
                    return this.stashedList;
                }
            }

            /// <summary>
            /// Gets the event frequency. 
            /// This is always 10 million (10000000) in RS2 devices.  
            /// </summary>
            [DataMember(Name = "Frequency")]
            public long Frequency { get; private set; }

            /// <summary>
            /// Gets or sets the raw list of events.  Not for straight usage, as it's entirely unformatted. 
            /// </summary>
            [DataMember(Name = "Events")]
            private List<Dictionary<string, string>> RawEvents { get; set; }
        }

        /// <summary>
        /// ETW Events Info. Allows strongly typed access to guaranteed fields 
        /// like ID or Timestamp, and raw (as string) access to all other 
        /// payload data, like Latency or PID. 
        /// </summary>
        public class EtwEventInfo : Dictionary<string, string>
        {
            /// <summary>
            ///  Initializes a new instance of the <see cref="EtwEventInfo" /> class.  Used by the DataContract at access time. 
            /// </summary>
            /// <param name="dictionary">Base dictionary used to populate the object. </param>
            internal EtwEventInfo(IDictionary<string, string> dictionary) : base(dictionary)
            {
            }

            /// <summary>
            /// Gets the event identifer.
            /// </summary>
            public ushort ID
            {
                get
                {
                    return ushort.Parse(this["ID"]);
                }
            }

            /// <summary>
            /// Gets the event keyword.
            /// </summary>
            public ulong Keyword
            {
                get
                {
                    return ulong.Parse(this["Keyword"]);
                }
            }

            /// <summary>
            /// Gets the event level.
            /// </summary>
            public uint Level
            {
                get
                {
                    return uint.Parse(this["Level"]);
                }
            }

            /// <summary>
            /// Gets the event provider name.
            /// </summary>
            public string Provider
            {
                get
                {
                    return this["ProviderName"];
                }
            }

            /// <summary>
            /// Gets the event timestamp. 
            /// </summary>
            public ulong Timestamp
            {
                get
                {
                    return ulong.Parse(this["Timestamp"]);
                }
            }
        }

        /// <summary>
        /// ETW Providers.
        /// </summary>
        [DataContract]
        public class EtwProviders
        {
            /// <summary>
            /// Gets list of ETW providers
            /// </summary>
            [DataMember(Name = "Providers")]
            public List<EtwProviderInfo> Providers { get; private set; }
        }

        /// <summary>
        /// ETW Provider Info. Contains the Name and GUID.
        /// </summary>
        [DataContract]
        public class EtwProviderInfo
        {
            /// <summary>
            /// Gets provider guid.
            /// </summary>
            [DataMember(Name = "GUID")]
            public Guid GUID { get; private set; }

            /// <summary>
            /// Gets provider name.
            /// </summary>
            [DataMember(Name = "Name")]
            public string Name { get; private set; }
        }

#endregion // Data contract
    }
}
