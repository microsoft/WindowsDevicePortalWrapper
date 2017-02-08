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
        /// Web socket to get ETW events.!
        /// </summary>
        private WebSocket<EtwEvents> realtimeEventsWebSocket;

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
        /// Starts listening for ETW events with it being returned via the RealtimeEventsMessageReceived event handler.
        /// </summary>
        /// <returns>Task for connecting to the WebSocket but not for listening to it.</returns>
        public async Task StartListeningForEtwEventsAsync()
        {
            if (this.realtimeEventsWebSocket == null)
            {
#if WINDOWS_UWP
                this.realtimeEventsWebSocket = new WebSocket<EtwEvents>(this.deviceConnection);
#else
                this.realtimeEventsWebSocket = new WebSocket<EtwEvents>(this.deviceConnection, this.ServerCertificateValidation);
#endif

                this.realtimeEventsWebSocket.WebSocketMessageReceived += this.EtwEventsReceivedHandler;
            }
            else
            {
                if (this.realtimeEventsWebSocket.IsListeningForMessages)
                {
                    return;
                }
            }

            await this.realtimeEventsWebSocket.StartListeningForMessagesAsync(RealtimeEtwSessionApi);
        }

        /// <summary>
        /// Stop listening for ETW events.
        /// </summary>
        /// <returns>Task for stop listening for ETW events and disconnecting from the WebSocket.</returns>
        public async Task StopListeningForEtwEventsAsync()
        {
            if (this.realtimeEventsWebSocket == null || !this.realtimeEventsWebSocket.IsListeningForMessages)
            {
                return;
            }

            await this.realtimeEventsWebSocket.StopListeningForMessagesAsync();
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
        public class EtwProviderInfo
        {
            /// <summary>
            /// Gets provider guid.
            /// </summary>
            [DataMember(Name = "GUID")]
            public string GUID { get; private set; }

            /// <summary>
            /// Gets provider name.
            /// </summary>
            [DataMember(Name = "Name")]
            public string Name { get; private set; }
        }

#endregion // Data contract
    }
}
