//----------------------------------------------------------------------------------------------
// <copyright file="WebSocket.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <summary>
    /// MOCK implementation of HTTP web socket wrapper
    /// </summary>
    /// <typeparam name="T">Return type for the websocket messages.</typeparam>
    internal partial class WebSocket<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocket{T}" /> class.
        /// </summary>
        /// <param name="connection">Implementation of a connection object.</param>
        /// <param name="serverCertificateValidationHandler">Server certificate handler.</param>
        public WebSocket(IDevicePortalConnection connection, Func<object, X509Certificate, X509Chain, SslPolicyErrors, bool> serverCertificateValidationHandler)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Stops listneing for messages from the websocket and closes the connection to the websocket.
        /// </summary>
        /// <returns>The task of closing the websocket connection.</returns>
        private async Task StopListeningForMessagesInternal()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Connects to the websocket and starts listening for messages from the websocket.
        /// Once they are received they are parsed and the WebSocketMessageReceived event is raised.
        /// </summary>
        /// <param name="endpoint">The uri that the weboscket should connect to</param>
        /// <returns>The task of listening for messages from the websocket.</returns>
        private async Task StartListeningForMessagesInternal(Uri endpoint)
        {
            throw new NotImplementedException();
        }
    }
}
