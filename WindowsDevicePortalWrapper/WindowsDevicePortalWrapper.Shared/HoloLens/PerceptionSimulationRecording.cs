//----------------------------------------------------------------------------------------------
// <copyright file="PerceptionSimulationRecording.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
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

        /// <summary>
        /// Gets the holographic simulation recording status.
        /// </summary>
        /// <returns>True if recording, false otherwise.</returns>
        /// <remarks>This method is only supported on HoloLens.</remarks>
        public async Task<bool> GetHolographicSimulationRecordingStatusAsync()
        {
            if (!Utilities.IsHoloLens(this.Platform, this.DeviceFamily))
            {
                throw new NotSupportedException("This method is only supported on HoloLens.");
            }

            HolographicSimulationRecordingStatus status = await this.GetAsync<HolographicSimulationRecordingStatus>(HolographicSimulationRecordingStatusApi);
            return status.IsRecording;
        }

        /// <summary>
        /// Starts a Holographic Simulation recording session.
        /// </summary>
        /// <param name="name">The name of the recording.</param>
        /// <param name="recordHead">Should head data be recorded? The default value is true.</param>
        /// <param name="recordHands">Should hand data be recorded? The default value is true.</param>
        /// <param name="recordSpatialMapping">Should Spatial Mapping data be recorded? The default value is true.</param>
        /// <param name="recordEnvironment">Should environment data be recorded? The default value is true.</param>
        /// <returns>Task tracking completion of the REST call.</returns>
        /// <remarks>This method is only supported on HoloLens.</remarks>
        public async Task StartHolographicSimulationRecordingAsync(
            string name,
            bool recordHead = true,
            bool recordHands = true,
            bool recordSpatialMapping = true,
            bool recordEnvironment = true)
        {
            if (!Utilities.IsHoloLens(this.Platform, this.DeviceFamily))
            {
                throw new NotSupportedException("This method is only supported on HoloLens.");
            }

            string payload = string.Format(
                "head={0}&hands={1}&spatialMapping={2}&environment={3}&name={4}",
                recordHead ? 1 : 0,
                recordHands ? 1 : 0,
                recordSpatialMapping ? 1 : 0,
                recordEnvironment ? 1 : 0,
                name);
            await this.PostAsync(StartHolographicSimulationRecordingApi, payload);
        }

        /// <summary>
        /// Stops a Holographic Simulation recording session.
        /// </summary>
        /// <returns>Byte array containing the recorded data.</returns>
        /// <exception cref="InvalidOperationException">No recording was in progress.</exception>
        /// <remarks>This method is only supported on HoloLens.</remarks>
        public async Task<byte[]> StopHolographicSimulationRecordingAsync()
        {
            if (!Utilities.IsHoloLens(this.Platform, this.DeviceFamily))
            {
                throw new NotSupportedException("This method is only supported on HoloLens.");
            }

            Uri uri = Utilities.BuildEndpoint(
                this.deviceConnection.Connection,
                StopHolographicSimulationRecordingApi);

            byte[] dataBytes = null;

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
                            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(HolographicSimulationError));
                            HolographicSimulationError error = null;

                            try
                            {
                                // Try to get / interpret an error response.
                                error = (HolographicSimulationError)serializer.ReadObject(outStream);
                            }
                            catch
                            {
                            }

                            if (error != null)
                            {
                                // We received an error response.
                                throw new InvalidOperationException(error.Reason);
                            }

                            // Getting here indicates that we have file data to return.
                            dataBytes = new byte[outStream.Length];
                            await outStream.ReadAsync(dataBytes, 0, dataBytes.Length);
                        }
                    }
                }
            }

            return dataBytes;
        }

        #region Data contract
        /// <summary>
        /// Object representation of a Holographic Simulation (playback or recording) error.
        /// </summary>
        [DataContract]
        public class HolographicSimulationError
        {
            /// <summary>
            /// Gets the Reason string.
            /// </summary>
            [DataMember(Name = "Reason")]
            public string Reason { get; private set; }
        }

        /// <summary>
        /// Object representation of Holographic Simulation recording status.
        /// </summary>
        [DataContract]
        public class HolographicSimulationRecordingStatus
        {
            /// <summary>
            /// Gets a value indicating whether the simulation is recording.
            /// </summary>
            [DataMember(Name = "recording")]
            public bool IsRecording { get; private set; }
        }
        #endregion // Data contract
    }
}
