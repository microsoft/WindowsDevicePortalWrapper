using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal
{
    public interface IDevicePortalConnection
    {
        /// <summary>
        /// The base uri (ex: http://localhost) used to communicate with the device.
        /// </summary>
        Uri Connection { get; }

        /// <summary>
        /// The credentials used when communicating with the device.
        /// </summary>
        NetworkCredential Credentials { get; }

        /// <summary>
        /// The friendly name of the device (ex: LivingRoomPC).
        /// </summary>
        String Name { get; }

        /// <summary>
        /// Information describing the operating system installed on the device.
        /// </summary>
        OperatingSystemInformation OsInfo { get; set; }

        /// <summary>
        /// Get the raw data of the device's root certificate.
        /// </summary>
        /// <returns>Byte array containing the certificate data.</returns>
        Byte[] GetDeviceCertificateData();

        /// <summary>
        /// Provides the raw data of the device's root certificate.
        /// </summary>
        /// <param name="certificateData">Byte array containing the certificate data.</param>
        /// <remarks>How this data is used and/or stored is implementation specific.</remarks>
        void SetDeviceCertificate(Byte[] certificateData);

        /// <summary>
        /// Updates the http security requirements for device communication.
        /// </summary>
        /// <param name="requiresHttps">True if an https connection is required, false otherwise.</param>
        void UpdateConnection(Boolean requiresHttps);

        /// <summary>
        /// Updates the connection details (IP address) and http security requirements used when communicating with the device.
        /// </summary>
        /// <param name="ipConfig">IpConfiguration object that describes the current network configuration.</param>
        /// <param name="requiresHttps">True if an https connection is required, false otherwise.</param>
        void UpdateConnection(IpConfiguration ipConfig,
                            Boolean requiresHttps);
    }
}
