// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE.TXT in the project root license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal
{
    public partial class DevicePortal
    {
        private static readonly String _mrcFileApi = "api/holographic/mrc/file";
        private static readonly String _mrcFileListApi = "api/holographic/mrc/files";
        private static readonly String _mrcPhotoApi = "api/holographic/mrc/photo";
        private static readonly String _mrcSettingsApi = "api/holographic/mrc/settings";
        private static readonly String _mrcStartRecordingApi = "api/holographic/mrc/video/control/start";
        private static readonly String _mrcStatusApi = "api/holographic/mrc/status";
        private static readonly String _mrcStopRecordingApi = "api/holographic/mrc/video/control/stop";
        private static readonly String _mrcThumbnailApi = "api/holographic/mrc/thumbnail";

        /// <summary>
        /// Removes a Mixed Reality Capture file from the device's local storage.
        /// </summary>
        /// <param name="fileName">The name of the file to be deleted.</param>
        public async Task DeleteMrcFile(String fileName)
        {
            await Delete(_mrcFileApi, 
                        String.Format("filename={0}", Utilities.Hex64Encode(fileName)));
        }

        public async Task<Byte[]> GetMrcFileData(String fileName)
        {
            throw new NotImplementedException();   
        }

        public async Task<MrcFileList> GetMrcFileList()
        {
            MrcFileList mrcFileList = await Get<MrcFileList>(_mrcFileListApi);

            foreach (MrcFileInformation mfi in mrcFileList.Files)
            {
                try 
                {
                    mfi.Thumbnail = await GetMrcThumbnailData(mfi.FileName);
                }
                catch
                { }
            }

            return mrcFileList;
        }

        public async Task<Byte[]> GetMrcThumbnailData(String fileName)
        {
            throw new NotImplementedException();   
        }

        // BUGBUG public async Task StartMrcRecording()
        // BUGBUG public async Task StopMrcRecording()

        public async Task TakeMrcPhoto(Boolean includeHolograms,
                                    Boolean includeColorCamera)
        {
            throw new NotImplementedException();   
        }

    }

    #region Data contract

    [DataContract]
    public class MrcFileList
    {
        [DataMember(Name="MrcRecordings")]
        public List<MrcFileInformation> Files { get; set; }
    }

    [DataContract]
    public class MrcFileInformation
    {
        [DataMember(Name="CreationTime")]
        public Int64 CreationTimeRaw { get; set; }

        [DataMember(Name="FileName")]
        public String FileName { get; set; }

        [DataMember(Name="FileSize")]
        public UInt32 FileSize { get; set; }

        public Byte[] Thumbnail { get; internal set; }

        public DateTime Created
        {
            get { return new DateTime(CreationTimeRaw); }
        }
    }

    // BUGBUG
    [DataContract]
    public class SettingKeyValuePair
    {
        [DataMember(Name = "Setting")]
        public String Key { get; set; }

        [DataMember(Name = "Value")]
        public Object Value { get; set; }

        public SettingKeyValuePair()
        { }

        public SettingKeyValuePair(String key, Object value)
        {
            Key = key;
            Value = value;
        }
    }

    public class MrcOptions
    {
        public Boolean Audio { get; set; }

        public Boolean ColorCamera { get; set; }
        
        public Boolean Holograms { get; set; }

        public Boolean Microphone { get; set; }

        public MrcOptions()
        {
            Audio = true;
            ColorCamera = true;
            Holograms = true;
            Microphone = true;   
        }
    }

    [DataContract]
    public class MrcSettings
    {
        [DataMember(Name = "MrcSettings")]
        public List<SettingKeyValuePair> Settings { get; set; }

        public MrcSettings()
        { 
            Settings = new List<SettingKeyValuePair>();
        }

        public MrcSettings(MrcOptions options) : this()
        {
            Settings.Add(new SettingKeyValuePair("EnableCamera", options.ColorCamera));
            Settings.Add(new SettingKeyValuePair("EnableHolograms", options.Holograms));
            Settings.Add(new SettingKeyValuePair("EnableSystemAudio", options.Audio));
            Settings.Add(new SettingKeyValuePair("EnableMicrophone", options.Microphone));
        }

        public MrcSettings(MrcOptions options, Int32 stabilizationBuffer) : this(options)
        {
            Settings.Add(new SettingKeyValuePair("VideoStabilizationBuffer", stabilizationBuffer));
        }
    }

    [DataContract]
    public class MrcStatus
    {
        [DataMember(Name="CreationTime")]
        public Int64 CreationTimeRaw { get; set; }

        [DataMember(Name="ProcessStatus")]
        public ProcessStatus Status { get; set; }
        
    }

    [DataContract]
    public class ProcessStatus
    {
        [DataMember(Name="MrcProcess")]
        public String MrcProcess { get; set; }  // BUGBUG this should be an enum
       
    }
    #endregion Data contract
}
