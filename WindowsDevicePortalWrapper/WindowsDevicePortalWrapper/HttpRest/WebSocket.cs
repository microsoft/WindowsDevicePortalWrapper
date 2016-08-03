//----------------------------------------------------------------------------------------------
// <copyright file="WebSocket.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
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
        /// The hresult for the connection being reset by the peer.
        /// </summary>
        private const int WSAECONNRESET = 0x2746;

        /// <summary>
        /// The maximum number of bytes that can be received in a single chunk.
        /// </summary>
        private static readonly uint MaxChunkSizeInBytes = 1024;

        /// <summary>
        /// The <see cref="ClientWebSocket" /> that is being wrapped.
        /// </summary>
        private ClientWebSocket websocket = new ClientWebSocket();

        /// <summary>
        /// <see cref="ManualResetEvent" /> used to indicate that the <see cref="WebSocket{T}" /> has stopped receiving messages.
        /// </summary>
        private ManualResetEvent stoppedReceivingMessages = new ManualResetEvent(false);

        /// <summary>
        /// The handler used to validate server certificates.
        /// </summary>
        private Func<object, X509Certificate, X509Chain, SslPolicyErrors, bool> serverCertificateValidationHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocket{T}" /> class.
        /// </summary>
        /// <param name="connection">Implementation of a connection object.</param>
        /// <param name="serverCertificateValidationHandler">Server certificate handler.</param>
        /// <param name="sendStreams">specifies whether the web socket should send streams (useful for creating mock data).</param>
        public WebSocket(IDevicePortalConnection connection, Func<object, X509Certificate, X509Chain, SslPolicyErrors, bool> serverCertificateValidationHandler, bool sendStreams = false)
        {
            this.sendStreams = sendStreams;
            this.deviceConnection = connection;
            this.IsListeningForMessages = false;
            this.serverCertificateValidationHandler = serverCertificateValidationHandler;
        }

        /// <summary>
        /// Opens a connection to the specified websocket API and starts listening for messages.
        /// </summary>
        /// <param name="endpoint">The uri that the weboscket should connect to</param>
        /// <returns>The task of opening a connection to the websocket.</returns>
        private async Task OpenConnection(
            Uri endpoint)
        {
            this.websocket.Options.UseDefaultCredentials = false;
            this.websocket.Options.Credentials = this.deviceConnection.Credentials;
            this.websocket.Options.SetRequestHeader("Origin", this.deviceConnection.Connection.AbsoluteUri);

            // There is no way to set a ServerCertificateValidationCallback for a single web socket, hence the workaround.
            ServicePointManager.ServerCertificateValidationCallback = delegate(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors policyErrors)
            {
                return this.serverCertificateValidationHandler(sender, cert, chain, policyErrors);
            };

            await this.websocket.ConnectAsync(endpoint, CancellationToken.None);
        }

        /// <summary>
        /// Stops listening for messages and closes the connection to the websocket.
        /// </summary>
        /// <returns>The task of closing the websocket connection.</returns>
        private async Task StopListeningForMessagesInternal()
        {
            if (this.IsListeningForMessages)
            {
                await this.websocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);

                // Wait for web socket to no longer be receiving messages.
                if (this.IsListeningForMessages)
                {
                    this.stoppedReceivingMessages.WaitOne();
                    this.stoppedReceivingMessages.Reset();
                }

                // Reset websocket as WDP will abort the connection once it receives the close message.
                this.websocket = new ClientWebSocket();
            }
        }

        /// <summary>
        /// Listen for messages from the websocket. Once they are received they are parsed and the WebSocketMessageReceived event is raised.
        /// </summary>
        /// <returns>The task of listening for messages from the websocket.</returns>
        private async Task ListenForMessagesInternal()
        {
            try
            {
                ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[MaxChunkSizeInBytes]);

                // Once close message is sent do not try to get any more messages as WDP will abort the web socket connection.
                while (this.websocket.State == WebSocketState.Open)
                {
                    // Receive single message in chunks.
                    using (var ms = new MemoryStream())
                    {
                        WebSocketReceiveResult result;

                        do
                        {
                            result = await this.websocket.ReceiveAsync(buffer, CancellationToken.None);

                            if (result.MessageType == WebSocketMessageType.Close)
                            {
                                await this.websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                                return;
                            }

                            if (result.Count > MaxChunkSizeInBytes)
                            {
                                throw new InvalidOperationException("Buffer not large enough");
                            }

                            ms.Write(buffer.Array, buffer.Offset, result.Count);
                        }
                        while (!result.EndOfMessage);

                        ms.Seek(0, SeekOrigin.Begin);

                        if (result.MessageType == WebSocketMessageType.Text)
                        {
                            Stream stream = new MemoryStream();

                            await ms.CopyToAsync(stream);

                            // Ensure we return with the stream pointed at the origin.
                            stream.Position = 0;

                            this.ConvertStreamToMessage(stream);
                        }
                    }
                }
            }
            catch (WebSocketException e)
            {
                // If WDP aborted the web socket connection ignore the exception.
                SocketException socketException = e.InnerException?.InnerException as SocketException;
                if (socketException != null)
                {
                    if (socketException.NativeErrorCode == WSAECONNRESET)
                    {
                        return;
                    }
                }

                throw;
            }
            finally
            {
                this.stoppedReceivingMessages.Set();
                this.IsListeningForMessages = false;
            }
        }

        /// <summary>
        /// Starts listening for messages from the websocket. Once they are received they are parsed and the WebSocketMessageReceived event is raised.
        /// </summary>
        /// <param name="endpoint">The uri that the weboscket should connect to</param>
        /// <returns>The task of listening for messages from the websocket.</returns>
        private async Task StartListeningForMessagesInternal(Uri endpoint)
        {
            if (this.IsListeningForMessages)
            {
                return;
            }

            this.IsListeningForMessages = true;

            try
            {
                await this.OpenConnection(endpoint);
            }
            catch
            {
                this.IsListeningForMessages = false;
            }

            // Do not wait on actually listening for messages.
            Task listenForMessagesInternal = this.ListenForMessagesInternal();
        }
    }
}
