//----------------------------------------------------------------------------------------------
// <copyright file="GenericDevicePortal.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <summary>
    /// This is the main GenericDevicePortal object. It provides a concrete class
    /// for callers who don't care about their device type.
    /// </summary>
    public partial class GenericDevicePortal : DevicePortal
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenericDevicePortal" /> class.
        /// </summary>
        /// <param name="connection">Implementation of a connection object.</param>
        public GenericDevicePortal(IDevicePortalConnection connection) : base(connection)
        {
        }
    }
}
