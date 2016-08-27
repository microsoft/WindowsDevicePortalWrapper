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
       


        public async Task<SoftAPSettingsInfo> GetSoftAPSettingsInfo()
        {
            return await this.Get<SoftAPSettingsInfo>(SoftAPSettingsApi);
        }
        public async Task <AllJoynSettingsInfo> GetAllJoynSettingsInfo()
        {
            return await this.Get<AllJoynSettingsInfo>(AllJoynSettingsApi);
        }

        #region Data contract

        [DataContract]
        public class SoftAPSettingsInfo
        {
            [DataMember(Name = "SoftAPEnabled")]
            public string SoftAPEnabled { get; set; }

            [DataMember(Name = "SoftApPassword")]
            public string SoftApPassword { get; set; }

            [DataMember(Name = "SoftApSsid")]
            public string SoftApSsid { get; set; }

        }

        [DataContract]
        public class AllJoynSettingsInfo
        {
            [DataMember(Name = "AllJoynOnboardingDefaultDescription")]
            public string AllJoynOnboardingDefaultDescription { get; set; }

            [DataMember(Name = "AllJoynOnboardingDefaultManufacturer")]
            public string AllJoynOnboardingDefaultManufacturer { get; set; }

            [DataMember(Name = "AllJoynOnboardingEnabled")]
            public string AllJoynOnboardingEnabled { get; set; }

            [DataMember(Name = "AllJoynOnboardingModelNumber")]
            public string AllJoynOnboardingModelNumber;
        }
        #endregion // Data contract
    }
}
