//----------------------------------------------------------------------------------------------
// <copyright file="ConnectionStatus.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1649:FileHeaderFileNameDocumentationMustMatchTypeName", Justification = "Filename matches the enum better than the EventArgs class.")]

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <summary>
    /// Handler for reporting on device connection status
    /// </summary>
    /// <param name="sender">sender object</param>
    /// <param name="args">connection status details</param>
    public delegate void DeviceConnectionStatusEventHandler(DevicePortal sender, DeviceConnectionStatusEventArgs args);

    /// <summary>
    /// Connection status enumeration
    /// </summary>
    public enum DeviceConnectionStatus
    {
        /// <summary>
        /// No status
        /// </summary>
        None,

        /// <summary>
        /// Currently Connecting
        /// </summary>
        Connecting,

        /// <summary>
        /// Connection complete
        /// </summary>
        Connected,

        /// <summary>
        /// Connection failed
        /// </summary>
        Failed,
    }

    /// <summary>
    /// Connection phase enumeration
    /// </summary>
    public enum DeviceConnectionPhase
    {
        /// <summary>
        /// Idle phase
        /// </summary>
        Idle,

        /// <summary>
        /// Acquiring the device certificate
        /// </summary>
        AcquiringCertificate,

        /// <summary>
        /// Determining the device connection requirements
        /// </summary>
        DeterminingConnectionRequirements,

        /// <summary>
        /// Getting some basic information about the device OS
        /// </summary>
        RequestingOperatingSystemInformation,

        /// <summary>
        /// Connecting the device to a provided network
        /// </summary>
        ConnectingToTargetNetwork,

        /// <summary>
        /// Updating the device address to reflect the new network
        /// </summary>
        UpdatingDeviceAddress
    }

    /// <summary>
    /// Contains the details about the connection status
    /// </summary>
    public class DeviceConnectionStatusEventArgs : System.EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceConnectionStatusEventArgs"/> class.
        /// </summary>
        /// <param name="status">Status of the connection</param>
        /// <param name="phase">Phase of the connection</param>
        /// <param name="message">Optional message describing our connection/phase</param>
        internal DeviceConnectionStatusEventArgs(
            DeviceConnectionStatus status,
            DeviceConnectionPhase phase,
            string message = "")
        {
            this.Status = status;
            this.Phase = phase;
            this.Message = message;
        }

        /// <summary>
        /// Gets the status of the connection attempt
        /// </summary>
        public DeviceConnectionStatus Status { get; private set; }
        
        /// <summary>
        /// Gets the phase of the connection
        /// </summary>
        public DeviceConnectionPhase Phase { get; private set; }

        /// <summary>
        /// Gets a message describing the connection status/phase
        /// </summary>
        public string Message { get; private set; }
    }
}
