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
        public static readonly string SleepStudyTransformApi = "api/power/sleepstudy/transform";

        /// <summary>
        /// Returns the current active power scheme.
        /// </summary>
        /// <returns>The power scheme identifier.</returns>
        public async Task<Guid> GetActivePowerSchemeAsync()
        {
            ActivePowerScheme activeScheme = await this.GetAsync<ActivePowerScheme>(ActivePowerSchemeApi);
            return activeScheme.Id;
        }

        /// <summary>
        /// Returns the current state of the device's battery.
        /// </summary>
        /// <returns>BatteryState object containing details such as the current battery level.</returns>
        public async Task<BatteryState> GetBatteryStateAsync()
        {
            return await this.GetAsync<BatteryState>(BatteryStateApi);
        }

        /// <summary>
        /// Gets the device's current power state.
        /// </summary>
        /// <returns>PowerState object containing details such as whether or not the device is in low power mode.</returns>
        public async Task<PowerState> GetPowerStateAsync()
        {
            return await this.GetAsync<PowerState>(PowerStateApi);
        }

        #region Data contract

        /// <summary>
        /// Battery state.
        /// </summary>
        [DataContract]
        public class ActivePowerScheme
        {
            /// <summary>
            /// Gets the active power scheme identifier.
            /// </summary>
            [DataMember(Name = "ActivePowerScheme")]
            public Guid Id { get; private set; }
        }

        /// <summary>
        /// Battery state.
        /// </summary>
        [DataContract]
        public class BatteryState
        {
            /// <summary>
            /// Gets a value indicating whether the device is on AC power.
            /// </summary>
            [DataMember(Name = "AcOnline")]
            public bool IsOnAcPower { get; private set; }

            /// <summary>
            /// Gets a value indicating whether a battery is present.
            /// </summary>
            [DataMember(Name = "BatteryPresent")]
            public bool IsBatteryPresent { get; private set; }

            /// <summary>
            /// Gets a value indicating whether the device is charging.
            /// </summary>
            [DataMember(Name = "Charging")]
            public bool IsCharging { get; private set; }

            /// <summary>
            /// Gets the default alert.
            /// </summary>
            [DataMember(Name = "DefaultAlert1")]
            public int DefaultAlert1 { get; private set; }
            
            /// <summary>
            /// Gets the default alert.
            /// </summary>
            [DataMember(Name = "DefaultAlert2")]
            public int DefaultAlert2 { get; private set; }

            /// <summary>
            /// Gets estimated battery time left in seconds.
            /// </summary>
            [DataMember(Name = "EstimatedTime")]
            public uint EstimatedTimeRaw { get; private set; }

            /// <summary>
            /// Gets maximum capacity.
            /// </summary>
            [DataMember(Name = "MaximumCapacity")]
            public int MaximumCapacity { get; private set; }

            /// <summary>
            /// Gets remaining capacity.
            /// </summary>
            [DataMember(Name = "RemainingCapacity")]
            public int RemainingCapacity { get; private set; }

            /// <summary>
            /// Gets the battery level as a percentage of the maximum capacity.
            /// </summary>
            public float Level
            {
                get 
                { 
                    // Desktop PCs typically do not have a battery, return 100%
                    if (this.MaximumCapacity == 0)
                    {
                        return 100f;
                    }

                    return 100.0f * ((float)this.RemainingCapacity / this.MaximumCapacity);
                }
            }

            /// <summary>
            /// Gets the remaining battery time left, as a TimeSpan. 
            /// Will be 0 if the device has no battery. 
            /// Will be 0xFFFF,FFFF (around 138 years) if the device is charging. 
            /// </summary>
            public TimeSpan EstimatedTime
            {
                get
                {
                    return new TimeSpan(0, 0, (int)this.EstimatedTimeRaw);
                }
            }
        }

        /// <summary>
        /// Power state
        /// </summary>
        [DataContract]
        public class PowerState
        {
            /// <summary>
            /// Gets a value indicating whether the device is in a lower power mode
            /// </summary>
            [DataMember(Name = "LowPowerState")]
            public bool InLowPowerState { get; private set; }

            /// <summary>
            /// Gets a value indicating whether the device supports a lower power mode
            /// </summary>
            [DataMember(Name = "LowPowerStateAvailable")]
            public bool IsLowPowerStateAvailable { get; private set; }
        }

        #endregion // Data contract
    }
}
