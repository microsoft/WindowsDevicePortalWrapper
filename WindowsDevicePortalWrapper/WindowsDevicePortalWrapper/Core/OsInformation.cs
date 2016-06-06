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
        private static readonly String _machineNameApi = "api/os/machinename";
        private static readonly String _osInfoApi = "api/os/info";

        /// <summary>
        /// Gets the name of the device.
        /// </summary>
        /// <returns>String containing the device's name.</returns>
        public async Task<String> GetDeviceName()
        {
            DeviceName deviceName = await Get<DeviceName>(_machineNameApi);
            return deviceName.Name;
        }


        /// <summary>
        /// Gets information about the device's operating system.
        /// </summary>
        /// <returns>OperatingSystemInformation object containing details of the installed operating system.</returns>
        public async Task<OperatingSystemInformation> GetOperatingSystemInformation()
        {
            return await Get<OperatingSystemInformation>(_osInfoApi);
        }

        /// <summary>
        /// Sets the device's name
        /// </summary>
        /// <param name="name">The name to assign to the device.</param>
        /// <param name="reboot">True to reboot the device after setting the name, false otherwise.</param>
        /// <remarks>The new name does not take effect until the device has been restarted.</remarks>
        public async Task SetDeviceName(String name, 
                                        Boolean reboot = true)
        {
            await Post(_machineNameApi,
                                String.Format("name={0}", Utilities.Hex64Encode(name)));

            // Names do not take effect until after a reboot.
            if (reboot)
            {
                await Reboot();
            }

            // TODO - wait until device has rebooted, then update the device name (osinfo and qualified name too?)
        }
    }

#region Data contract

    [DataContract]
    public class DeviceName
    {
        [DataMember(Name="ComputerName")]
        public String Name { get; set; }
    }

    [DataContract]
    public class OperatingSystemInformation
    {
        [DataMember(Name="ComputerName")]
        public String Name { get; set; }

        [DataMember(Name="Language")]
        public String Languasge { get; set; }

        [DataMember(Name="OsEdition")]
        public String OsEdition { get; set; }

        [DataMember(Name="OsEditionId")]
        public UInt32 OsEditionId { get; set; }

        [DataMember(Name="OsVersion")]
        public String OsVersionString { get; set; }

        [DataMember(Name="Platform")]       
        public String PlatformRaw { get; set; }

        public DevicePortalPlatforms Platform
        {
            get
            {
                DevicePortalPlatforms platform = DevicePortalPlatforms.Unknown;

                try
                {
                    switch(PlatformRaw)
                    {
                        case "Xbox One":
                            platform = DevicePortalPlatforms.XboxOne;
                            break;

                        default:
                            platform = (DevicePortalPlatforms)Enum.Parse(typeof(DevicePortalPlatforms), PlatformRaw);
                            break;                        
                    }
                }
                catch
                {}

                return platform;
            }
        }
    }

    public enum DevicePortalPlatforms
    {
        Unknown = -1,

        // TODO are these the correct names?
        Windows = 0,
        Mobile,
        IoT,
        HoloLens,
        XboxOne
    }
#endregion // Data contract
}
