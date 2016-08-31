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

    public partial class DevicePortal
    {
        public static readonly string IcsInterfacesApi = "api/iot/ics/interfaces";
        public static readonly string IcSharingApi = "api/iot/ics/sharing";
       

        /// <summary>
        /// Gets the Remote Settings Status Information.
        /// </summary>
        /// <returns>String containing the Remote Settings Status information.</returns>
        public async Task<RemoteSettingsStatusInfo> GetIcsInterfacesInfo()
        {
            return await this.Get<RemoteSettingsStatusInfo>(IcsInterfacesApi);
        }

        /// <summary>
        /// Enables the remote settings.
        /// </summary>
        public async Task<RemoteSettingsStatusInfo> IcSharing()
        {
            return await this.Post<RemoteSettingsStatusInfo>(
                IcSharingApi);
        }

       
        #region Data contract

        
        #endregion // Data contract
    }
}
