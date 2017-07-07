//----------------------------------------------------------------------------------------------
// <copyright file="HoloLensDevicePortal.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <summary>
    /// This is the main HoloLensDevicePortal object. It isolates HoloLens specific endpoints from
    /// other DevicePortal objects while still exposing all the core endpoints.
    /// </summary>
    public partial class HoloLensDevicePortal : DevicePortal
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HoloLensDevicePortal" /> class.
        /// </summary>
        /// <param name="connection">Implementation of a connection object.</param>
        public HoloLensDevicePortal(IDevicePortalConnection connection) : base(connection)
        {
        }
    }
}
