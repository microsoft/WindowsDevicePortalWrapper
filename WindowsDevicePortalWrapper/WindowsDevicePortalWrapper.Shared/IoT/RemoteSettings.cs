﻿//----------------------------------------------------------------------------------------------
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
        public static readonly string RemoteSettingsEnableApi = "api/iot/remote/enable";
        public static readonly string RemoteSettingsDisableApi = "api/iot/remote/disable";

        /// <summary>
        /// Gets the Remote Settings Status Information.
        /// </summary>
        /// <returns>String containing the Remote Settings Status information.</returns>
        public async Task<RemoteSettingsStatusInfo> GetRemoteSettingsStatusInfo()
        {
            return await this.Get<RemoteSettingsStatusInfo>(RemoteSettingsStatusApi);
        }

        /// <summary>
        /// Enables the remote settings.
        /// </summary>
        public async Task<RemoteSettingsStatusInfo> RemoteSettingsEnable()
        {
            return await this.Post<RemoteSettingsStatusInfo>(
                RemoteSettingsEnableApi);
        }

        /// <summary>
        /// Disables the remote settings.
        /// </summary>
        public async Task<RemoteSettingsStatusInfo> RemoteSettingsDisable()
        {
            return await this.Post<RemoteSettingsStatusInfo>(
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
