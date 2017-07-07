//----------------------------------------------------------------------------------------------
// <copyright file="XboxDevicePortal.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <summary>
    /// This is the main XboxDevicePortal object. It isolates Xbox specific endpoints from
    /// other DevicePortal objects while still exposing all the core endpoints.
    /// </summary>
    public partial class XboxDevicePortal : DevicePortal
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XboxDevicePortal" /> class.
        /// </summary>
        /// <param name="connection">Implementation of a connection object.</param>
        public XboxDevicePortal(IDevicePortalConnection connection) : base(connection)
        {
        }
    }
}
