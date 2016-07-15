﻿//----------------------------------------------------------------------------------------------
// <copyright file="WebSocket.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <summary>
    /// HTTP Websocket Wrapper
    /// </summary>
    /// <typeparam name="T">Return type for the websocket messages.</typeparam>
    internal partial class WebSocket<T>
    {
        /// <summary>
        /// The device that the <see cref="WebSocket{T}" /> is connected to.
        /// </summary>
        private IDevicePortalConnection deviceConnection;

        /// <summary>
        /// Gets a value indicating whether the web socket is listening for messages.
        /// </summary>
        public bool IsListeningForMessages
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the message received handler.
        /// </summary>
        public WebSocketMessageReceivedEventHandler<T> WebSocketMessageReceived
        {
            get;
            set;
        }

        /// <summary>
        /// Closes the connection to the websocket and stop listening for messages.
        /// </summary>
        /// <returns>The task of closing the websocket connection.</returns>
        internal async Task StopListeningForMessages()
        {
            if (this.IsListeningForMessages)
            {
                await this.StopListeningForMessagesInternal();
            }
        }

        /// <summary>
        /// Starts listening for messages from the websocket. Once they are received they are parsed and the WebSocketMessageReceived event is raised.
        /// </summary>
        /// <param name="apiPath">The relative portion of the uri path that specifies the API to call.</param>
        /// <param name="payload">The query string portion of the uri path that provides the parameterized data.</param>
        /// <returns>The task of listening for messages from the websocket.</returns>
        internal async Task StartListeningForMessages(
            string apiPath,
            string payload = null)
        {
            if (!this.IsListeningForMessages)
            {
                Uri uri = Utilities.BuildEndpoint(
                    this.deviceConnection.WebSocketConnection,
                    apiPath,
                    payload);

                await this.StartListeningForMessagesInternal(uri);
            }
        }

        /// <summary>
        /// Converts received stream to a parsed message and passes it to the WebSocketMessageReceived handler.
        /// </summary>
        /// <param name="stream">The received stream.</param>
        private void ConvertStreamToMessage(Stream stream)
        {
            if (stream != null && stream.Length != 0)
            {
                using (stream)
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
                    T message = (T)serializer.ReadObject(stream);

                    this.WebSocketMessageReceived?.Invoke(
                        this,
                        new WebSocketMessageReceivedEventArgs<T>(message));
                }
            }
        }
    }
}