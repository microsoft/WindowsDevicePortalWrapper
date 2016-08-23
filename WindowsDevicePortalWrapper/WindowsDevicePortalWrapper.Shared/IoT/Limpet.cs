//----------------------------------------------------------------------------------------------
// <copyright file="OsInformation.cs" company="Microsoft Corporation">
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

        public static readonly string TmpSettingsApi = "api/iot/tpm/settings";
        public static readonly string TpmGetAcpiTablesApi = "api/iot/tpm/acpitables";
        public static readonly string TpmGetLogicalDeviceSettingsApi = "/api/iot/tpm/settings/#";
        public static readonly string TpmGetAzureTokenApi = "api/iot/tpm/azuretoken/#";


        public async Task<StatusInfo1> GetTmpSettingsInfo()
        {
            return await this.Get<StatusInfo1>(TmpSettingsApi);
        }
        public async Task<UpdateInstallTimeInfo1> TpmGetAcpiTablesInfo()
        {
            return await this.Get<UpdateInstallTimeInfo1>(TpmGetAcpiTablesApi);
        }

        public async Task<StatusInfo1> TpmGetLogicalDeviceSettingsInfo()
        {
            return await this.Get<StatusInfo1>(TpmGetLogicalDeviceSettingsApi);
        }
        public async Task<UpdateInstallTimeInfo1> TpmGetAzureTokenInfo()
        {
            return await this.Get<UpdateInstallTimeInfo1>(TpmGetAzureTokenApi);
        }

        #region Data contract

        [DataContract]
        public class StatusInfo1
        {
            [DataMember(Name = "lastCheckTime")]
            public string lastCheckTime;

            [DataMember(Name = "lastUpdateTime")]
            public string lastUpdateTime;

            [DataMember]
            public int updateState;

            [DataMember]
            public string updateStatusMessage;


        }



        public class UpdateInstallTimeInfo1
        {
            [DataMember(Name = "rebootscheduled")]
            public int rebootscheduled;
        }

        #endregion // Data contract
    }
}
