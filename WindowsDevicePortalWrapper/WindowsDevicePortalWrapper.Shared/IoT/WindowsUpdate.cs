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

        public static readonly string InstallTimeApi = "api/iot/windowsupdate/installtime";
        public static readonly string UpdateNowApi = "api/iot/windowsupdate/updatenow";
        public static readonly string UpdateRestartApi = "api/iot/windowsupdate/updaterestart";
        public static readonly string StatusApi = "api/iot/windowsupdate/status";


        public async Task<StatusInfo> GetStatusInfo()
        {
            return await this.Get<StatusInfo>(StatusApi);
        }
        public async Task<UpdateInstallTimeInfo> GetUpdateInstallTime()
        {
            return await this.Get<UpdateInstallTimeInfo>(InstallTimeApi);
        }

        #region Data contract

        [DataContract]
        public class StatusInfo {
            [DataMember(Name = "lastCheckTime")]
            public string lastCheckTime;

            [DataMember(Name = "lastUpdateTime")]
            public string lastUpdateTime;

            [DataMember]
            public int updateState;

            [DataMember]
            public string updateStatusMessage;


        }



        public class UpdateInstallTimeInfo
        {
            [DataMember(Name = "rebootscheduled")]
            public int rebootscheduled;
        }

        #endregion // Data contract
    }
}
