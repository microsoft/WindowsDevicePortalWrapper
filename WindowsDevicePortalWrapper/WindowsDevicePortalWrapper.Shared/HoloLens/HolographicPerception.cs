//----------------------------------------------------------------------------------------------
// <copyright file="HolographicPerception.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

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

        /// <summary>
        /// Enumeration defining the control modes used by the Holographic Perception Simulation.
        /// </summary>
        public enum SimulationControlMode
        {
            /// <summary>
            /// Default mode.
            /// </summary>
            Default = 0,

            /// <summary>
            /// Simulation mode.
            /// </summary>
            Simulation,

            /// <summary>
            /// Remote mode.
            /// </summary>
            Remote,

            /// <summary>
            /// Legacy mode.
            /// </summary>
            Legacy
        }

        /// <summary>
        /// Gets the perception simulation control mode.
        /// </summary>
        /// <returns>The simulation control mode.</returns>
        /// <remarks>This method is only supported on HoloLens devices.</remarks>
        public async Task<SimulationControlMode> GetPerceptionSimulationControlMode()
        {
            if (!Utilities.IsHoloLens(this.Platform, this.DeviceFamily))
            {
                throw new NotSupportedException("This method is only supported on HoloLens.");
            }

            PerceptionSimulationControlMode controlMode = await this.Get<PerceptionSimulationControlMode>(HolographicSimulationModeApi);
            return controlMode.Mode;
        }

        /// <summary>
        /// Sets the perception simulation control mode.
        /// </summary>
        /// <param name="mode">The simulation control mode.</param>
        /// <remarks>This method is only supported on HoloLens devices.</remarks>
        /// <returns>Task tracking completion of the REST call.</returns>
        public async Task SetPerceptionSimulationControlMode(SimulationControlMode mode)
        {
            if (!Utilities.IsHoloLens(this.Platform, this.DeviceFamily))
            {
                throw new NotSupportedException("This method is only supported on HoloLens.");
            }

            string payload = string.Format(
                "mode={0}",
                (int)mode);
            await this.Post(HolographicSimulationModeApi, payload);
        }

        #region Data contract
        /// <summary>
        /// Object representation of Perception Simulation control mode.
        /// </summary>
        [DataContract]
        public class PerceptionSimulationControlMode
        {
            /// <summary>
            /// Gets the control mode.
            /// </summary>
            [DataMember(Name = "mode")]
            public SimulationControlMode Mode { get; private set; }
        }
        #endregion // Data contract
    }
}
