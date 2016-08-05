//----------------------------------------------------------------------------------------------
// <copyright file="Power.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <content>
    /// Wrappers for Power methods.
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// API for getting or setting the active power scheme.
        /// </summary>
        public static readonly string ActivePowerSchemeApi = "api/power/activecfg";

        /// <summary>
        /// API for getting battery state.
        /// </summary>
        public static readonly string BatteryStateApi = "api/power/battery";

        /// <summary>
        /// API for getting or setting a power scheme's sub-value.
        /// </summary>
        public static readonly string PowerSchemeSubValueApi = "api/power/cfg";

        /// <summary>
        /// API for controlling power state.
        /// </summary>
        public static readonly string PowerStateApi = "api/power/state";

        /// <summary>
        /// API for getting a sleep study report.
        /// </summary>
        public static readonly string SleepStudyReportApi = "api/power/sleepstudy/report";

        /// <summary>
        /// API for getting the list of sleep study reports.
        /// </summary>
        public static readonly string SleepStudyReportsApi = "api/power/sleepstudy/reports";

        /// <summary>
        /// API for getting a sleep study report.
        /// </summary>
        private static readonly string SleepStudyTransformApi = "api/power/sleepstudy/transform";

        /// <summary>
        /// Returns the current active power scheme.
        /// </summary>
        /// <returns>The power scheme identifier.</returns>
        public async Task<Guid> GetActivePowerScheme()
        {
            ActivePowerScheme activeScheme = await this.Get<ActivePowerScheme>(ActivePowerSchemeApi);
            return activeScheme.Id;
        }

        /// <summary>
        /// Returns the current state of the device's battery.
        /// </summary>
        /// <returns>BatteryState object containing details such as the current battery level.</returns>
        public async Task<BatteryState> GetBatteryState()
        {
            return await this.Get<BatteryState>(BatteryStateApi);
        }

        /// <summary>
        /// Gets the device's current power state.
        /// </summary>
        /// <returns>PowerState object containing details such as whether or not the device is in low power mode.</returns>
        public async Task<PowerState> GetPowerState()
        {
            return await this.Get<PowerState>(PowerStateApi);
        }

        #region Data contract

        /// <summary>
        /// Battery state.
        /// </summary>
        [DataContract]
        public class ActivePowerScheme
        {
            /// <summary>
            /// Gets or sets the active power scheme identifier.
            /// </summary>
            [DataMember(Name = "ActivePowerScheme")]
            public Guid Id { get; set; }
        }

        /// <summary>
        /// Battery state.
        /// </summary>
        [DataContract]
        public class BatteryState
        {
            /// <summary>
            /// Gets or sets a value indicating whether the device is on AC power.
            /// </summary>
            [DataMember(Name = "AcOnline")]
            public bool IsOnAcPower { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether a battery is present.
            /// </summary>
            [DataMember(Name = "BatteryPresent")]
            public bool IsBatteryPresent { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the device is charging.
            /// </summary>
            [DataMember(Name = "Charging")]
            public bool IsCharging { get; set; }

            /// <summary>
            /// Gets or sets the default alert.
            /// </summary>
            [DataMember(Name = "DefaultAlert1")]
            public int DefaultAlert1 { get; set; }
            
            /// <summary>
            /// Gets or sets the default alert.
            /// </summary>
            [DataMember(Name = "DefaultAlert2")]
            public int DefaultAlert2 { get; set; }

            /// <summary>
            /// Gets or sets estimated battery time.
            /// </summary>
            [DataMember(Name = "EstimatedTime")]
            public uint EstimatedTimeRaw { get; set; }

            /// <summary>
            /// Gets or sets maximum capacity.
            /// </summary>
            [DataMember(Name = "MaximumCapacity")]
            public int MaximumCapacity { get; set; }

            /// <summary>
            /// Gets or sets remaining capacity.
            /// </summary>
            [DataMember(Name = "RemainingCapacity")]
            public int RemainingCapacity { get; set; }

            /// <summary>
            /// Gets the battery level as a percentage of the maximum capacity.
            /// </summary>
            public float Level
            {
                get { return 100.0f * ((float)this.RemainingCapacity / this.MaximumCapacity); }
            }
        }

        /// <summary>
        /// Power state
        /// </summary>
        [DataContract]
        public class PowerState
        {
            /// <summary>
            /// Gets or sets a value indicating whether the device is in a lower power mode
            /// </summary>
            [DataMember(Name = "LowPowerState")]
            public bool InLowPowerState { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the device supports a lower power mode
            /// </summary>
            [DataMember(Name = "LowPowerStateAvailable")]
            public bool IsLowPowerStateAvailable { get; set; }
        }

        #endregion // Data contract
    }
}
