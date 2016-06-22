// <copyright file="HolographicOs.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsDevicePortal
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;
    using System.Threading.Tasks;

    /// <content>
    /// Wrappers for Holographic OS methods
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// API for getting or setting InterPupilaryDistance
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Interpupilary is spelled correctly.")]
        private static readonly string IpdApi = "api/holographic/os/settings/ipd";

        /// <summary>
        /// API for getting or setting HTTPS setting
        /// </summary>
        private static readonly string WebManagementHttpSettingsApi = "api/holographic/os/webmanagement/settings/https";

        /// <summary>
        /// Gets the interpupilary distance registered on the device.
        /// </summary>
        /// <returns>Interpupilary distance, in millimeters.</returns>
        /// <remarks>This method is only supported on HoloLens devices.</remarks>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Interpupilary and HoloLens are spelled correctly.")]
        public async Task<float> GetInterPupilaryDistance()
        {
            if (Platform != DevicePortalPlatforms.HoloLens)
            {
                throw new NotSupportedException("This method is only supported on HoloLens.");
            }

            InterPupilaryDistance ipd = await Get<InterPupilaryDistance>(IpdApi);
            return ipd.Ipd;
        }

        /// <summary>
        /// Sets the WiFi http security requirements for communication with the device.
        /// </summary>
        /// <param name="httpsRequired">Desired value for HTTPS communication</param>
        /// <returns>True if WiFi based communication requires a secure connection, false otherwise.</returns>
        /// <remarks>This method is only supported on HoloLens devices.</remarks>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "HoloLens is spelled correctly.")]
        public async Task SetIsHttpsRequired(bool httpsRequired)
        {
            if (Platform != DevicePortalPlatforms.HoloLens)
            {
                throw new NotSupportedException("This method is only supported on HoloLens.");
            }

            await Post(
                WebManagementHttpSettingsApi,
                string.Format("required={0}", httpsRequired));

            deviceConnection.UpdateConnection(httpsRequired);
        }

        /// <summary>
        /// Sets the interpupilary distance registered on the device.
        /// </summary>
        /// <param name="ipd">Interpupilary distance, in millimeters.</param>
        /// <returns>Task for tracking the POST call</returns>
        /// <remarks>This method is only supported on HoloLens devices.</remarks>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Interpupilary is spelled correctly.")]
        public async Task SetInterPupilaryDistance(float ipd)
        {
            if (Platform != DevicePortalPlatforms.HoloLens)
            {
                throw new NotSupportedException("This method is only supported on HoloLens.");
            }

            string payload = string.Format("ipd={0}", (int)(ipd * 1000.0f));

            await Post(
                IpdApi,
                payload);
        }

        /// <summary>
        /// Gets the WiFi http security requirements for communication with the device.
        /// </summary>
        /// <returns>True if WiFi based communication requires a secure connection, false otherwise.</returns>
        /// <remarks>This method is only supported on HoloLens devices.</remarks>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "HoloLens is spelled correctly.")]
        private async Task<bool> GetIsHttpsRequired()
        {
            try
            {
                if ((Platform != DevicePortalPlatforms.Unknown) &&
                    (Platform != DevicePortalPlatforms.HoloLens))
                {
                    throw new NotSupportedException("This method is only supported on HoloLens.");
                }

                WebManagementHttpSettings httpSettings = await Get<WebManagementHttpSettings>(WebManagementHttpSettingsApi);
                return httpSettings.IsHttpsRequired;
            }
            catch (Exception e)
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

        #region Data contract
        /// <summary>
        /// Object representation for HTTP settings
        /// </summary>
        [DataContract]
        public class WebManagementHttpSettings
        {
            /// <summary>
            /// Gets or sets a value indicating whether HTTPS is required
            /// </summary>
            [DataMember(Name = "httpsRequired")]
            public bool IsHttpsRequired { get; set; }
        }

        /// <summary>
        /// Object representation for Interpupilary Distance
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Interpupilary is spelled correctly.")]
        [DataContract]
        public class InterPupilaryDistance
        {
            /// <summary>
            /// Gets or sets the raw interpupilary distance
            /// </summary>
            [DataMember(Name = "ipd")]
            public int IpdRaw { get; set; }

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
