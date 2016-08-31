//----------------------------------------------------------------------------------------------
// <copyright file="IoTOnboarding.cs" company="Microsoft Corporation">
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
        public static readonly string SoftAPSettingsApi = "api/iot/iotonboarding/softapsettings";
        public static readonly string AllJoynSettingsApi = "api/iot/iotonboarding/alljoynsettings";

        /// <summary>
        /// Gets SoftAp Settings.
        /// </summary>
        public async Task<SoftAPSettingsInfo> GetSoftAPSettingsInfo()
        {
            return await this.Get<SoftAPSettingsInfo>(SoftAPSettingsApi);
        }

        /// <summary>
        /// Gets AllJoyn Settings.
        /// </summary>
        public async Task <AllJoynSettingsInfo> GetAllJoynSettingsInfo()
        {
            return await this.Get<AllJoynSettingsInfo>(AllJoynSettingsApi);
        }

        /// <summary>
        /// Sets SoftAp Settings.
        /// </summary>
        public async Task SetSoftApSettings(string softApStatus, string softApSsid, string softApPassword)
        {
            await this.Post(
                 SoftAPSettingsApi,
                string.Format("SoftApEnabled={0}&SoftApSsid={1}&SoftApPassword={2}", Utilities.Hex64Encode(softApStatus), Utilities.Hex64Encode(softApSsid), Utilities.Hex64Encode(softApPassword)));
        }

        /// <summary>
        /// Sets AllJoyn Settings.
        /// </summary>
        public async Task SetAllJoynSettings(string allJoynStatus, string allJoynDescription, string allJoynManufacturer, string allJoynModelNumber)
        {
            await this.Post(
                 AllJoynSettingsApi,
                string.Format("AllJoynOnboardingEnabled={0}&AllJoynOnboardingDefaultDescription={1}&AllJoynOnboardingDefaultManufacturer={2}&AllJoynOnboardingModelNumber={3}", Utilities.Hex64Encode(allJoynStatus), Utilities.Hex64Encode(allJoynDescription), Utilities.Hex64Encode(allJoynManufacturer), Utilities.Hex64Encode(allJoynModelNumber)));
        }

        #region Data contract
       
        /// <summary>
        /// SoftAp Settings.
        /// </summary>
        [DataContract]
        public class SoftAPSettingsInfo
        {
            /// <summary>
            /// Gets or sets SoftAp status 
            /// </summary>
            [DataMember(Name = "SoftAPEnabled")]
            public string SoftAPEnabled { get; set; }

            /// <summary>
            /// Gets or sets SoftAp password
            /// </summary>
            [DataMember(Name = "SoftApPassword")]
            public string SoftApPassword { get; set; }

            /// <summary>
            /// Gets or sets SoftAp SSID
            /// </summary>
            [DataMember(Name = "SoftApSsid")]
            public string SoftApSsid { get; set; }

        }

        /// <summary>
        /// AllJoyn Settings.
        /// </summary>
        [DataContract]
        public class AllJoynSettingsInfo
        {

            /// <summary>
            /// Gets or sets AllJoyn Onboarding Default Description
            /// </summary>
            [DataMember(Name = "AllJoynOnboardingDefaultDescription")]
            public string AllJoynOnboardingDefaultDescription { get; set; }

            /// <summary>
            /// Gets or sets AllJoyn Onboarding Default Manufacturer
            /// </summary>
            [DataMember(Name = "AllJoynOnboardingDefaultManufacturer")]
            public string AllJoynOnboardingDefaultManufacturer { get; set; }

            /// <summary>
            /// Gets or sets AllJoyn Onboarding status
            /// </summary>
            [DataMember(Name = "AllJoynOnboardingEnabled")]
            public string AllJoynOnboardingEnabled { get; set; }

            /// <summary>
            /// Gets or sets AllJoyn Onboarding Model Number
            /// </summary>
            [DataMember(Name = "AllJoynOnboardingModelNumber")]
            public string AllJoynOnboardingModelNumber { get; set; }
        }
        #endregion // Data contract
    }
}
