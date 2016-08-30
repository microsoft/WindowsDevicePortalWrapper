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
        /// Gets the Remote Settings Status Information.
        /// </summary>
        /// <returns>String containing the Remote Settings Status information.</returns>
        public async Task<RemoteSettingsStatusInfo> GetRemoteSettingsStatusInfo()
        {
            return await this.Get<RemoteSettingsStatusInfo>(RemoteSettingsStatusApi);
        }

        #region Data contract

        /// <summary>
        /// Remote Settings Status information.
        /// </summary>
        [DataContract]
        public class RemoteSettingsStatusInfo
        {
            /// <summary>
            /// Gets or sets a value indicating whether the service is running.
            /// </summary>
            [DataMember(Name = "IsRunning")]
            public bool IsRunning { get; set; }

            /// <summary>
            ///  Gets or sets a value indicating whether the service is scheduled.
            /// </summary>
            [DataMember(Name = "IsScheduled")]
            public bool IsScheduled { get; set; }
        }
        #endregion // Data contract
    }
}
