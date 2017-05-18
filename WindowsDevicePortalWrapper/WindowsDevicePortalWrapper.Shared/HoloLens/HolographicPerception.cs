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
        /// API for running a Perception client.
        /// </summary>
        public static readonly string HolographicPerceptionClient = "api/holographic/perception/client";

        /// <summary>
        /// API for getting or setting the Holographic Perception Simulation control mode.
        /// </summary>
        public static readonly string HolographicSimulationModeApi = "api/holographic/simulation/control/mode";

        /// <summary>
        /// API for controlling the Holographic Perception Simulation control stream.
        /// </summary>
        public static readonly string HolographicSimulationStreamApi = "api/holographic/simulation/control/stream";

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
            Simulation
        }

        /// <summary>
        /// Enumeration defining the priority levels for the Holographic Perception Simulation control stream.
        /// </summary>
        public enum SimulationControlStreamPriority
        {
            /// <summary>
            /// Low priority.
            /// </summary>
            Low = 0,

            /// <summary>
            /// Normal priority.
            /// </summary>
            Normal
        }

        /// <summary>
        /// Creates a simulation control stream.
        /// </summary>
        /// <param name="priority">The control stream priority.</param>
        /// <returns>The identifier of the created stream.</returns>
        /// <remarks>This method is only supported on HoloLens.</remarks>
        public async Task<string> CreatePerceptionSimulationControlStreamAsync(SimulationControlStreamPriority priority)
        {
            if (!Utilities.IsHoloLens(this.Platform, this.DeviceFamily))
            {
                throw new NotSupportedException("This method is only supported on HoloLens.");
            }

            if (!(await this.VerifySimulationControlModeAsync(SimulationControlMode.Simulation)))
            {
                throw new InvalidOperationException("The simulation control mode on the target HoloLens must be 'Simulation'.");
            }

            string payload = string.Format(
                "priority={0}",
                (int)priority);

            PerceptionSimulationControlStreamId controlStreamId = await this.GetAsync<PerceptionSimulationControlStreamId>(
                            HolographicSimulationStreamApi,
                            payload);

            return controlStreamId.StreamId;
        }

        /// <summary>
        /// Deletes a simulation control stream.
        /// </summary>
        /// <param name="streamId">The identifier of the stream to be deleted.</param>
        /// <returns>Task tracking completion of the REST call.</returns>
        /// <remarks>This method is only supported on HoloLens.</remarks>
        public async Task DeletePerceptionSimulationControlStreamAsync(string streamId)
        {
            if (!Utilities.IsHoloLens(this.Platform, this.DeviceFamily))
            {
                throw new NotSupportedException("This method is only supported on HoloLens.");
            }

            if (!(await this.VerifySimulationControlModeAsync(SimulationControlMode.Simulation)))
            {
                throw new InvalidOperationException("The simulation control mode on the target HoloLens must be 'Simulation'.");
            }

            string payload = string.Format(
                "streamId={0}",
                streamId);

            await this.DeleteAsync(
                HolographicSimulationStreamApi,
                payload);
        }

        /// <summary>
        /// Gets the perception simulation control mode.
        /// </summary>
        /// <returns>The simulation control mode.</returns>
        /// <remarks>This method is only supported on HoloLens.</remarks>
        public async Task<SimulationControlMode> GetPerceptionSimulationControlModeAsync()
        {
            if (!Utilities.IsHoloLens(this.Platform, this.DeviceFamily))
            {
                throw new NotSupportedException("This method is only supported on HoloLens.");
            }

            PerceptionSimulationControlMode controlMode = await this.GetAsync<PerceptionSimulationControlMode>(HolographicSimulationModeApi);
            return controlMode.Mode;
        }

        /// <summary>
        /// Sets the perception simulation control mode.
        /// </summary>
        /// <param name="mode">The simulation control mode.</param>
        /// <returns>Task tracking completion of the REST call.</returns>
        /// <remarks>This method is only supported on HoloLens.</remarks>
        public async Task SetPerceptionSimulationControlModeAsync(SimulationControlMode mode)
        {
            if (!Utilities.IsHoloLens(this.Platform, this.DeviceFamily))
            {
                throw new NotSupportedException("This method is only supported on HoloLens.");
            }

            string payload = string.Format(
                "mode={0}",
                (int)mode);
            await this.PostAsync(HolographicSimulationModeApi, payload);
        }

        /// <summary>
        /// Compares the current simulation control mode with the expected mode.
        /// </summary>
        /// <param name="expectedMode">The simulation control mode that we expect the device to be in.</param>
        /// <returns>The simulation control mode.</returns>
        private async Task<bool> VerifySimulationControlModeAsync(SimulationControlMode expectedMode)
        {
            SimulationControlMode simMode = await this.GetPerceptionSimulationControlModeAsync();
            return simMode == expectedMode;
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

        /// <summary>
        /// Object representation of the response recevied when creating a Perception Simulation control stream.
        /// </summary>
        [DataContract]
        public class PerceptionSimulationControlStreamId
        {
            /// <summary>
            /// Gets the stream identifier.
            /// </summary>
            [DataMember(Name = "streamId")]
            public string StreamId { get; private set; }
        }
        #endregion // Data contract
    }
}
