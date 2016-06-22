//----------------------------------------------------------------------------------------------
// <copyright file="IDevicePortalConnection.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsDevicePortal
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Net;

    /// <summary>
    /// Interface for creating a connection with the device portal.
    /// </summary>
    public interface IDevicePortalConnection
    {
        /// <summary>
        /// Gets the base uri (ex: http://localhost) used to communicate with the device.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "localhost is a valid word.")]
        Uri Connection { get; }

        /// <summary>
        /// Gets the credentials used when communicating with the device.
        /// </summary>
        NetworkCredential Credentials { get; }

        /// <summary>
        /// Gets the friendly name of the device (ex: LivingRoomPC).
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets or sets information describing the operating system installed on the device.
        /// </summary>
        DevicePortal.OperatingSystemInformation OsInfo { get; set; }

        /// <summary>
        /// Get the raw data of the device's root certificate.
        /// </summary>
        /// <returns>Byte array containing the certificate data.</returns>
        byte[] GetDeviceCertificateData();

        /// <summary>
        /// Provides the raw data of the device's root certificate.
        /// </summary>
        /// <param name="certificateData">Byte array containing the certificate data.</param>
        /// <remarks>How this data is used and/or stored is implementation specific.</remarks>
        void SetDeviceCertificate(byte[] certificateData);

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
            DevicePortal.IpConfiguration ipConfig,
            bool requiresHttps);
    }
}
