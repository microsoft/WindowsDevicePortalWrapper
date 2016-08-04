//----------------------------------------------------------------------------------------------
// <copyright file="HolographicPerception.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <content>
    /// Wrappers for Holographic Perception methods
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// API for getting or setting the Holographic Perception Simulation control mode.
        /// </summary>
        public static readonly string HolographicSimulationModeApi = "api/holographic/simulation/control/mode";

        /// <summary>
        /// API for controlling the Holographic Perception Simulation control stream.
        /// </summary>
        public static readonly string HolographicSimulationStreamApi = "api/holographic/simulation/control/Stream";
    }
}
