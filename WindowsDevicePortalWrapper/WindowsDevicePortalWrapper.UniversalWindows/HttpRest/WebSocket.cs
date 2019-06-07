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
        /// The websocket connection has closed after the request was fulfilled.
        /// </summary>
        private const ushort WebSocketCloseStatusNormalClosure = 1000;

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
        /// Opens a connection to the specified websocket API.
        /// </summary>
        /// <param name="endpoint">The uri that the weboscket should connect to</param>
        /// <returns>The task of opening a connection to the websocket.</returns>
        private async Task ConnectInternalAsync(
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

            //Origin address must be especially cooked to pass through all Device Portal checks
            string OriginAddress = this.deviceConnection.Connection.Scheme + "://" + this.deviceConnection.Connection.Host;
            if ((this.deviceConnection.Connection.Scheme == "http" && this.deviceConnection.Connection.Port != 80) ||
                (this.deviceConnection.Connection.Scheme == "https" && this.deviceConnection.Connection.Port != 443))
            {
                OriginAddress += ":" + this.deviceConnection.Connection.Port;
            }
            this.websocket.SetRequestHeader("Origin", OriginAddress);

            await this.websocket.ConnectAsync(endpoint);

            this.IsConnected = true;
        }

        /// <summary>
        /// Closes the connection to the websocket.
        /// </summary>
        /// <returns>The task of closing the websocket connection.</returns>
        private async Task CloseInternalAsync()
        {
            await Task.Run(() =>
            {
                this.websocket.Close(WebSocketCloseStatusNormalClosure, "Closed due to user request.");
                this.websocket.Dispose();
                this.websocket = null;
                this.IsConnected = false;
            });
        }

        /// <summary>
        /// Opens a connection to the specified websocket API and starts listening for messages.
        /// </summary>
        /// <returns>The task of opening a connection to the websocket.</returns>
        private async Task StartListeningForMessagesInternalAsync()
        {
            await Task.Run(() =>
            {
                this.IsListeningForMessages = true;
            });
        }

        /// <summary>
        /// Converts received stream to a parsed message and passes it to the WebSocketMessageReceived handler.
        /// </summary>
        /// <param name="sender">The  <see cref="MessageWebSocket" /> that sent the message.</param>
        /// <param name="args">The message from the web socket.</param>
        private async void MessageReceived(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args)
        {
            if (this.IsListeningForMessages)
            {
                using (IInputStream inputStream = args.GetDataStream())
                {
                    Stream stream = new MemoryStream();

                    await inputStream.AsStreamForRead().CopyToAsync(stream);

                    // Ensure we return with the stream pointed at the origin.
                    stream.Position = 0;

                    this.ConvertStreamToMessage(stream);
                }
            }
        }

        /// <summary>
        /// Stops listening for messages from the websocket.
        /// </summary>
        /// <returns>The task of closing the websocket connection.</returns>
        private async Task StopListeningForMessagesInternalAsync()
        {
            await Task.Run(() =>
            {
                this.IsListeningForMessages = false;
            });
        }

        /// <summary>
        /// Sends a message to the websocket.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        /// <returns>The task of sending the specified message to the server.</returns>
        private async Task SendMessageInternalAsync(string message)
        {
            using (DataWriter data = new DataWriter(this.websocket.OutputStream))
            {
                // Load the content into the data writer.
                data.UnicodeEncoding = UnicodeEncoding.Utf8;
                data.WriteString(message);

                // Send the content to the output stream.
                await data.StoreAsync();
                await data.FlushAsync();

                // Do not close the output stream when the data writer is disposed.
                data.DetachStream();
            }
        }
    }
}
