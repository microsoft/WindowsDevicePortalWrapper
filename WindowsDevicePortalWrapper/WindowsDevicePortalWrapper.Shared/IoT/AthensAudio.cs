//----------------------------------------------------------------------------------------------
// <copyright file="AthensAudio.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <content>
    /// Wrappers for Athens Audio Devices.
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// IOT List Audio Devices API.
        /// </summary>
        public static readonly string AudioDeviceListApi = "api/iot/audio/listdevices";

        /// <summary>
        /// API to set render volume on audio devices connected to IoT.
        /// </summary>
        public static readonly string SetRenderVolumeApi = "api/iot/audio/setrendervolume";

        /// <summary>
        /// API to set capture volume on audio devices connected to IoT.
        /// </summary>
        public static readonly string SetCaptureVolumeApi = "api/iot/audio/setcapturevolume";
        
        /// <summary>
        /// Gets the Audio Device List Information.
        /// </summary>
        /// <returns>String containing the Audio Device List information.</returns>
        public async Task<AudioDeviceListInfo> GetAudioDeviceListInfo()
        {
            return await this.Get<AudioDeviceListInfo>(AudioDeviceListApi);
        }

        /// <summary>
        /// Sets volume for the audio devices.
        /// </summary>
        /// <returns>Task tracking completion of the REST call.</returns>
        public async Task SetRenderVolume(string renderVolume)
        {
            await this.Post(
                 SetRenderVolumeApi,
                string.Format("rendervolume={0}", Utilities.Hex64Encode(renderVolume)));
        }

        /// <summary>
        /// Sets volume for the audio devices.
        /// </summary>
        /// <returns>Task tracking completion of the REST call.</returns>
        public async Task SetCaptureVolume(string captureVolume)
        {
            await this.Post(
                 SetCaptureVolumeApi,
                string.Format("capturevolume={0}", Utilities.Hex64Encode(captureVolume)));
        }

        #region Data contract

        /// <summary>
        /// Audio Device List information.
        /// </summary>
        [DataContract]
        public class AudioDeviceListInfo
        {
            /// <summary>
            /// Gets the audio device name
            /// </summary>
            [DataMember(Name = "RenderName")]
            public string RenderName { get; set; }

            /// <summary>
            ///  Gets the audio device volume
            /// </summary>
            [DataMember(Name = "RenderVolume")]
            public string RenderVolume { get; set; }

            /// <summary>
            /// Gets the microphones name
            /// </summary>
            [DataMember(Name = "CaptureName")]
            public string CaptureName { get; set; }

            /// <summary>
            ///  Gets the microphones volume
            /// </summary>
            [DataMember(Name = "CaptureVolume")]
            public string CaptureVolume { get; set; }

            /// <summary>
            ///  Gets the status of a failed devices 
            /// </summary>
            [DataMember(Name = "LabelStatus")]
            public string LabelStatus { get; set; }

            /// <summary>
            ///  Gets the error code for the device failure 
            /// </summary>
            [DataMember(Name = "LabelErrorCode")]
            public string LabelErrorCode { get; set; }
        }
        #endregion // Data contract
    }
}
