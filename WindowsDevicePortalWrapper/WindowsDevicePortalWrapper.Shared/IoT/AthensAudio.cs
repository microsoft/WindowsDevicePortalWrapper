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

    public partial class DevicePortal
    {
        public static readonly string AudioDeviceListApi = "api/iot/audio/listdevices";
        public static readonly string SetRenderVolumeApi = "api/iot/audio/setrendervolume";
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
        public async Task SetRenderVolume(string renderVolume)
        {
            await this.Post(
                 SetRenderVolumeApi,
                string.Format("rendervolume={0}", Utilities.Hex64Encode(renderVolume)));
        }

        /// <summary>
        /// Sets volume for the audio devices.
        /// </summary>
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
        }
        #endregion // Data contract
    }
}
