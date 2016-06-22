//----------------------------------------------------------------------------------------------
// <copyright file="RemoteControl.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsDevicePortal
{
    using System.Threading.Tasks;

    /// <content>
    /// Wrappers for Remote Control methods
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// API for rebooting the device
        /// </summary>
        private static readonly string RebootApi = "api/control/restart";

        /// <summary>
        /// API for shutting down the device
        /// </summary>
        private static readonly string ShutdownApi = "api/control/shutdown";

        /// <summary>
        /// Reboots the device.
        /// </summary>
        /// <returns>
        /// Task tracking reboot completion
        /// </returns>
        public async Task Reboot()
        {
            await Post(RebootApi);
        }

        /// <summary>
        /// Shuts down the device.
        /// </summary>
        /// <returns>
        /// Task tracking shutdown completion
        /// </returns>
        public async Task Shutdown()
        {
            await Post(ShutdownApi);
        }
    }
}
