// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE.TXT in the project root license information.

using System;
using System.Collections.Generic;
using System.IO;
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
            if (Platform != DevicePortalPlatforms.HoloLens)
            {
                throw new NotSupportedException("This method is only supported on HoloLens.");
            }

            await Delete(_mrcFileApi, 
                        String.Format("filename={0}", Utilities.Hex64Encode(fileName)));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="isThumbnailRequest"></param>
        /// <returns></returns>
        public async Task<Byte[]> GetMrcFileData(String fileName,
                                                Boolean isThumbnailRequest = false)
        {
            if (Platform != DevicePortalPlatforms.HoloLens)
            {
                throw new NotSupportedException("This method is only supported on HoloLens.");
            }

            Byte[] dataBytes = null;

            String apiPath = isThumbnailRequest ? _mrcThumbnailApi : _mrcFileApi;

            using (MemoryStream data = await Get<MemoryStream>(apiPath,
                                                        String.Format("filename={0}", Utilities.Hex64Encode(fileName))))
            {
                dataBytes = new Byte[data.Length];
                data.Read(dataBytes, 0, dataBytes.Length);
            }

            return dataBytes;   
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<MrcFileList> GetMrcFileList()
        {
            if (Platform != DevicePortalPlatforms.HoloLens)
            {
                throw new NotSupportedException("This method is only supported on HoloLens.");
            }

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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<MrcStatus> GetMrcStatus()
        {
            if (Platform != DevicePortalPlatforms.HoloLens)
            {
                throw new NotSupportedException("This method is only supported on HoloLens.");
            }

            return await Get<MrcStatus>(_mrcStatusApi);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public async Task<Byte[]> GetMrcThumbnailData(String fileName)
        {
            // GetMrcFileData checks for the appropriate platform. We do not need to duplicate the check here.
            return await GetMrcFileData(fileName, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="includeHolograms"></param>
        /// <param name="includeColorCamera"></param>
        /// <param name="includeMicrophone"></param>
        /// <param name="includeAudio"></param>
        /// <returns></returns>
        public async Task StartMrcRecording(Boolean includeHolograms = true,
                                        Boolean includeColorCamera = true,
                                        Boolean includeMicrophone = true,
                                        Boolean includeAudio = true)
        {
            if (Platform != DevicePortalPlatforms.HoloLens)
            {
                throw new NotSupportedException("This method is only supported on HoloLens.");
            }

            String payload = String.Format("holo={0}&pv={1}&mic={2}&loopback={3}",
                                        includeHolograms, includeColorCamera,
                                        includeMicrophone, includeAudio).ToLower();

            await Post(_mrcStartRecordingApi,
                    payload);
        }
        
        public async Task StopMrcRecording()
        {
            if (Platform != DevicePortalPlatforms.HoloLens)
            {
                throw new NotSupportedException("This method is only supported on HoloLens.");
            }

            await Post(_mrcStopRecordingApi);
        }

        public async Task TakeMrcPhoto(Boolean includeHolograms = true,
                                    Boolean includeColorCamera = true)
        {
            if (Platform != DevicePortalPlatforms.HoloLens)
            {
                throw new NotSupportedException("This method is only supported on HoloLens.");
            }

            await Post(_mrcPhotoApi,
                    String.Format("holo={0}&pv={1}",includeHolograms, includeColorCamera).ToLower());
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

    [DataContract]
    public class MrcStatus
    {
        [DataMember(Name="IsRecording")]
        public Boolean IsRecording { get; set; }

        [DataMember(Name="ProcessStatus")]
        public ProcessStatus Status { get; set; }
        
    }

    [DataContract]
    public class ProcessStatus
    {
        [DataMember(Name="MrcProcess")]
        public String MrcProcess { get; set; }  // TODO this should be an enum
       
    }
    #endregion Data contract
}
