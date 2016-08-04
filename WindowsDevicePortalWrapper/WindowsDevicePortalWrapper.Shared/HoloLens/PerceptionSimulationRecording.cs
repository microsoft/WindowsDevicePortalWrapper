//----------------------------------------------------------------------------------------------
// <copyright file="PerceptionSimulationRecording.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <content>
    /// Wrappers for Perception Simulation Recording methods
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// API for getting a Holographic Perception Simulation recording status.
        /// </summary>
        public static readonly string HolographicSimulationRecordingStatusApi = "api/holographic/simulation/recording/status";

        /// <summary>
        /// API for starting a Holographic Perception Simulation recording.
        /// </summary>
        public static readonly string StartHolographicSimulationRecordingApi = "api/holographic/simulation/recording/start";

        /// <summary>
        /// API for stopping a Holographic Perception Simulation recording.
        /// </summary>
        public static readonly string StopHolographicSimulationRecordingApi = "api/holographic/simulation/recording/stop";
    }
}
