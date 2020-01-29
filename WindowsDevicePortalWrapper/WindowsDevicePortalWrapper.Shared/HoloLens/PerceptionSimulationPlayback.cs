//----------------------------------------------------------------------------------------------
// <copyright file="PerceptionSimulationPlayback.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;

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
        public static readonly string HolographicSimulationPlaybackSessionFileApi = "api/holographic/simulation/playback/session/file";

        /// <summary>
        /// API for pausing a Holographic Perception Simulation recording.
        /// </summary>
        public static readonly string HolographicSimulationPlaybackPauseApi = "api/holographic/simulation/playback/session/pause";

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
        public static readonly string HolographicSimulationPlaybackPlayApi = "api/holographic/simulation/playback/session/play";

        /// <summary>
        /// API for getting the list of loaded Holographic Perception Simulation files.
        /// </summary>
        public static readonly string HolographicSimulationPlaybackSessionFilesApi = "api/holographic/simulation/playback/session/files";

        /// <summary>
        /// API for retrieving the playback state of a Holographic Perception Simulation recording.
        /// </summary>
        public static readonly string HolographicSimulationPlaybackStateApi = "api/holographic/simulation/playback/session";

        /// <summary>
        /// API for starting playback of a Holographic Perception Simulation recording.
        /// </summary>
        public static readonly string HolographicSimulationPlaybackStopApi = "api/holographic/simulation/playback/session/stop";

        /// <summary>
        /// API for retrieving the types of data in a Holographic Perception Simulation recording.
        /// </summary>
        public static readonly string HolographicSimulationPlaybackDataTypesApi = "api/holographic/simulation/playback/session/types";

        /// <summary>
        /// Enumeration describing the available Holgraphic Simulation playback states.
        /// </summary>
        public enum HolographicSimulationPlaybackStates
        {
            /// <summary>
            /// The simulation has been stopped.
            /// </summary>
            Stopped = 0,

            /// <summary>
            /// The simulation is playing.
            /// </summary>
            Playing,

            /// <summary>
            /// The simulation has been paused.
            /// </summary>
            Paused,

            /// <summary>
            /// Playback has completed.
            /// </summary>
            Complete,

            /// <summary>
            /// Playback is in an unexpected / unknown state.
            /// </summary>
            Unexpected = 9999
        }

        /// <summary>
        /// Deletes the specified Holographic Simulation recording.
        /// </summary>
        /// <param name="name">The name of the recording to delete (ex: testsession.xef).</param>
        /// <returns>Task tracking completion of the REST call.</returns>
        /// <remarks>This method is only supported on HoloLens.</remarks>
        public async Task DeleteHolographicSimulationRecordingAsync(string name)
        {
            if (!Utilities.IsHoloLens(this.Platform, this.DeviceFamily))
            {
                throw new NotSupportedException("This method is only supported on HoloLens.");
            }

            string payload = string.Format(
                "recording={0}",
                name);

            await this.DeleteAsync(HolographicSimulationPlaybackFileApi, payload);
        }

        /// <summary>
        /// Gets the collection of Holographic Perception Simulation files on this HoloLens.
        /// </summary>
        /// <returns>HolographicSimulationPlaybackFiles object representing the files on the HoloLens</returns>
        /// <remarks>This method is only supported on HoloLens.</remarks>
        public async Task<HolographicSimulationPlaybackFiles> GetHolographicSimulationPlaybackFilesAsync()
        {
            if (!Utilities.IsHoloLens(this.Platform, this.DeviceFamily))
            {
                throw new NotSupportedException("This method is only supported on HoloLens.");
            }

            return await this.GetHolographicSimulationPlaybackFilesPrivateAsync(false);
        }

        /// <summary>
        /// Gets the collection of loaded Holographic Perception Simulation files on this HoloLens.
        /// </summary>
        /// <returns>HolographicSimulationPlaybackFiles object representing the files loaded on the HoloLens</returns>
        /// <remarks>This method is only supported on HoloLens.</remarks>
        public async Task<HolographicSimulationPlaybackFiles> GetHolographicSimulationPlaybackSessionFilesAsync()
        {
            if (!Utilities.IsHoloLens(this.Platform, this.DeviceFamily))
            {
                throw new NotSupportedException("This method is only supported on HoloLens.");
            }

            return await this.GetHolographicSimulationPlaybackFilesPrivateAsync(true);
        }

        /// <summary>
        /// Gets the types of data that are in a loaded Holographic Perception Simulation file.
        /// </summary>
        /// <param name="recordingName">Name of the recording file, with extension.</param>
        /// <returns>HolographicSimulationDataTypes object representing they types of data in the file</returns>
        /// <remarks>This method is only supported on HoloLens.</remarks>
        public async Task<HolographicSimulationDataTypes> GetHolographicSimulationPlaybackSessionDataTypesAsync(string recordingName)
        {
            if (!Utilities.IsHoloLens(this.Platform, this.DeviceFamily))
            {
                throw new NotSupportedException("This method is only supported on HoloLens.");
            }

            string payload = string.Format(
                "recording={0}",
                recordingName);
                
            return await this.GetAsync<HolographicSimulationDataTypes>(
                HolographicSimulationPlaybackDataTypesApi,
                payload);
        }

        /// <summary>
        /// Gets the playback state of a Holographic Simulation recording.
        /// </summary>
        /// <param name="name">The name of the recording (ex: testsession.xef).</param>
        /// <returns>HolographicSimulationPlaybackStates enum value describing the state of the recording.</returns>
        /// <remarks>This method is only supported on HoloLens.</remarks>
        public async Task<HolographicSimulationPlaybackStates> GetHolographicSimulationPlaybackStateAsync(string name)
        {
            if (!Utilities.IsHoloLens(this.Platform, this.DeviceFamily))
            {
                throw new NotSupportedException("This method is only supported on HoloLens.");
            }

            HolographicSimulationPlaybackStates playbackState = HolographicSimulationPlaybackStates.Unexpected;

            string payload = string.Format(
                "recording={0}",
                name);

            Uri uri = Utilities.BuildEndpoint(
                this.deviceConnection.Connection,
                HolographicSimulationPlaybackStateApi,
                payload);

            using (Stream dataStream = await this.GetAsync(uri))
            {
                if (dataStream != null)
                {
                    using (MemoryStream outStream = new MemoryStream())
                    {
                        dataStream.CopyTo(outStream);
                        if (outStream.Length != 0)
                        {
                            outStream.Seek(0, SeekOrigin.Begin);
                            // Try to get the session state.
                            try
                            {
                                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(HolographicSimulationPlaybackSessionState));
                                HolographicSimulationPlaybackSessionState sessionState = (HolographicSimulationPlaybackSessionState)serializer.ReadObject(dataStream);
                                playbackState = sessionState.State;
                            }
                            catch
                            {
                                // We did not receive the session state, check to see if we received a simulation error.
                                dataStream.Position = 0;
                                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(HolographicSimulationError));
                                HolographicSimulationError error = (HolographicSimulationError)serializer.ReadObject(dataStream);
                                throw new InvalidOperationException(error.Reason);
                            }
                        }
                    }
                }
            }

            return playbackState;
        }

        /// <summary>
        /// Loads the specified Holographic Simulation recording.
        /// </summary>
        /// <param name="recordingName">The name of the recording to load (ex: testsession.xef).</param>
        /// <returns>Task tracking completion of the REST call.</returns>
        /// <remarks>This method is only supported on HoloLens.</remarks>
        public async Task LoadHolographicSimulationRecordingAsync(string recordingName)
        {
            if (!Utilities.IsHoloLens(this.Platform, this.DeviceFamily))
            {
                throw new NotSupportedException("This method is only supported on HoloLens.");
            }

            string payload = string.Format(
                "recording={0}",
                recordingName);

            await this.PostAsync(HolographicSimulationPlaybackSessionFileApi, payload);
        }

        /// <summary>
        /// Pauses playback of a Holographic Perception Simulation recording
        /// </summary>
        /// <param name="recordingName">The name of the recording to pause</param>
        /// <returns>Task tracking completion of the REST call.</returns>
        /// <remarks>This method is only supported on HoloLens.</remarks>
        public async Task PauseHolographicSimulationRecordingAsync(string recordingName)
        {
            if (!Utilities.IsHoloLens(this.Platform, this.DeviceFamily))
            {
                throw new NotSupportedException("This method is only supported on HoloLens.");
            }

            string payload = string.Format(
                "recording={0}",
                recordingName);

            await this.PostAsync(HolographicSimulationPlaybackPauseApi, payload);
        }

        /// <summary>
        /// Starts playback of a Holographic Perception Simulation recording
        /// </summary>
        /// <param name="recordingName">The name of the recording to play</param>
        /// <returns>Task tracking completion of the REST call.</returns>
        /// <remarks>This method is only supported on HoloLens.</remarks>
        public async Task PlayHolographicSimulationRecordingAsync(string recordingName)
        {
            if (!Utilities.IsHoloLens(this.Platform, this.DeviceFamily))
            {
                throw new NotSupportedException("This method is only supported on HoloLens.");
            }

            string payload = string.Format(
                "recording={0}",
                recordingName);

            await this.PostAsync(HolographicSimulationPlaybackPlayApi, payload);
        }

        /// <summary>
        /// Stops playback of a Holographic Perception Simulation recording
        /// </summary>
        /// <param name="recordingName">The name of the recording to stop</param>
        /// <returns>Task tracking completion of the REST call.</returns>
        /// <remarks>This method is only supported on HoloLens.</remarks>
        public async Task StopHolographicSimulationRecordingAsync(string recordingName)
        {
            if (!Utilities.IsHoloLens(this.Platform, this.DeviceFamily))
            {
                throw new NotSupportedException("This method is only supported on HoloLens.");
            }

            string payload = string.Format(
                "recording={0}",
                recordingName);

            await this.PostAsync(HolographicSimulationPlaybackStopApi, payload);
        }

        /// <summary>
        /// Unloads the specified Holographic Simulation recording.
        /// </summary>
        /// <param name="recordingName">The name of the recording to unload (ex: testsession.xef).</param>
        /// <returns>Task tracking completion of the REST call.</returns>
        /// <remarks>This method is only supported on HoloLens.</remarks>
        public async Task UnloadHolographicSimulationRecordingAsync(string recordingName)
        {
            if (!Utilities.IsHoloLens(this.Platform, this.DeviceFamily))
            {
                throw new NotSupportedException("This method is only supported on HoloLens.");
            }

            string payload = string.Format(
                "recording={0}",
                recordingName);

            await this.DeleteAsync(HolographicSimulationPlaybackSessionFileApi, payload);
        }

        /// <summary>
        /// Gets the collection of Holographic Perception Simulation files
        /// </summary>
        /// <param name="session">Value indicating whether or not to return loaded files.</param>
        /// <returns>Collection of Holographic Perception simulation file names</returns>
        private async Task<HolographicSimulationPlaybackFiles> GetHolographicSimulationPlaybackFilesPrivateAsync(bool session)
        {
            string apiPath = session ? HolographicSimulationPlaybackSessionFilesApi : HolographicSimulationPlaybackFilesApi;

            return await this.GetAsync<HolographicSimulationPlaybackFiles>(apiPath);
        }

        #region Data contract
        /// <summary>
        /// Object representing the data types in a Holographic Perception Simulation file
        /// </summary>
        [DataContract]
        public class HolographicSimulationDataTypes
        {
            /// <summary>
            /// Gets a value indicating whether or not the file contains hand data.
            /// </summary>
            [DataMember(Name = "hands")]
            public bool IncludesHands { get; private set; }

            /// <summary>
            /// Gets a value indicating whether or not the file contains head data.
            /// </summary>
            [DataMember(Name = "head")]
            public bool IncludesHead { get; private set; }

            /// <summary>
            /// Gets a value indicating whether or not the file contains environmentatl data.
            /// </summary>
            [DataMember(Name = "environment")]
            public bool IncludesEnvironment { get; private set; }

            /// <summary>
            /// Gets a value indicating whether or not the file contains spatial mapping data.
            /// </summary>
            [DataMember(Name = "spatialMapping")]
            public bool IncludesSpatialMapping { get; private set; }
        }

        /// <summary>
        /// Object representation of the Holographic Perception Simulation files collection
        /// </summary>
        [DataContract]
        public class HolographicSimulationPlaybackFiles
        {
            /// <summary>
            /// Gets the list of recording file names.
            /// </summary>
            [DataMember(Name = "recordings")]
            public List<string> Files { get; private set; }
        }

        /// <summary>
        /// Object representation of the Holographic Perception Simulation playback state
        /// </summary>
        [DataContract]
        public class HolographicSimulationPlaybackSessionState
        {   
            /// <summary>
            /// Gets the state value as a string
            /// </summary>
            [DataMember(Name = "state")]
            public string StateRaw { get; private set; }

            /// <summary>
            /// Gets the playback session state
            /// </summary>
            public HolographicSimulationPlaybackStates State
            {
                get 
                {
                    HolographicSimulationPlaybackStates state = HolographicSimulationPlaybackStates.Unexpected;

                    switch (this.StateRaw)
                    {
                        case "stopped":
                            state = HolographicSimulationPlaybackStates.Stopped;
                            break;

                        case "playing":
                            state = HolographicSimulationPlaybackStates.Playing;
                            break;

                        case "paused":
                            state = HolographicSimulationPlaybackStates.Paused;
                            break;

                        case "end":
                            state = HolographicSimulationPlaybackStates.Complete;
                            break;

                        default:
                            state = HolographicSimulationPlaybackStates.Unexpected;
                            break;
                    }

                    return state;
                }
            }
        }
        #endregion // Data contract
    }
}
