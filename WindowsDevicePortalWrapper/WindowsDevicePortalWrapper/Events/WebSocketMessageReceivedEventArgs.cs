//----------------------------------------------------------------------------------------------
// <copyright file="WebSocketMessageReceivedEventArgs.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System.IO;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <summary>
    /// Web socket message received event handler
    /// </summary>
    /// <param name="sender">Sender <see cref="DevicePortal"/> object</param>
    /// <param name="args">Web socket message received args</param>
    /// <typeparam name="T">Return type for the websocket messages.</typeparam>
    public delegate void WebSocketMessageReceivedEventHandler<T>(DevicePortal sender, WebSocketMessageReceivedEventArgs<T> args);

    /// <summary>
    /// Web socket message received event args
    /// </summary>
    /// <typeparam name="T">Return type for the websocket messages.</typeparam>
    public class WebSocketMessageReceivedEventArgs<T> : System.EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketMessageReceivedEventArgs{T}"/> class.
        /// </summary>
        /// <param name="message">The message from the web socket.</param>
        internal WebSocketMessageReceivedEventArgs(
            T message)
        {
            this.Message = message;
        }

        /// <summary>
        /// Gets the web socket message
        /// </summary>
        public T Message { get; private set; }
    }
}
