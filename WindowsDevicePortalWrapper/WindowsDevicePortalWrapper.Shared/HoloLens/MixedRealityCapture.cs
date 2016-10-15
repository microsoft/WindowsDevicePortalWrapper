﻿//----------------------------------------------------------------------------------------------
// <copyright file="MixedRealityCapture.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <content>
    /// Wrappers for Mixed reality capture methods
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        ///  API for getting or deleting a Mixed Reality Capture file.
        /// </summary>
        public static readonly string MrcFileApi = "api/holographic/mrc/file";

        /// <summary>
        /// API for getting the list of Holographic Mixed Reality Capture files.
        /// </summary>
        public static readonly string MrcFileListApi = "api/holographic/mrc/files";

        /// <summary>
        /// API for taking a Mixed Reality Capture photo.
        /// </summary>
        public static readonly string MrcPhotoApi = "api/holographic/mrc/photo";

        /// <summary>
        /// API for getting or setting the default Mixed Reality Capture settings.
        /// </summary>
        public static readonly string MrcSettingsApi = "api/holographic/mrc/settings";

        /// <summary>
        /// API for starting a Holographic Mixed Reality Capture recording.
        /// </summary>
        public static readonly string MrcStartRecordingApi = "api/holographic/mrc/video/control/start";

        /// <summary>
        /// API for getting the Holographic Mixed Reality Capture status.
        /// </summary>
        public static readonly string MrcStatusApi = "api/holographic/mrc/status";

        /// <summary>
        /// API for stopping a Holographic Mixed Reality Capture recording.
        /// </summary>
        public static readonly string MrcStopRecordingApi = "api/holographic/mrc/video/control/stop";

        /// <summary>
        /// API for getting a live Holographic Mixed Reality Capture stream.
        /// </summary>
        public static readonly string MrcLiveStreamApi = "api/holographic/stream/live.mp4";

        /// <summary>
        /// API for getting a high resolution live Holographic Mixed Reality Capture stream.
        /// </summary>
        public static readonly string MrcLiveStreamHighResApi = "api/holographic/stream/live_high.mp4";

        /// <summary>
        /// API for getting a low resolution live Holographic Mixed Reality Capture stream.
        /// </summary>
        public static readonly string MrcLiveStreamLowResApi = "api/holographic/stream/live_low.mp4";

        /// <summary>
        /// API for getting a medium resolution live Holographic Mixed Reality Capture stream.
        /// </summary>
        public static readonly string MrcLiveStreamMediumResApi = "api/holographic/stream/live_med.mp4";

        /// <summary>
        /// API for getting a mixed reality capture thumbnail
        /// </summary>
        public static readonly string MrcThumbnailApi = "api/holographic/mrc/thumbnail";

        /// <summary>
        /// Removes a Mixed Reality Capture file from the device's local storage.
        /// </summary>
        /// <param name="fileName">The name of the file to be deleted.</param>
        /// <returns>Task tracking completion of the REST call.</returns>
        /// <remarks>This method is only supported on HoloLens devices.</remarks>
        public async Task DeleteMrcFileAsync(string fileName)
        {
            if (!Utilities.IsHoloLens(this.Platform, this.DeviceFamily))
            {
                throw new NotSupportedException("This method is only supported on HoloLens.");
            }

            await this.DeleteAsync(
                MrcFileApi,
                string.Format("filename={0}", Utilities.Hex64Encode(fileName)));
        }

        /// <summary>
        /// Retrieve the Uri for the high resolution Mixed Reality Capture live stream.
        /// </summary>
        /// <param name="includeHolograms">Specifies whether or not to include holograms</param>
        /// <param name="includeColorCamera">Specifies whether or not to include the color camera</param>
        /// <param name="includeMicrophone">Specifies whether or not to include microphone data</param>
        /// <param name="includeAudio">Specifies whether or not to include audio data</param>
        /// <returns>Uri used to retreive the Mixed Reality Capture stream.</returns>
        /// <remarks>This method is only supported on HoloLens devices.</remarks>
        public Uri GetHighResolutionMrcLiveStreamUri(
            bool includeHolograms = true,
            bool includeColorCamera = true,
            bool includeMicrophone = true,
            bool includeAudio = true)
        {
            string payload = string.Format(
                "holo={0}&pv={1}&mic={2}&loopback={3}",
                includeHolograms,
                includeColorCamera,
                includeMicrophone,
                includeAudio).ToLower();

            return Utilities.BuildEndpoint(
                this.deviceConnection.Connection,
                MrcLiveStreamHighResApi,
                payload);
        }

        /// <summary>
        /// Retrieve the Uri for the low resolution Mixed Reality Capture live stream.
        /// </summary>
        /// <param name="includeHolograms">Specifies whether or not to include holograms</param>
        /// <param name="includeColorCamera">Specifies whether or not to include the color camera</param>
        /// <param name="includeMicrophone">Specifies whether or not to include microphone data</param>
        /// <param name="includeAudio">Specifies whether or not to include audio data</param>
        /// <returns>Uri used to retreive the Mixed Reality Capture stream.</returns>
        /// <remarks>This method is only supported on HoloLens devices.</remarks>
        public Uri GetLowResolutionMrcLiveStreamUri(
            bool includeHolograms = true,
            bool includeColorCamera = true,
            bool includeMicrophone = true,
            bool includeAudio = true)
        {
            string payload = string.Format(
                "holo={0}&pv={1}&mic={2}&loopback={3}",
                includeHolograms,
                includeColorCamera,
                includeMicrophone,
                includeAudio).ToLower();

            return Utilities.BuildEndpoint(
                this.deviceConnection.Connection,
                MrcLiveStreamLowResApi,
                payload);
        }

        /// <summary>
        /// Retrieve the Uri for the medium resolution Mixed Reality Capture live stream.
        /// </summary>
        /// <param name="includeHolograms">Specifies whether or not to include holograms</param>
        /// <param name="includeColorCamera">Specifies whether or not to include the color camera</param>
        /// <param name="includeMicrophone">Specifies whether or not to include microphone data</param>
        /// <param name="includeAudio">Specifies whether or not to include audio data</param>
        /// <returns>Uri used to retreive the Mixed Reality Capture stream.</returns>
        /// <remarks>This method is only supported on HoloLens devices.</remarks>
        public Uri GetMediumResolutionMrcLiveStreamUri(
            bool includeHolograms = true,
            bool includeColorCamera = true,
            bool includeMicrophone = true,
            bool includeAudio = true)
        {
            string payload = string.Format(
                "holo={0}&pv={1}&mic={2}&loopback={3}",
                includeHolograms,
                includeColorCamera,
                includeMicrophone,
                includeAudio).ToLower();

            return Utilities.BuildEndpoint(
                this.deviceConnection.Connection,
                MrcLiveStreamMediumResApi,
                payload);
        }

        /// <summary>
        /// Gets the capture file data
        /// </summary>
        /// <param name="fileName">Name of the file to retrieve.</param>
        /// <param name="isThumbnailRequest">Specifies whether or not we are requesting a thumbnail image.</param>
        /// <returns>Byte array containing the file data.</returns>
        /// <remarks>This method is only supported on HoloLens devices.</remarks>
        public async Task<byte[]> GetMrcFileDataAsync(
            string fileName,
            bool isThumbnailRequest = false)
        {
            if (!Utilities.IsHoloLens(this.Platform, this.DeviceFamily))
            {
                throw new NotSupportedException("This method is only supported on HoloLens.");
            }

            byte[] dataBytes = null;

            string apiPath = isThumbnailRequest ? MrcThumbnailApi : MrcFileApi;

            using (MemoryStream data = await this.GetAsync<MemoryStream>(
                apiPath,
                string.Format("filename={0}", Utilities.Hex64Encode(fileName))))
            {
                dataBytes = new byte[data.Length];
                data.Read(dataBytes, 0, dataBytes.Length);
            }

            return dataBytes;   
        }

        /// <summary>
        /// Gets the list of capture files
        /// </summary>
        /// <returns>List of the capture files</returns>
        /// <remarks>This method is only supported on HoloLens devices.</remarks>
        public async Task<MrcFileList> GetMrcFileListAsync()
        {
            if (!Utilities.IsHoloLens(this.Platform, this.DeviceFamily))
            {
                throw new NotSupportedException("This method is only supported on HoloLens.");
            }

            MrcFileList mrcFileList = await this.GetAsync<MrcFileList>(MrcFileListApi);

            foreach (MrcFileInformation mfi in mrcFileList.Files)
            {
                try 
                {
                    mfi.Thumbnail = await this.GetMrcThumbnailDataAsync(mfi.FileName);
                }
                catch
                {
                }
            }

            return mrcFileList;
        }

        /// <summary>
        /// Retrieve the Uri for the Mixed Reality Capture live stream using the default resolution.
        /// </summary>
        /// <param name="includeHolograms">Specifies whether or not to include holograms</param>
        /// <param name="includeColorCamera">Specifies whether or not to include the color camera</param>
        /// <param name="includeMicrophone">Specifies whether or not to include microphone data</param>
        /// <param name="includeAudio">Specifies whether or not to include audio data</param>
        /// <returns>Uri used to retreive the Mixed Reality Capture stream.</returns>
        /// <remarks>This method is only supported on HoloLens devices.</remarks>
        public Uri GetMrcLiveStreamUri(
            bool includeHolograms = true,
            bool includeColorCamera = true,
            bool includeMicrophone = true,
            bool includeAudio = true)
        {
            string payload = string.Format(
                "holo={0}&pv={1}&mic={2}&loopback={3}",
                includeHolograms,
                includeColorCamera,
                includeMicrophone,
                includeAudio).ToLower();

            return Utilities.BuildEndpoint(
                this.deviceConnection.Connection,
                MrcLiveStreamApi,
                payload);
        }

        // TODO: GetMrcSettings()

        /// <summary>
        /// Gets the status of the reality capture
        /// </summary>
        /// <returns>Status of the capture</returns>
        /// <remarks>This method is only supported on HoloLens devices.</remarks>
        public async Task<MrcStatus> GetMrcStatusAsync()
        {
            if (!Utilities.IsHoloLens(this.Platform, this.DeviceFamily))
            {
                throw new NotSupportedException("This method is only supported on HoloLens.");
            }

            return await this.GetAsync<MrcStatus>(MrcStatusApi);
        }

        /// <summary>
        /// Gets thumbnail data for the capture
        /// </summary>
        /// <param name="fileName">Name of the capture file</param>
        /// <returns>Byte array containing the thumbnail image data</returns>
        /// <remarks>This method is only supported on HoloLens devices.</remarks>
        public async Task<byte[]> GetMrcThumbnailDataAsync(string fileName)
        {
            // GetMrcFileData checks for the appropriate platform. We do not need to duplicate the check here.
            return await this.GetMrcFileDataAsync(fileName, true);
        }

        // TODO: SetMrcSettings()

        /// <summary>
        /// Starts a Mixed Reality Capture recording.
        /// </summary>
        /// <param name="includeHolograms">Specifies whether or not to include holograms</param>
        /// <param name="includeColorCamera">Specifies whether or not to include the color camera</param>
        /// <param name="includeMicrophone">Specifies whether or not to include microphone data</param>
        /// <param name="includeAudio">Specifies whether or not to include audio data</param>
        /// <returns>Task tracking completion of the REST call.</returns>
        /// <remarks>This method is only supported on HoloLens devices.</remarks>
        public async Task StartMrcRecordingAsync(
            bool includeHolograms = true,
            bool includeColorCamera = true,
            bool includeMicrophone = true,
            bool includeAudio = true)
        {
            if (!Utilities.IsHoloLens(this.Platform, this.DeviceFamily))
            {
                throw new NotSupportedException("This method is only supported on HoloLens.");
            }

            string payload = string.Format(
                "holo={0}&pv={1}&mic={2}&loopback={3}",
                includeHolograms,
                includeColorCamera,
                includeMicrophone,
                includeAudio).ToLower();

            await this.PostAsync(
                MrcStartRecordingApi,
                payload);
        }

        /// <summary>
        /// Stops the Mixed Reality Capture recording
        /// </summary>
        /// <returns>Task tracking completion of the REST call.</returns>
        /// <remarks>This method is only supported on HoloLens devices.</remarks>
        public async Task StopMrcRecordingAsync()
        {
            if (!Utilities.IsHoloLens(this.Platform, this.DeviceFamily))
            {
                throw new NotSupportedException("This method is only supported on HoloLens.");
            }

            await this.PostAsync(MrcStopRecordingApi);
        }

        /// <summary>
        /// Take a Mixed Reality Capture photo
        /// </summary>
        /// <param name="includeHolograms">Specifies whether or not to include holograms</param>
        /// <param name="includeColorCamera">Specifies whether or not to include the color camera</param>
        /// <returns>Task tracking completion of the REST call.</returns>
        /// <remarks>This method is only supported on HoloLens devices.</remarks>
        public async Task TakeMrcPhotoAsync(
            bool includeHolograms = true,
            bool includeColorCamera = true)
        {
            if (!Utilities.IsHoloLens(this.Platform, this.DeviceFamily))
            {
                throw new NotSupportedException("This method is only supported on HoloLens.");
            }

            await this.PostAsync(
                MrcPhotoApi,
                string.Format("holo={0}&pv={1}", includeHolograms, includeColorCamera).ToLower());
        }

        #region Data contract
        /// <summary>
        /// Object representation of the capture file list
        /// </summary>
        [DataContract]
        public class MrcFileList
        {
            /// <summary>
            /// Gets the list of files
            /// </summary>
            [DataMember(Name = "MrcRecordings")]
            public List<MrcFileInformation> Files { get; private set; }
        }

        /// <summary>
        /// Object representation of an individual capture file
        /// </summary>
        [DataContract]
        public class MrcFileInformation
        {
            /// <summary>
            /// Gets the raw creation time
            /// </summary>
            [DataMember(Name = "CreationTime")]
            public long CreationTimeRaw { get; private set; }

            /// <summary>
            /// Gets the filename
            /// </summary>
            [DataMember(Name = "FileName")]
            public string FileName { get; private set; }

            /// <summary>
            /// Gets the file size
            /// </summary>
            [DataMember(Name = "FileSize")]
            public uint FileSize { get; private set; }

            /// <summary>
            /// Gets the thumbnail
            /// </summary>
            public byte[] Thumbnail { get; internal set; }

            /// <summary>
            /// Gets the creation time
            /// </summary>
            public DateTime Created
            {
                get { return new DateTime(this.CreationTimeRaw); }
            }
        }

        /// <summary>
        /// Object representation of the Capture status
        /// </summary>
        [DataContract]
        public class MrcStatus
        {
            /// <summary>
            /// Gets a value indicating whether the device is recording
            /// </summary>
            [DataMember(Name = "IsRecording")]
            public bool IsRecording { get; private set; }

            /// <summary>
            /// Gets the recording status
            /// </summary>
            [DataMember(Name = "ProcessStatus")]
            public MrcProcessStatus Status { get; private set; }
        }

        /// <summary>
        /// Object representation of the recording process status
        /// </summary>
        [DataContract]
        public class MrcProcessStatus
        {
            /// <summary>
            /// Gets the process status
            /// </summary>
            [DataMember(Name = "MrcProcess")]
            public string MrcProcess { get; private set; }  // TODO this should be an enum
        }
        #endregion Data contract
    }
}
