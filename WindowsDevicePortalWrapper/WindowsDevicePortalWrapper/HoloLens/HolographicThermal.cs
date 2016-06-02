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
        private static readonly String _thermalStageApi = "api/holographic/thermal/stage";

        /// <summary>
        /// Gets the curent thermal stage reading from the device.
        /// </summary>
        /// <returns>ThermalStages enum value.</returns>
        /// <remarks>This method is only supported on HoloLens devices.</remarks>
        public async Task<ThermalStages> GetThermalStage()
        {
            if (Platform != DevicePortalPlatforms.HoloLens)
            {
                throw new NotSupportedException("This method is only supported on HoloLens.");
            }

            ThermalStage thermalStage = await Get<ThermalStage>(_thermalStageApi);
            return thermalStage.Stage;
        }
    }

#region Data contract

    public enum ThermalStages
    {
        Normal,
        Warm,
        Critical,
        Unknown = 9999
    }

    [DataContract]
    public class ThermalStage
    {
        [DataMember(Name="CurrentStage")]
        public Int32 StageRaw { get; set; }

        public ThermalStages Stage
        {
            get 
            {
                ThermalStages stage = ThermalStages.Unknown;

                try
                {
                    stage = (ThermalStages)Enum.ToObject(typeof(ThermalStages), StageRaw);
                    
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
