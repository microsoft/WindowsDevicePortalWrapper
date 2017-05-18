//----------------------------------------------------------------------------------------------
// <copyright file="HolographicThermal.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <summary>
    /// Thermal State enumeration
    /// </summary>
    public enum ThermalStages
    {
        /// <summary>
        /// No thermal stage
        /// </summary>
        Normal,

        /// <summary>
        /// Warm stage
        /// </summary>
        Warm,

        /// <summary>
        /// Critical stage
        /// </summary>
        Critical,

        /// <summary>
        /// Unknown stage
        /// </summary>
        Unknown = 9999
    }

    /// <content>
    /// Wrappers for Holographic Thermal methods
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// API for getting the thermal stage
        /// </summary>
        public static readonly string ThermalStageApi = "api/holographic/thermal/stage";

        /// <summary>
        /// Gets the current thermal stage reading from the device.
        /// </summary>
        /// <returns>ThermalStages enum value.</returns>
        /// <remarks>This method is only supported on HoloLens.</remarks>
        public async Task<ThermalStages> GetThermalStageAsync()
        {
            if (!Utilities.IsHoloLens(this.Platform, this.DeviceFamily))
            {
                throw new NotSupportedException("This method is only supported on HoloLens.");
            }

            ThermalStage thermalStage = await this.GetAsync<ThermalStage>(ThermalStageApi);
            return thermalStage.Stage;
        }

        #region Data contract

        /// <summary>
        /// Object representation of thermal stage
        /// </summary>
        [DataContract]
        public class ThermalStage
        {
            /// <summary>
            /// Gets the raw stage value
            /// </summary>
            [DataMember(Name = "CurrentStage")]
            public int StageRaw { get; private set; }

            /// <summary>
            /// Gets the enumeration value of the thermal stage
            /// </summary>
            public ThermalStages Stage
            {
                get
                {
                    ThermalStages stage = ThermalStages.Unknown;

                    try
                    {
                        stage = (ThermalStages)Enum.ToObject(typeof(ThermalStages), this.StageRaw);

                        if (!Enum.IsDefined(typeof(ThermalStages), stage))
                        {
                            stage = ThermalStages.Unknown;
                        }
                    }
                    catch
                    {
                        stage = ThermalStages.Unknown;
                    }

                    return stage;
                }
            }
        }

        #endregion // Data contract
    }
}
