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

        public static readonly string RemoteSettingsStatusApi = "api/iot/remote/status";
        public static readonly string RemoteSettingsEnableApi = "api/iot/remote/enable";
        public static readonly string RemoteSettingsDisableApi = "api/iot/remote/disable";


        public async Task<RemoteSettingsStatusInfo> GetRemoteSettingsStatusInfo()
        {
            return await this.Get<RemoteSettingsStatusInfo>(RemoteSettingsStatusApi);
        }
        #region Data contract

        [DataContract]
        public class RemoteSettingsStatusInfo
        {
            [DataMember(Name = "IsRunning")]
            public bool IsRunning;

            [DataMember(Name = "IsScheduled")]
            public bool IsScheduled;



        }
        #endregion // Data contract
    }
}
