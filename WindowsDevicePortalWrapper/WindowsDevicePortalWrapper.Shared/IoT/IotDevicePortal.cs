//----------------------------------------------------------------------------------------------
// <copyright file="IotDevicePortal.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <summary>
    /// This is the main IotDevicePortal object. It isolates Iot specific endpoints from
    /// other DevicePortal objects while still exposing all the core endpoints.
    /// </summary>
    public partial class IotDevicePortal : DevicePortal
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IotDevicePortal" /> class.
        /// </summary>
        /// <param name="connection">Implementation of a connection object.</param>
        public IotDevicePortal(IDevicePortalConnection connection) : base(connection)
        {
        }
    }
}
