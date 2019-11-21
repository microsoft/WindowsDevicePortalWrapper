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
        /// <remarks>This method is only supported on HoloLens.</remarks>
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
        /// <remarks>This method is only supported on HoloLens.</remarks>
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
        /// <remarks>This method is only supported on HoloLens.</remarks>
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
        /// <remarks>This method is only supported on HoloLens.</remarks>
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
        /// <remarks>This method is only supported on HoloLens.</remarks>
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

            string payload = string.Format("filename={0}", Utilities.Hex64Encode(fileName));
            Uri uri = Utilities.BuildEndpoint(
                this.deviceConnection.Connection,
                apiPath,
                payload);

            using (Stream dataStream = await this.GetAsync(uri))
            {
                using (MemoryStream outStream = new MemoryStream())
                {
                    await dataStream.CopyToAsync(outStream);
                    dataBytes = new byte[outStream.Length];
                    await outStream.ReadAsync(dataBytes, 0, dataBytes.Length);
                }
            }

            return dataBytes;   
        }

        /// <summary>
        /// Gets the list of capture files
        /// </summary>
        /// <returns>List of the capture files</returns>
        /// <remarks>This method is only supported on HoloLens.</remarks>
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
        /// <remarks>This method is only supported on HoloLens.</remarks>
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

        /// <summary>
        /// Gets the current Mixed Reality Capture settings
        /// </summary>
        /// <returns>MrcSettings object containing the current settings</returns>
        /// <remarks>This method is only supported on HoloLens.</remarks>
        public async Task<MrcSettings> GetMrcSettingsAsync()
        {
            if (!Utilities.IsHoloLens(this.Platform, this.DeviceFamily))
            {
                throw new NotSupportedException("This method is only supported on HoloLens.");
            }

            return await this.GetAsync<MrcSettings>(MrcSettingsApi);
        }

        /// <summary>
        /// Gets the status of the reality capture
        /// </summary>
        /// <returns>Status of the capture</returns>
        /// <remarks>This method is only supported on HoloLens.</remarks>
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
        /// <remarks>This method is only supported on HoloLens.</remarks>
        public async Task<byte[]> GetMrcThumbnailDataAsync(string fileName)
        {
            // GetMrcFileData checks for the appropriate platform. We do not need to duplicate the check here.
            return await this.GetMrcFileDataAsync(fileName, true);
        }

        /// <summary>
        /// Sets the default Mixed Reality Capture settings
        /// </summary>
        /// <param name="settings">Mixed Reality Capture settings to be used as the default.</param>
        /// <returns>Task tracking completion of the REST call.</returns>
        /// <remarks>This method is only supported on HoloLens.</remarks>
        public async Task SetMrcSettingsAsync(MrcSettings settings)
        {
            if (!Utilities.IsHoloLens(this.Platform, this.DeviceFamily))
            {
                throw new NotSupportedException("This method is only supported on HoloLens.");
            }

            string payload = string.Format(
                "holo={0}&pv={1}&mic={2}&appAudio={3}&vstabbuffer={4}",
                settings.IncludeHolograms.ToString().ToLower(),
                settings.IncludeColorCamera.ToString().ToLower(),
                settings.IncludeMicrophone.ToString().ToLower(),
                settings.IncludeAudio.ToString().ToLower(),
                settings.VideoStabilizationBuffer);

            await this.PostAsync(
                MrcSettingsApi,
                payload);
        }

        /// <summary>
        /// Starts a Mixed Reality Capture recording.
        /// </summary>
        /// <param name="includeHolograms">Specifies whether or not to include holograms</param>
        /// <param name="includeColorCamera">Specifies whether or not to include the color camera</param>
        /// <param name="includeMicrophone">Specifies whether or not to include microphone data</param>
        /// <param name="includeAudio">Specifies whether or not to include audio data</param>
        /// <returns>Task tracking completion of the REST call.</returns>
        /// <remarks>This method is only supported on HoloLens.</remarks>
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
        /// <remarks>This method is only supported on HoloLens.</remarks>
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
        /// <remarks>This method is only supported on HoloLens.</remarks>
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
        /// Object representation of the Mixed Reality Capture process status
        /// </summary>
        [DataContract]
        public class MrcProcessStatus
        {
            /// <summary>
            /// Gets the raw data for the Mixed Reality Capture process status
            /// </summary>
            [DataMember(Name = "MrcProcess")]
            public string MrcProcessRaw { get; private set; }

            /// <summary>
            /// Gets the status of the Mixed Reality Capture process
            /// </summary>
            public ProcessStatus MrcProcess
            {
                get
                {
                    return (this.MrcProcessRaw == "Running") ? ProcessStatus.Running : ProcessStatus.Stopped;
                }
            }
        }

        /// <summary>
        /// Object representation of a Mixed Reality Capture setting.
        /// </summary>
        [DataContract]
        public class MrcSetting
        {
            /// <summary>
            /// Gets or sets the name of the setting
            /// </summary>
            [DataMember(Name = "Setting")]
            public string Setting { get; set; }

            /// <summary>
            /// Gets or sets the value of the setting
            /// </summary>
            [DataMember(Name = "Value")]
            public object Value { get; set; }
        }

        /// <summary>
        /// Object representing the collection of Mixed Reality Capture settings
        /// </summary>
        [DataContract]
        public class MrcSettings
        {
            /// <summary>
            /// Gets the collection of settings
            /// </summary>
            [DataMember(Name = "MrcSettings")]    
            public List<MrcSetting> Settings { get; private set; }

            /// <summary>
            /// Gets or sets a value indicating whether or not holograms are included.
            /// </summary>
            public bool IncludeHolograms
            {
                get
                {
                    object setting = this.GetSetting("EnableHolograms");

                    if (setting == null)
                    {
                        return true;
                    }

                    return (bool)setting;
                }
                
                set
                {
                    this.SetSetting(
                        "EnableHolograms",
                        value);
                }
            }

            /// <summary>
            /// Gets or sets a value indicating whether or not color camera data is included.
            /// </summary>
            public bool IncludeColorCamera
            {
                get
                {
                    object setting = this.GetSetting("EnableCamera");

                    if (setting == null)
                    {
                        return true;
                    }

                    return (bool)setting;
                }

                set
                {
                    this.SetSetting(
                        "EnableCamera",
                        value);
                }
            }

            /// <summary>
            /// Gets or sets a value indicating whether or not microphone audio is included.
            /// </summary>
            public bool IncludeMicrophone
            {
                get
                {
                    object setting = this.GetSetting("EnableMicrophone");

                    if (setting == null)
                    {
                        return true;
                    }

                    return (bool)setting;
                }

                set
                {
                    this.SetSetting(
                        "EnableMicrophone",
                        value);
                }
            }

            /// <summary>
            /// Gets or sets a value indicating whether or not audio is included.
            /// </summary>
            public bool IncludeAudio
            {
                get
                {
                    object setting = this.GetSetting("EnableSystemAudio");

                    if (setting == null)
                    {
                        return true;
                    }

                    return (bool)setting;
                }

                set
                {
                    this.SetSetting(
                        "EnableSystemAudio",
                        value);
                }
            }

            /// <summary>
            /// Gets or sets the size, in frames, of the video stabilization buffer.
            /// </summary>
            public int VideoStabilizationBuffer
            {
                get
                {
                    object setting = this.GetSetting("VideoStabilizationBuffer");

                    if (setting == null)
                    {
                        return 0;
                    }

                    return (int)setting;
                }

                set
                {
                    if (value < 0)
                    {
                        throw new ArgumentException("The video stabilization buffer value must be >= 0");
                    }
                    
                    this.SetSetting(
                        "VideoStabilizationBuffer",
                        value);
                }
            }

            /// <summary>
            /// Gets the value of a setting
            /// </summary>
            /// <param name="settingName">The name of the setting</param>
            /// <returns>The value of the setting, or if not found, null.</returns>
            private object GetSetting(string settingName)
            {
                object value = null;

                foreach (MrcSetting setting in this.Settings)
                {
                    if (setting.Setting == settingName)
                    {
                        value = setting.Value;
                        break;
                    }
                }

                return value;
            }

            /// <summary>
            /// Sets the value of a Mixed Reality Capture setting.
            /// </summary>
            /// <param name="settingName">The name of the setting</param>
            /// <param name="value">The value of the setting</param>
            private void SetSetting(
                string settingName,
                object value)
            {
                // If the setting exists, update the value, otherwise create a new one.
                MrcSetting mrcSetting = null;

                foreach (MrcSetting setting in this.Settings)
                {
                    if (setting.Setting == settingName)
                    {
                        mrcSetting = setting;
                        break;
                    }
                }

                if (mrcSetting != null)
                {
                    mrcSetting.Value = value;
                }
                else
                {
                    mrcSetting = new MrcSetting();
                    mrcSetting.Setting = settingName;
                    mrcSetting.Value = value;

                    this.Settings.Add(mrcSetting);
                }
            }
        }
        #endregion Data contract
    }
}
