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
        ///  API for getting or deleting a Mixed Reality Capture file
        /// </summary>
        public static readonly string MrcFileApi = "api/holographic/mrc/file";

        /// <summary>
        /// API for getting the list of Holographic Mixed Reality Capture files
        /// </summary>
        public static readonly string MrcFileListApi = "api/holographic/mrc/files";

        /// <summary>
        /// API for taking a Mixed Reality Capture photo 
        /// </summary>
        public static readonly string MrcPhotoApi = "api/holographic/mrc/photo";

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
        /// <returns>Task tracking the deletion request</returns>
        public async Task DeleteMrcFile(string fileName)
        {
            if (this.Platform != DevicePortalPlatforms.HoloLens)
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
        /// <returns>The raw capture data</returns>
        public async Task<byte[]> GetMrcFileData(
            string fileName,
            bool isThumbnailRequest = false)
        {
            if (this.Platform != DevicePortalPlatforms.HoloLens)
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
        public async Task<MrcFileList> GetMrcFileList()
        {
            if (this.Platform != DevicePortalPlatforms.HoloLens)
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
        public async Task<MrcStatus> GetMrcStatus()
        {
            if (this.Platform != DevicePortalPlatforms.HoloLens)
            {
                throw new NotSupportedException("This method is only supported on HoloLens.");
            }

            return await this.Get<MrcStatus>(MrcStatusApi);
        }

        /// <summary>
        /// Gets thumbnail data for the capture
        /// </summary>
        /// <param name="fileName">Name of the capture file</param>
        /// <returns>Thumbnail data</returns>
        public async Task<byte[]> GetMrcThumbnailData(string fileName)
        {
            // GetMrcFileData checks for the appropriate platform. We do not need to duplicate the check here.
            return await this.GetMrcFileData(fileName, true);
        }

        /// <summary>
        /// Starts a reality capture recording
        /// </summary>
        /// <param name="includeHolograms">Whether to include holograms</param>
        /// <param name="includeColorCamera">Whether to include the color camera</param>
        /// <param name="includeMicrophone">Whether to include microphone data</param>
        /// <param name="includeAudio">Whether to include audio data</param>
        /// <returns>Task tracking the start recording request</returns>
        public async Task StartMrcRecording(
            bool includeHolograms = true,
            bool includeColorCamera = true,
            bool includeMicrophone = true,
            bool includeAudio = true)
        {
            if (this.Platform != DevicePortalPlatforms.HoloLens)
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
        /// Stops the capture recording
        /// </summary>
        /// <returns>Task tracking the stop request</returns>
        public async Task StopMrcRecording()
        {
            if (this.Platform != DevicePortalPlatforms.HoloLens)
            {
                throw new NotSupportedException("This method is only supported on HoloLens.");
            }

            await this.Post(MrcStopRecordingApi);
        }

        /// <summary>
        /// Take a capture photo
        /// </summary>
        /// <param name="includeHolograms">Whether to include holograms</param>
        /// <param name="includeColorCamera">Whether to include the color camera</param>
        /// <returns>Task tracking the photo request</returns>
        public async Task TakeMrcPhoto(
            bool includeHolograms = true,
            bool includeColorCamera = true)
        {
            if (this.Platform != DevicePortalPlatforms.HoloLens)
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
            /// Gets or sets the list of files
            /// </summary>
            [DataMember(Name = "MrcRecordings")]
            public List<MrcFileInformation> Files { get; set; }
        }

        /// <summary>
        /// Object representation of an individual capture file
        /// </summary>
        [DataContract]
        public class MrcFileInformation
        {
            /// <summary>
            /// Gets or sets the raw creation time
            /// </summary>
            [DataMember(Name = "CreationTime")]
            public long CreationTimeRaw { get; set; }

            /// <summary>
            /// Gets or sets the filename
            /// </summary>
            [DataMember(Name = "FileName")]
            public string FileName { get; set; }

            /// <summary>
            /// Gets or sets the file size
            /// </summary>
            [DataMember(Name = "FileSize")]
            public uint FileSize { get; set; }

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
            /// Gets or sets a value indicating whether the device is recording
            /// </summary>
            [DataMember(Name = "IsRecording")]
            public bool IsRecording { get; set; }

            /// <summary>
            /// Gets or sets the recording status
            /// </summary>
            [DataMember(Name = "ProcessStatus")]
            public ProcessStatus Status { get; set; }
        }

        /// <summary>
        /// Object representation of the recording process status
        /// </summary>
        [DataContract]
        public class ProcessStatus
        {
            /// <summary>
            /// Gets or sets the process status
            /// </summary>
            [DataMember(Name = "MrcProcess")]
            public string MrcProcess { get; set; }  // TODO this should be an enum
        }
        #endregion Data contract
    }
}
