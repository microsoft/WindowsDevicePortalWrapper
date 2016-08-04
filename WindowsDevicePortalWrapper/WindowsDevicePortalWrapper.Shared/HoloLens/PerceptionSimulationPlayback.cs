//----------------------------------------------------------------------------------------------
// <copyright file="PerceptionSimulationPlayback.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <content>
    /// Wrappers for Perception Simulation Playback methods
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// API for loading or unloading a Holographic Perception Simulation recording.
        /// </summary>
        public static readonly string HolographicSimulationLoadUnloadRecordingApi = "api/holographic/simulation/playback/session/file";

        /// <summary>
        /// API for pausing a Holographic Perception Simulation recording.
        /// </summary>
        public static readonly string HolographicSimulationPauseApi = "api/holographic/simulation/playback/session/pause";

        /// <summary>
        /// API for uploading or deleting a Holographic Perception Simulation recording file.
        /// </summary>
        public static readonly string HolographicSimulationPlaybackFileApi = "api/holographic/simulation/playback/file";

        /// <summary>
        /// API for retrieving a list of a Holographic Perception Simulation recording files.
        /// </summary>
        public static readonly string HolographicSimulationPlaybackFilesApi = "api/holographic/simulation/playback/files";

        /// <summary>
        /// API for starting playback of a Holographic Perception Simulation recording.
        /// </summary>
        public static readonly string HolographicSimulationPlayApi = "api/holographic/simulation/playback/session/play";

        /// <summary>
        /// API for loading or unloading a Holographic Perception Simulation recording.
        /// </summary>
        public static readonly string HolographicSimulationPlaybackRecordingsApi = "api/holographic/simulation/playback/session/files";

        /// <summary>
        /// API for retrieving the playback state of a Holographic Perception Simulation recording.
        /// </summary>
        public static readonly string HolographicSimulationPlaybackStateApi = "api/holographic/simulation/playback/session";

        /// <summary>
        /// API for starting playback of a Holographic Perception Simulation recording.
        /// </summary>
        public static readonly string HolographicSimulationStopApi = "api/holographic/simulation/playback/session/stop";

        /// <summary>
        /// API for retrieving the types of data in a Holographic Perception Simulation recording.
        /// </summary>
        public static readonly string HolographicSimulationTypesApi = "api/holographic/simulation/playback/session/types";
    }
}
