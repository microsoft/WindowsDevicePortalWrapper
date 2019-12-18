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
    /// <summary>
    /// Wrappers for IoT Onboarding methods.
    /// </summary>
    public partial class DevicePortal
    {
        /// <summary>
        /// IOT SoftAP Settings API.
        /// </summary>
        public static readonly string SoftAPSettingsApi = "api/iot/iotonboarding/softapsettings";

        /// <summary>
        /// IOT  AllJoyn Settings API.
        /// </summary>
        public static readonly string AllJoynSettingsApi = "api/iot/iotonboarding/alljoynsettings";

        /// <summary>
        /// Retrieves the Soft AP Settings Info.
        /// </summary>
        /// <returns>SoftAPSettingsInfo for this device.</returns>
        public async Task<SoftAPSettingsInfo> GetSoftAPSettingsInfoAsync()
        {
            return await this.GetAsync<SoftAPSettingsInfo>(SoftAPSettingsApi);
        }

        /// <summary>
        /// Retrieves the All Joyn Settings Info.
        /// </summary>
        /// <returns>AllJoynSettingsInfo for this device.</returns>
        public async Task<AllJoynSettingsInfo> GetAllJoynSettingsInfoAsync()
        {
            return await this.GetAsync<AllJoynSettingsInfo>(AllJoynSettingsApi);
        }

        /// <summary>
        /// Sets SoftAp Settings.
        /// </summary>
        /// <param name="softApStatus">SoftAp Status.</param>
        /// <param name="softApSsid">SoftAp Ssid.</param>
        /// /// <param name="softApPassword">SoftAp Password.</param>
        /// <returns>Task tracking completion of the REST call.</returns>
        public async Task SetSoftApSettingsAsync(string softApStatus, string softApSsid, string softApPassword)
        {
            await this.PostAsync(
                 SoftAPSettingsApi,
                string.Format("SoftAPEnabled={0}&SoftApSsid={1}&SoftApPassword={2}", Utilities.Hex64Encode(softApStatus), Utilities.Hex64Encode(softApSsid), Utilities.Hex64Encode(softApPassword)));
        }

        /// <summary>
        /// Sets AllJoyn Settings.
        /// </summary>
        /// <param name="allJoynStatus">AllJoyn Status.</param>
        /// <param name="allJoynDescription">AllJoyn Description.</param>
        /// <param name="allJoynManufacturer"> AllJoyn Manufacturer.</param>
        /// <param name="allJoynModelNumber"> AllJoyn Number.</param>
        /// <returns>Task tracking completion of the REST call.</returns>
        public async Task SetAllJoynSettingsAsync(string allJoynStatus, string allJoynDescription, string allJoynManufacturer, string allJoynModelNumber)
        {
            await this.PostAsync(
                 AllJoynSettingsApi,
                string.Format("AllJoynOnboardingEnabled={0}&AllJoynOnboardingDefaultDescription={1}&AllJoynOnboardingDefaultManufacturer={2}&AllJoynOnboardingModelNumber={3}", Utilities.Hex64Encode(allJoynStatus), Utilities.Hex64Encode(allJoynDescription), Utilities.Hex64Encode(allJoynManufacturer), Utilities.Hex64Encode(allJoynModelNumber)));
        }

        #region Data contract
       
        /// <summary>
        /// Object representation for Soft AP Settings.
        /// </summary>
        [DataContract]
        public class SoftAPSettingsInfo
        {
            /// <summary>
            /// Gets whether Soft AP is enabled.
            /// </summary>
            [DataMember(Name = "SoftAPEnabled")]
            public string SoftAPEnabled { get; private set; }

            /// <summary>
            /// Gets the Soft AP Password.
            /// </summary>
            [DataMember(Name = "SoftApPassword")]
            public string SoftApPassword { get; private set; }

            /// <summary>
            /// Gets the Soft AP SSID.
            /// </summary>
            [DataMember(Name = "SoftApSsid")]
            public string SoftApSsid { get; private set; }
        }

        /// <summary>
        /// Object represenation of All Joyn Settings.
        /// </summary>
        [DataContract]
        public class AllJoynSettingsInfo
        {
            /// <summary>
            /// Gets the Default description.
            /// </summary>
            [DataMember(Name = "AllJoynOnboardingDefaultDescription")]
            public string AllJoynOnboardingDefaultDescription { get; private set; }

            /// <summary>
            /// Gets the Default Manufacturer.
            /// </summary>
            [DataMember(Name = "AllJoynOnboardingDefaultManufacturer")]
            public string AllJoynOnboardingDefaultManufacturer { get; private set; }

            /// <summary>
            /// Gets whether this is enabled.
            /// </summary>
            [DataMember(Name = "AllJoynOnboardingEnabled")]
            public string AllJoynOnboardingEnabled { get; private set; }

            /// <summary>
            /// Gets the model number.
            /// </summary>
            [DataMember(Name = "AllJoynOnboardingModelNumber")]
            public string AllJoynOnboardingModelNumber { get; private set; }
        }
        #endregion // Data contract
    }
}
