//----------------------------------------------------------------------------------------------
// <copyright file="WebSocket.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <summary>
    /// Internal Web socket message received event handler
    /// </summary>
    /// <param name="sender">sender <see cref="WebSocket{T}"/> object</param>
    /// <param name="args">Web socket message received args</param>
    /// <typeparam name="T">Return type for the websocket messages.</typeparam>
    internal delegate void WebSocketMessageReceivedEventInternalHandler<T>(WebSocket<T> sender, WebSocketMessageReceivedEventArgs<T> args);

    /// <summary>
    /// Internal Web socket stream received event handler
    /// </summary>
    /// <param name="sender">sender <see cref="WebSocket{T}"/> object</param>
    /// <param name="args">Web socket message received args</param>
    /// <typeparam name="T">Return type for the websocket.</typeparam>
    internal delegate void WebSocketStreamReceivedEventInternalHandler<T>(WebSocket<T> sender, WebSocketMessageReceivedEventArgs<Stream> args);

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
        /// Indicates whether the web socket should send streams instead of parsed <see cref="T" /> objects
        /// </summary>
        private bool sendStreams = false;

        /// <summary>
        /// Gets or sets the message received handler.
        /// </summary>
        public event WebSocketMessageReceivedEventInternalHandler<T> WebSocketMessageReceived;

        /// <summary>
        /// Gets or sets the stream received handler.
        /// </summary>
        public event WebSocketStreamReceivedEventInternalHandler<T> WebSocketStreamReceived;

        /// <summary>
        /// Gets a value indicating whether the web socket is connected.
        /// </summary>
        public bool IsConnected
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether the web socket is listening for messages.
        /// </summary>
        public bool IsListeningForMessages
        {
            get;
            private set;
        }

        /// <summary>
        /// Initialize a connection to the websocket.
        /// </summary>
        /// <param name="apiPath">The relative portion of the uri path that specifies the API to call.</param>
        /// <param name="payload">The query string portion of the uri path that provides the parameterized data.</param>
        /// <returns>The task of opening the websocket connection.</returns>
        internal async Task ConnectAsync(string apiPath, string payload = null)
        {
            if (this.IsConnected)
            {
                return;
            }

            Uri uri = Utilities.BuildEndpoint(
                this.deviceConnection.WebSocketConnection,
                apiPath,
                payload);
            await this.ConnectInternalAsync(uri);
        }

        /// <summary>
        /// Closes the connection to the websocket and stop listening for messages.
        /// </summary>
        /// <returns>The task of closing the websocket connection.</returns>
        internal async Task CloseAsync()
        {
            if (this.IsConnected)
            {
                if (this.IsListeningForMessages)
                {
                    await this.StopListeningForMessagesInternalAsync();
                }

                await this.CloseInternalAsync();
            }
        }

        /// <summary>
        /// Starts listening for messages from the websocket. Once they are received they are parsed and the WebSocketMessageReceived event is raised.
        /// </summary>
        /// <returns>The task of listening for messages from the websocket.</returns>
        internal async Task ReceiveMessagesAsync()
        {
            if (this.IsConnected && !this.IsListeningForMessages)
            {
                await this.StartListeningForMessagesInternalAsync();
            }
        }

        /// <summary>
        /// Sends a message to the server.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <returns>The task of sending a message to the websocket.</returns>
        internal async Task SendMessageAsync(string message)
        {
            if (this.IsConnected)
            {
                await this.SendMessageInternalAsync(message);
            }
        }

        /// <summary>
        /// Converts received stream to a parsed <see cref="T" /> object and passes it to
        /// the WebSocketMessageReceived handler. The sendstreams property can be used to
        /// override this and send the <see cref="Stream" /> instead via the WebSocketStreamReceived handler.
        /// </summary>
        /// <param name="stream">The received stream.</param>
        private void ConvertStreamToMessage(Stream stream)
        {
            if (stream != null && stream.Length != 0)
            {
                if (this.sendStreams)
                {
                    this.WebSocketStreamReceived?.Invoke(
                            this,
                            new WebSocketMessageReceivedEventArgs<Stream>(stream));
                }
                else
                {
                    DataContractJsonSerializerSettings settings = new DataContractJsonSerializerSettings()
                    {
                        UseSimpleDictionaryFormat = true
                    };

                    T message = DevicePortal.ReadJsonStream<T>(stream, settings);

                    this.WebSocketMessageReceived?.Invoke(
                        this,
                        new WebSocketMessageReceivedEventArgs<T>(message));
                }
            }
        }
    }
}