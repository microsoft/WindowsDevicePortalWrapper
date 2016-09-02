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
       
        public static readonly string machineIdApi = "api/iot/telemetry/sqmmachineid";

    

        public async Task<TelemetryInfo> GetTelemetryInfo()
        {
            return await this.Get<TelemetryInfo>(machineIdApi);
        }

        #region Data contract


        [DataContract]
        public class TelemetryInfo
        {
            [DataMember(Name = "SqmMachineId")]
            public string machineId { get; set; }

          
        }
      
     
       
                  #endregion // Data contract
    }
}
