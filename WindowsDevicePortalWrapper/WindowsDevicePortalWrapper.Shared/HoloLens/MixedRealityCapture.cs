//----------------------------------------------------------------------------------------------
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
        public static readonly string MrcLiveStreamHighwResApi = "api/holographic/stream/live_high.mp4";

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
        /// <remarks>This method is only supported on HoloLens devices.</remarks>
        /// <returns>Task tracking completion of the REST call.</returns>
        public async Task DeleteMrcFile(string fileName)
        {
            if (!Utilities.IsHoloLens(this.Platform, this.DeviceFamily))
            {
                throw new NotSupportedException("This method is only supported on HoloLens.");
            }

            await this.Delete(
                MrcFileApi,
                string.Format("filename={0}", Utilities.Hex64Encode(fileName)));
        }

        /// <summary>
        /// Gets the capture file data
        /// </summary>
        /// <param name="fileName">Name of the file to retrieve</param>
        /// <param name="isThumbnailRequest">Whether or not we just want a thumbnail</param>
        /// <returns>Byte array containing the file data.</returns>
        /// <remarks>This method is only supported on HoloLens devices.</remarks>
        public async Task<byte[]> GetMrcFileData(
            string fileName,
            bool isThumbnailRequest = false)
        {
            if (!Utilities.IsHoloLens(this.Platform, this.DeviceFamily))
            {
                throw new NotSupportedException("This method is only supported on HoloLens.");
            }

            byte[] dataBytes = null;

            string apiPath = isThumbnailRequest ? MrcThumbnailApi : MrcFileApi;

            using (MemoryStream data = await this.Get<MemoryStream>(
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
        public async Task<MrcFileList> GetMrcFileList()
        {
            if (!Utilities.IsHoloLens(this.Platform, this.DeviceFamily))
            {
                throw new NotSupportedException("This method is only supported on HoloLens.");
            }

            MrcFileList mrcFileList = await this.Get<MrcFileList>(MrcFileListApi);

            foreach (MrcFileInformation mfi in mrcFileList.Files)
            {
                try 
                {
                    mfi.Thumbnail = await this.GetMrcThumbnailData(mfi.FileName);
                }
                catch
                {
                }
            }

            return mrcFileList;
        }

        /// <summary>
        /// Gets the status of the reality capture
        /// </summary>
        /// <returns>Status of the capture</returns>
        /// <remarks>This method is only supported on HoloLens devices.</remarks>
        public async Task<MrcStatus> GetMrcStatus()
        {
            if (!Utilities.IsHoloLens(this.Platform, this.DeviceFamily))
            {
                throw new NotSupportedException("This method is only supported on HoloLens.");
            }

            return await this.Get<MrcStatus>(MrcStatusApi);
        }

        /// <summary>
        /// Gets thumbnail data for the capture
        /// </summary>
        /// <param name="fileName">Name of the capture file</param>
        /// <returns>Byte array containing the thumbnail image data</returns>
        /// <remarks>This method is only supported on HoloLens devices.</remarks>
        public async Task<byte[]> GetMrcThumbnailData(string fileName)
        {
            // GetMrcFileData checks for the appropriate platform. We do not need to duplicate the check here.
            return await this.GetMrcFileData(fileName, true);
        }

        /// <summary>
        /// Starts a Mixed Reality Capture recording.
        /// </summary>
        /// <param name="includeHolograms">Whether to include holograms</param>
        /// <param name="includeColorCamera">Whether to include the color camera</param>
        /// <param name="includeMicrophone">Whether to include microphone data</param>
        /// <param name="includeAudio">Whether to include audio data</param>
        /// <remarks>This method is only supported on HoloLens devices.</remarks>
        /// <returns>Task tracking completion of the REST call.</returns>
        public async Task StartMrcRecording(
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

            await this.Post(
                MrcStartRecordingApi,
                payload);
        }

        /// <summary>
        /// Stops the Mixed Reality Capture recording
        /// </summary>
        /// <remarks>This method is only supported on HoloLens devices.</remarks>
        /// <returns>Task tracking completion of the REST call.</returns>
        public async Task StopMrcRecording()
        {
            if (!Utilities.IsHoloLens(this.Platform, this.DeviceFamily))
            {
                throw new NotSupportedException("This method is only supported on HoloLens.");
            }

            await this.Post(MrcStopRecordingApi);
        }

        /// <summary>
        /// Take a Mixed Reality Capture photo
        /// </summary>
        /// <param name="includeHolograms">Whether to include holograms</param>
        /// <param name="includeColorCamera">Whether to include the color camera</param>
        /// <remarks>This method is only supported on HoloLens devices.</remarks>
        /// <returns>Task tracking completion of the REST call.</returns>
        public async Task TakeMrcPhoto(
            bool includeHolograms = true,
            bool includeColorCamera = true)
        {
            if (!Utilities.IsHoloLens(this.Platform, this.DeviceFamily))
            {
                throw new NotSupportedException("This method is only supported on HoloLens.");
            }

            await this.Post(
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
