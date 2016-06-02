// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE.TXT in the project root license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal
{
    public partial class DevicePortal
    {
        private static readonly String _batteryStateApi = "api/power/battery";
        private static readonly String _powerStateApi = "api/power/state";

        /// <summary>
        /// Returns the current state of the device's battery.
        /// </summary>
        /// <returns>BatteryState object containing details such as the current battery level.</returns>
        public async Task<BatteryState> GetBatteryState()
        {
            return await Get<BatteryState>(_batteryStateApi);
        }

        /// <summary>
        /// Gets the device's current power state.
        /// </summary>
        /// <returns>PowerState object containing details such as whether or not the device is in low power mode.</returns>
        public async Task<PowerState> GetPowerState()
        {
            return await Get<PowerState>(_powerStateApi);
        }
    }

#region Data contract

    [DataContract]
    public class BatteryState
    {
        [DataMember(Name="AcOnline")]
        public Boolean IsOnAcPower { get; set; }

        [DataMember(Name="BatteryPresent")]
        public Boolean IsBatteryPresent { get; set; }

        [DataMember(Name="Charging")]
        public Boolean IsCharging { get; set; }

        [DataMember(Name="DefaultAlert1")]
        public Int32 DefaultAlert1 { get; set; }

        [DataMember(Name="DefaultAlert2")]
        public Int32 DefaultAlert2 { get; set; }

        [DataMember(Name="EstimatedTime")]
        public UInt32 EstimatedTimeRaw { get; set; }

        [DataMember(Name="MaximumCapacity")]
        public UInt32 MaximumCapacity { get; set; }

        [DataMember(Name="RemainingCapacity")]
        public Int32 RemainingCapacity { get; set; }

        /// <summary>
        /// Returns the battery level as a percentage of the maximum capacity.
        /// </summary>
        /// <returns>
        /// Current battery level.
        /// </returns>
        public Single Level
        {
            get { return 100.0f * ((Single)RemainingCapacity/(Single)MaximumCapacity); }
        }
    }

    [DataContract]
    public class PowerState
    {
        [DataMember(Name="LowPowerState")]
        public Boolean InLowPowerState { get; set; }

        [DataMember(Name="LowPowerStateAvailable")]
        public Boolean IsLowPowerStateAvailable { get; set; }
    }

#endregion // Data contract
}
