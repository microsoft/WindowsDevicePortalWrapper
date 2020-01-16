//----------------------------------------------------------------------------------------------
// <copyright file="AudioDevices.cs" company="Microsoft Corporation">
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
    /// Wrappers for Audio Devices.
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// List Audio Devices API.
        /// </summary>
        public static readonly string AudioDeviceListApi = "api/iot/audio/listdevices";

        /// <summary>
        /// API to set render volume on audio devices connected to IoT Core devices.
        /// </summary>
        public static readonly string SetRenderVolumeApi = "api/iot/audio/setrendervolume";

        /// <summary>
        /// API to set capture volume on audio devices connected to IoT Core devices.
        /// </summary>
        public static readonly string SetCaptureVolumeApi = "api/iot/audio/setcapturevolume";
        
        /// <summary>
        /// Gets the Audio Device List Information.
        /// </summary>
        /// <returns>String containing the Audio Device List information.</returns>
        public async Task<AudioDeviceListInfo> GetAudioDeviceListInfoAsync()
        {
            return await this.GetAsync<AudioDeviceListInfo>(AudioDeviceListApi);
        }

        /// <summary>
        /// Sets volume for the audio devices.
        /// </summary>
        /// <param name="renderVolume">Render Volume.</param>
        /// <returns>Task tracking completion of the REST call.</returns>
        public async Task SetRenderVolumeAsync(string renderVolume)
        {
            await this.PostAsync(
                 SetRenderVolumeApi,
                string.Format("rendervolume={0}", Utilities.Hex64Encode(renderVolume)));
        }

        /// <summary>
        /// Sets volume for the audio devices.
        /// </summary>
        /// <param name="captureVolume">Capture Volume.</param>
        /// <returns>Task tracking completion of the REST call.</returns>
        public async Task SetCaptureVolumeAsync(string captureVolume)
        {
            await this.PostAsync(
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
            public string RenderName { get; private set; }

            /// <summary>
            ///  Gets the audio device volume
            /// </summary>
            [DataMember(Name = "RenderVolume")]
            public string RenderVolume { get; private set; }

            /// <summary>
            /// Gets the microphones name
            /// </summary>
            [DataMember(Name = "CaptureName")]
            public string CaptureName { get; private set; }

            /// <summary>
            ///  Gets the microphones volume
            /// </summary>
            [DataMember(Name = "CaptureVolume")]
            public string CaptureVolume { get; private set; }

            /// <summary>
            ///  Gets the status of a failed devices 
            /// </summary>
            [DataMember(Name = "LabelStatus")]
            public string LabelStatus { get; private set; }

            /// <summary>
            ///  Gets the error code for the device failure 
            /// </summary>
            [DataMember(Name = "LabelErrorCode")]
            public string LabelErrorCode { get; private set; }
        }
        #endregion // Data contract
    }
}
