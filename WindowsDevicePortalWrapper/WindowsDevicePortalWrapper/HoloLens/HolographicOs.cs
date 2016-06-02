// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE.TXT in the project root license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal
{
    public partial class DevicePortal
    {
        private static readonly String _ipdApi = "api/holographic/os/settings/ipd";
        private static readonly String _servicesApi  = "api/holographic/os/services";
        private static readonly String _webManagementHttpSettingsApi = "api/holographic/os/webmanagement/settings/https";

        /// <summary>
        /// Gets the interpupilary distance registered on the device.
        /// </summary>
        /// <returns>Interpupilary distance, in millimeters.</returns>
        /// <remarks>This method is only supported on HoloLens devices.</remarks>
        public async Task<Single> GetInterPupilaryDistance()
        {
            if (Platform != DevicePortalPlatforms.HoloLens)
            {
                throw new NotSupportedException("This method is only supported on HoloLens.");
            }

            InterPupilaryDistance ipd = await Get<InterPupilaryDistance>(_ipdApi);
            return ipd.Ipd;
        }

        /// <summary>
        /// Gets the WiFi http security requirements for communication with the device.
        /// </summary>
        /// <returns>True if WiFi based communication requires a secure connection, false otherwise.</returns>
        /// <remarks>This method is only supported on HoloLens devices.</remarks>
        private async Task<Boolean> GetIsHttpsRequired()
        {
            try
            {
                if ((Platform != DevicePortalPlatforms.Unknown) && 
                    (Platform != DevicePortalPlatforms.HoloLens))
                {
                    throw new NotSupportedException("This method is only supported on HoloLens.");
                }

                WebManagementHttpSettings httpSettings = await Get<WebManagementHttpSettings>(_webManagementHttpSettingsApi);
                return httpSettings.IsHttpsRequired;
            }
            catch(Exception e)
            {
                DevicePortalException dpe = e as DevicePortalException;

                if ((dpe != null) &&
                    (dpe.StatusCode != System.Net.HttpStatusCode.NotFound))
                {
                    throw new NotSupportedException("This method is only supported on HoloLens.");
                }

                throw;
            }
        }

        /// <summary>
        /// Sets the WiFi http security requirements for communication with the device.
        /// </summary>
        /// <returns>True if WiFi based communication requires a secure connection, false otherwise.</returns>
        /// <remarks>This method is only supported on HoloLens devices.</remarks>
        public async Task SetIsHttpsRequired(Boolean httpsRequired)
        {
            if (Platform != DevicePortalPlatforms.HoloLens)
            {
                throw new NotSupportedException("This method is only supported on HoloLens.");
            }

            await Post(_webManagementHttpSettingsApi,
                                String.Format("required={0}", httpsRequired));

            _deviceConnection.UpdateConnection(httpsRequired);
        }

        /// <summary>
        /// Sets the interpupilary distance registered on the device.
        /// </summary>
        /// <param name="ipd">Interpupilary distance, in millimeters.</param>
        /// <remarks>This method is only supported on HoloLens devices.</remarks>
        public async Task SetInterPupilaryDistance(Single ipd)
        {
            if (Platform != DevicePortalPlatforms.HoloLens)
            {
                throw new NotSupportedException("This method is only supported on HoloLens.");
            }

            String payload = String.Format("ipd={0}", (Int32)(ipd * 1000.0f));
            await Post(_ipdApi,
                                payload);
        }
    }

    #region Data contract
    [DataContract]
    public class WebManagementHttpSettings
    {
        [DataMember(Name="httpsRequired")]
        public Boolean IsHttpsRequired { get; set; }
    }

    [DataContract]   
    public class InterPupilaryDistance
    {
        [DataMember(Name="ipd")]
        public Int32 IpdRaw{ get; set; }

        public Single Ipd
        {
            get { return IpdRaw / 1000.0f; }
            set { IpdRaw = (Int32)(value * 1000); }
        }
    }
    #endregion // Data contract
}
