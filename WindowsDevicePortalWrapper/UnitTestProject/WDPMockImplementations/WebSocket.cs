//----------------------------------------------------------------------------------------------
// <copyright file="WebSocket.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Tools.WindowsDevicePortal.Tests;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <summary>
    /// MOCK implementation of HTTP web socket wrapper
    /// </summary>
    /// <typeparam name="T">Return type for the websocket messages.</typeparam>
    internal partial class WebSocket<T>
    {
        /// <summary>
        /// Indicates whether the websocket should continue listening for messages.
        /// </summary>
        private bool keepListeningForMessages = false;

        /// <summary>
        /// <see cref="ManualResetEvent" /> used to indicate that the <see cref="WebSocket{T}" /> has stopped receiving messages.
        /// </summary>
        private ManualResetEvent stoppedReceivingMessages = new ManualResetEvent(false);

        /// <summary>
        /// The handler used to validate server certificates.
        /// </summary>
        private Func<object, X509Certificate, X509Chain, SslPolicyErrors, bool> serverCertificateValidationHandler;

        /// <summary>
        /// The connection to the websocket.
        /// </summary>
        private Task<HttpResponseMessage> webSocketTask;

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
        /// Initialize a connection to the websocket.
        /// </summary>
        /// <param name="endpoint">The uri that the weboscket should connect to.</param>
        /// <returns>The task of opening a connection to the websocket.</returns>
        private async Task ConnectInternalAsync(Uri endpoint)
        {
            await Task.Run(() =>
            {
                this.webSocketTask = TestHelpers.MockHttpResponder.WebSocketAsync(endpoint);
                this.IsConnected = true;
            });
        }

        /// <summary>
        /// Closes the connection to the websocket.
        /// </summary>
        /// <returns>The task of closing the websocket connection.</returns>
        private async Task CloseInternalAsync()
        {
            await Task.Run(() =>
            {
                this.webSocketTask.Dispose();
                this.webSocketTask = null;
                this.IsConnected = false;
            });
        }

        /// <summary>
        /// Stops listneing for messages from the websocket and closes the connection to the websocket.
        /// </summary>
        /// <returns>The task of closing the websocket connection.</returns>
#pragma warning disable 1998
        private async Task StopListeningForMessagesInternalAsync()
        {
            this.keepListeningForMessages = false;

            // Wait for web socket to no longer be receiving messages.
            if (this.IsListeningForMessages)
            {
                this.stoppedReceivingMessages.WaitOne();
                this.stoppedReceivingMessages.Reset();
            }
        }
#pragma warning restore 1998

        /// <summary>
        /// Starts listening for messages from the websocket.
        /// Once they are received they are parsed and the WebSocketMessageReceived event is raised.
        /// </summary>
        /// <returns>The task of listening for messages from the websocket.</returns>
        private async Task StartListeningForMessagesInternalAsync()
        {
            this.IsListeningForMessages = true;
            this.keepListeningForMessages = true;

            try
            {
                while (this.keepListeningForMessages)
                {
                    await this.webSocketTask.ConfigureAwait(false);

                    using (HttpResponseMessage response = this.webSocketTask.Result)
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            throw await DevicePortalException.CreateAsync(response);
                        }

                        using (HttpContent content = response.Content)
                        {
                            MemoryStream dataStream = new MemoryStream();

                            await content.CopyToAsync(dataStream).ConfigureAwait(false);

                            // Ensure we return with the stream pointed at the origin.
                            dataStream.Position = 0;

                            this.ConvertStreamToMessage(dataStream);
                        }
                    }
                }
            }
            catch
            {
            }
            finally
            {
                this.stoppedReceivingMessages.Set();
                this.IsListeningForMessages = false;
            }
        }

        /// <summary>
        /// Sends a message to the server.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <returns>The task of sending the message to the websocket</returns>
        private async Task SendMessageInternalAsync(string message)
        {
            await this.webSocketTask.ConfigureAwait(false);
            this.webSocketTask.Wait();

            using (HttpResponseMessage response = this.webSocketTask.Result)
            {
                if (!response.IsSuccessStatusCode)
                {
                    throw await DevicePortalException.CreateAsync(response);
                }
            }
        }
    }
}
