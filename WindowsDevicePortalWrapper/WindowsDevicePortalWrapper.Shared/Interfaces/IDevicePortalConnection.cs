﻿//----------------------------------------------------------------------------------------------
// <copyright file="IDevicePortalConnection.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Net;
#if !WINDOWS_UWP
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
#endif
using static Microsoft.Tools.WindowsDevicePortal.DevicePortal;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <summary>
    /// Interface for creating a connection with the device portal.
    /// </summary>
    public interface IDevicePortalConnection
    {
        /// <summary>
        /// Gets the base uri (ex: http://localhost) used to communicate with the device.
        /// </summary>
        Uri Connection { get; }

        /// <summary>
        /// Gets the base uri (ex: ws://localhost) used to communicate with web sockets on the device.
        /// </summary>
        Uri WebSocketConnection { get; }

        /// <summary>
        /// Gets the credentials used when communicating with the device.
        /// </summary>
        NetworkCredential Credentials { get; }

        /// <summary>
        /// Gets or sets the family of the device (ex: Windows.Holographic).
        /// </summary>
        string Family { get; set; }

        /// <summary>
        /// Gets or sets information describing the operating system installed on the device.
        /// </summary>
        OperatingSystemInformation OsInfo { get; set; }

#if !WINDOWS_UWP
        /// <summary>
        /// Gets the provided device certificate.
        /// </summary>
        /// <returns>Stored device certificate.</returns>
        X509Certificate2 GetDeviceCertificate();

        /// <summary>
        /// Stores a manually provided device certificate.
        /// </summary>
        /// <param name="certificate">The device's root certificate.</param>
        void SetDeviceCertificate(X509Certificate2 certificate);
#endif

        /// <summary>
        /// Updates the http security requirements for device communication.
        /// </summary>
        /// <param name="requiresHttps">True if an https connection is required, false otherwise.</param>
        void UpdateConnection(bool requiresHttps);

        /// <summary>
        /// Updates the connection details (IP address) and http security requirements used when communicating with the device.
        /// </summary>
        /// <param name="ipConfig">Object that describes the current network configuration.</param>
        /// <param name="requiresHttps">True if an https connection is required, false otherwise.</param>
        void UpdateConnection(
            IpConfiguration ipConfig,
            bool requiresHttps);
    }
}
