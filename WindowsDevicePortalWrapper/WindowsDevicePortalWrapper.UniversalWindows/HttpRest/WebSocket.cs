//----------------------------------------------------------------------------------------------
// <copyright file="WebSocket.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Security.Credentials;
using Windows.Storage.Streams;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <summary>
    /// HTTP Websocket Wrapper
    /// </summary>
    /// <typeparam name="T">Return type for the websocket messages.</typeparam>
    internal partial class WebSocket<T>
    {
        /// <summary>
        /// The <see cref="MessageWebSocket" /> that is being wrapped.
        /// </summary>
        private MessageWebSocket websocket = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocket{T}" /> class.
        /// </summary>
        /// <param name="connection">Implementation of a connection object.</param>
        /// <param name="sendStreams">specifies whether the web socket should send streams (useful for creating mock data).</param>
        public WebSocket(IDevicePortalConnection connection, bool sendStreams = false)
        {
            this.sendStreams = sendStreams;
            this.deviceConnection = connection;
            this.IsListeningForMessages = false;
        }

        /// <summary>
        /// Opens a connection to the specified websocket API and starts listening for messages.
        /// </summary>
        /// <param name="endpoint">The uri that the weboscket should connect to</param>
        /// <returns>The task of opening a connection to the websocket.</returns>
        private async Task StartListeningForMessagesInternal(
            Uri endpoint)
        {
            this.websocket = new MessageWebSocket();

            this.websocket.Control.MessageType = SocketMessageType.Utf8;

            this.websocket.MessageReceived += this.MessageReceived;

            this.websocket.Closed += (senderSocket, args) =>
            {
                if (this.websocket != null)
                {
                    this.websocket.Dispose();
                    this.websocket = null;
                    this.IsListeningForMessages = false;
                }
            };

            if (this.deviceConnection.Credentials != null)
            {
                PasswordCredential cred = new PasswordCredential();
                cred.UserName = this.deviceConnection.Credentials.UserName;
                cred.Password = this.deviceConnection.Credentials.Password;

                this.websocket.Control.ServerCredential = cred;
            }

            this.websocket.SetRequestHeader("Origin", this.deviceConnection.Connection.AbsoluteUri);

            // Do not wait on receiving messages.
            Task connectTask = this.ConnectAsync(endpoint);
        }

        /// <summary>
        /// Opens a connection to the specified websocket API.
        /// </summary>
        /// <param name="endpoint">The uri that the weboscket should connect to</param>
        /// <returns>The task of opening a connection to the websocket.</returns>
        private async Task ConnectAsync(
            Uri endpoint)
        {
            bool connecting = true;
            try
            {
                await this.websocket.ConnectAsync(endpoint);
                connecting = false;
                this.IsListeningForMessages = true;
            }
            catch (Exception)
            {
                // Error happened during connect operation.
                if (connecting && this.websocket != null)
                {
                    this.websocket.Dispose();
                    this.websocket = null;
                    this.IsListeningForMessages = false;
                }
            }
        }

        /// <summary>
        /// Converts received stream to a parsed message and passes it to the WebSocketMessageReceived handler.
        /// </summary>
        /// <param name="sender">The  <see cref="MessageWebSocket" /> that sent the message.</param>
        /// <param name="args">The message from the web socket.</param>
        private void MessageReceived(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args)
        {
            using (IInputStream inputStream = args.GetDataStream())
            {
                Stream stream = new MemoryStream();

                Task copyTask = inputStream.AsStreamForRead().CopyToAsync(stream);
                copyTask.Wait();

                // Ensure we return with the stream pointed at the origin.
                stream.Position = 0;

                this.ConvertStreamToMessage(stream);
            }
        }

        /// <summary>
        /// Closes the connection to the websocket.
        /// </summary>
        /// <returns>The task of closing the websocket connection.</returns>
        private async Task StopListeningForMessagesInternal()
        {
            if (this.IsListeningForMessages)
            {
                if (this.websocket != null)
                {
                    // Code 1000 indicates that the purpose of the connection has been fulfilled and the connection is no longer needed.
                    this.websocket.Close(1000, "Closed due to user request.");
                    this.websocket = null;
                    this.IsListeningForMessages = false;
                }
            }
        }
    }
}
