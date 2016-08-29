//----------------------------------------------------------------------------------------------
// <copyright file="WindowsUpdate.cs" company="Microsoft Corporation">
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
        public static readonly string StatusApi = "api/iot/windowsupdate/status";

        /// <summary>
        /// Gets Status information.
        /// </summary>
        /// <returns>String containing the status information.</returns>
        public async Task<StatusInfo> GetStatusInfo()
        {
            return await this.Get<StatusInfo>(StatusApi);
        }

        /// <summary>
        /// Gets the update install time information.
        /// </summary>
        /// <returns>String containing the update install timne information.</returns>
        public async Task<UpdateInstallTimeInfo> GetUpdateInstallTime()
        {
            return await this.Get<UpdateInstallTimeInfo>(InstallTimeApi);
        }

        #region Data contract

        /// <summary>
        /// Status information.
        /// </summary>
        [DataContract]
        public class StatusInfo
        {
            /// <summary>
            /// Gets last update check time. 
            /// </summary>
            [DataMember(Name = "lastCheckTime")]
            public string lastCheckTime;

            /// <summary>
            ///  Gets last update time.
            /// </summary>
            [DataMember(Name = "lastUpdateTime")]
            public string lastUpdateTime;

            /// <summary>
            ///  Gets update status.
            /// </summary>
            [DataMember(Name = "updateState")]
            public int updateState;

            /// <summary>
            ///  Gets update status message.
            /// </summary>
            [DataMember(Name = "updateStatusMessage")]
            public string updateStatusMessage;
        }

        /// <summary>
        /// Update install time information.
        /// </summary>
        public class UpdateInstallTimeInfo
        {
            /// <summary>
            ///  Returns true if a reboot is scheduled. 
            /// </summary>
            [DataMember(Name = "rebootscheduled")]
            public int rebootscheduled;
        }

        #endregion // Data contract
    }
}
