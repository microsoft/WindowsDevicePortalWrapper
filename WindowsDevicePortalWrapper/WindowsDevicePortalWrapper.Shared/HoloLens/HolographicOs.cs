﻿// <copyright file="HolographicOs.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <content>
    /// Wrappers for Holographic OS methods
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// API for getting or setting Interpupilary distance
        /// </summary>
        public static readonly string HolographicIpdApi = "api/holographic/os/settings/ipd";

        /// <summary>
        /// API for getting a list of running HoloLens specific services.
        /// </summary>
        public static readonly string HolographicServicesApi = "api/holographic/os/services";

        /// <summary>
        /// API for getting or setting HTTPS setting
        /// </summary>
        public static readonly string HolographicWebManagementHttpSettingsApi = "api/holographic/os/webmanagement/settings/https";

        /// <summary>
        /// Gets the interpupilary distance registered on the device.
        /// </summary>
        /// <returns>Interpupilary distance, in millimeters.</returns>
        /// <remarks>This method is only supported on HoloLens devices.</remarks>
        public async Task<float> GetInterPupilaryDistanceAsync()
        {
            if (!Utilities.IsHoloLens(this.Platform, this.DeviceFamily))
            {
                throw new NotSupportedException("This method is only supported on HoloLens.");
            }

            InterPupilaryDistance ipd = await this.GetAsync<InterPupilaryDistance>(HolographicIpdApi);
            return ipd.Ipd;
        }

        /// <summary>
        /// Sets the WiFi http security requirements for communication with the device.
        /// </summary>
        /// <param name="httpsRequired">Desired value for HTTPS communication</param>
        /// <returns>True if WiFi based communication requires a secure connection, false otherwise.</returns>
        /// <remarks>This method is only supported on HoloLens devices.</remarks>
        public async Task SetIsHttpsRequiredAsync(bool httpsRequired)
        {
            if (!Utilities.IsHoloLens(this.Platform, this.DeviceFamily))
            {
                throw new NotSupportedException("This method is only supported on HoloLens.");
            }

            await this.PostAsync(
                HolographicWebManagementHttpSettingsApi,
                string.Format("required={0}", httpsRequired));

            this.deviceConnection.UpdateConnection(httpsRequired);
        }

        /// <summary>
        /// Sets the interpupilary distance registered on the device.
        /// </summary>
        /// <param name="ipd">Interpupilary distance, in millimeters.</param>
        /// <returns>Task for tracking the POST call</returns>
        /// <remarks>This method is only supported on HoloLens devices.</remarks>
        public async Task SetInterPupilaryDistanceAsync(float ipd)
        {
            if (!Utilities.IsHoloLens(this.Platform, this.DeviceFamily))
            {
                throw new NotSupportedException("This method is only supported on HoloLens.");
            }

            string payload = string.Format("ipd={0}", (int)(ipd * 1000.0f));

            await this.PostAsync(
                HolographicIpdApi,
                payload);
        }

        /// <summary>
        /// Gets the WiFi http security requirements for communication with the device.
        /// </summary>
        /// <returns>True if WiFi based communication requires a secure connection, false otherwise.</returns>
        /// <remarks>This method is only supported on HoloLens devices.</remarks>
        public async Task<bool> GetIsHttpsRequiredAsync()
        {
            if (!Utilities.IsHoloLens(this.Platform, this.DeviceFamily))
            {
                throw new NotSupportedException("This method is only supported on HoloLens.");
            }

            WebManagementHttpSettings httpSettings = await this.GetAsync<WebManagementHttpSettings>(HolographicWebManagementHttpSettingsApi);
            return httpSettings.IsHttpsRequired;
        }

        #region Data contract
        /// <summary>
        /// Object representation for HTTP settings
        /// </summary>
        [DataContract]
        public class WebManagementHttpSettings
        {
            /// <summary>
            /// Gets a value indicating whether HTTPS is required
            /// </summary>
            [DataMember(Name = "httpsRequired")]
            public bool IsHttpsRequired { get; private set; }
        }

        /// <summary>
        /// Object representation for Interpupilary distance
        /// </summary>
        [DataContract]
        public class InterPupilaryDistance
        {
            /// <summary>
            /// Gets the raw interpupilary distance
            /// </summary>
            [DataMember(Name = "ipd")]
            public int IpdRaw { get; private set; }

            /// <summary>
            /// Gets or sets the interpupilary distance
            /// </summary>
            public float Ipd
            {
                get { return this.IpdRaw / 1000.0f; }
                set { this.IpdRaw = (int)(value * 1000); }
            }
        }
        #endregion // Data contract
    }
}
