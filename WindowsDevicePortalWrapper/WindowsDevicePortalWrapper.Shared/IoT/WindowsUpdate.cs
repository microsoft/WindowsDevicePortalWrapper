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
    /// <summary>
    /// Windows update APIs for IoT.
    /// </summary>
    public partial class DevicePortal
    {
        /// <summary>
        /// Install time API.
        /// </summary>
        public static readonly string InstallTimeApi = "api/iot/windowsupdate/installtime";

        /// <summary>
        /// Update status API.
        /// </summary>
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
        /// <returns>String containing the update install time information.</returns>
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
            /// Gets or sets last update check time. 
            /// </summary>
            [DataMember(Name = "lastCheckTime")]
            public string LastCheckTime { get; set; }

            /// <summary>
            ///  Gets or sets last update time.
            /// </summary>
            [DataMember(Name = "lastUpdateTime")]
            public string LastUpdateTime { get; set; }

            /// <summary>
            ///  Gets or sets update status.
            /// </summary>
            [DataMember(Name = "updateState")]
            public int UpdateState { get; set; }

            /// <summary>
            ///  Gets or sets update status message.
            /// </summary>
            [DataMember(Name = "updateStatusMessage")]
            public string UpdateStatusMessage { get; set; }
        }

        /// <summary>
        /// Update install time information.
        /// </summary>
        public class UpdateInstallTimeInfo
        {
            /// <summary>
            /// Gets or sets whether a reboot is scheduled. 
            /// </summary>
            [DataMember(Name = "rebootscheduled")]
            public int Rebootscheduled { get; set; }
        }

        #endregion // Data contract
    }
}
