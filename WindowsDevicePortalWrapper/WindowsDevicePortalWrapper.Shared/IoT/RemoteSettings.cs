//----------------------------------------------------------------------------------------------
// <copyright file="RemoteSettings.cs" company="Microsoft Corporation">
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
    /// Wrappers for remote settings for IoT.
    /// </summary>
    public partial class DevicePortal
    {
        /// <summary>
        /// Remote status API.
        /// </summary>
        public static readonly string RemoteSettingsStatusApi = "api/iot/remote/status";

        /// <summary>
        /// Enable Remote Settings API.
        /// </summary>
        public static readonly string RemoteSettingsEnableApi = "api/iot/remote/enable";

        /// <summary>
        /// Disable Remote Settings API.
        /// </summary>
        public static readonly string RemoteSettingsDisableApi = "api/iot/remote/disable";

        /// <summary>
        /// Gets the Remote Settings Status Information.
        /// </summary>
        /// <returns>String containing the Remote Settings Status information.</returns>
        public async Task<RemoteSettingsStatusInfo> GetRemoteSettingsStatusInfoAsync()
        {
            return await this.GetAsync<RemoteSettingsStatusInfo>(RemoteSettingsStatusApi);
        }

        /// <summary>
        /// Enables the remote settings.
        /// </summary>
        /// <returns>Task tracking completion of the REST call.</returns>
        public async Task<RemoteSettingsStatusInfo> RemoteSettingsEnableAsync()
        {
            return await this.PostAsync<RemoteSettingsStatusInfo>(
                RemoteSettingsEnableApi);
        }

        /// <summary>
        /// Disables the remote settings.
        /// </summary>
        /// <returns>Task tracking completion of the REST call.</returns>
        public async Task<RemoteSettingsStatusInfo> RemoteSettingsDisableAsync()
        {
            return await this.PostAsync<RemoteSettingsStatusInfo>(
                RemoteSettingsDisableApi);
        }

        #region Data contract

        /// <summary>
        /// Remote Settings Status information.
        /// </summary>
        [DataContract]
        public class RemoteSettingsStatusInfo
        {
            /// <summary>
            /// Gets a value indicating whether the service is running.
            /// </summary>
            [DataMember(Name = "IsRunning")]
            public bool IsRunning { get; private set; }

            /// <summary>
            ///  Gets a value indicating whether the service is scheduled.
            /// </summary>
            [DataMember(Name = "IsScheduled")]
            public bool IsScheduled { get; private set; }
        }
        #endregion // Data contract
    }
}
