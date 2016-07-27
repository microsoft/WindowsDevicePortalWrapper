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
    /// <content>
    /// Wrappers for OS Information.
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// API for getting the device family.
        /// </summary>
        public static readonly string DeviceFamilyApi = "api/os/devicefamily";

        /// <summary>
        /// API for getting the machine name.
        /// </summary>
        public static readonly string MachineNameApi = "api/os/machinename";

        /// <summary>
        /// API for getting the OS information.
        /// </summary>
        public static readonly string OsInfoApi = "api/os/info";

        /// <summary>
        /// Device portal platforms
        /// </summary>
        public enum DevicePortalPlatforms
        {
            /// <summary>
            /// Unknown platform
            /// </summary>
            Unknown = -1,

            /// <summary>
            /// Windows platform
            /// </summary>
            Windows = 0,

            /// <summary>
            /// Mobile platform
            /// </summary>
            Mobile,

            /// <summary>
            /// HoloLens platform
            /// </summary>
            HoloLens,

            /// <summary>
            /// Xbox One platform
            /// </summary>
            XboxOne,

            /// <summary>
            /// Windows IoT on Dragonboard 410c
            /// </summary>
            IoTDragonboard410c,

            /// <summary>
            /// Windows IoT on Minnowboard Max
            /// </summary>
            IoTMinnowboardMax,

            /// <summary>
            /// Windows IoT on Raspberry Pi 2
            /// </summary>
            IoTRaspberryPi2,

            /// <summary>
            /// Windows IoT on Raspberry Pi 3
            /// </summary>
            IoTRaspberryPi3,
        }

        /// <summary>
        /// Gets the family name (ex: Windows.Holographic) of the device.
        /// </summary>
        /// <returns>String containing the device's family.</returns>
        public async Task<string> GetDeviceFamily()
        {
            DeviceOsFamily deviceFamily = await this.Get<DeviceOsFamily>(DeviceFamilyApi);
            return deviceFamily.Family;
        }

        /// <summary>
        /// Gets the name of the device.
        /// </summary>
        /// <returns>String containing the device's name.</returns>
        public async Task<string> GetDeviceName()
        {
            DeviceName deviceName = await this.Get<DeviceName>(MachineNameApi);
            return deviceName.Name;
        }

        /// <summary>
        /// Gets information about the device's operating system.
        /// </summary>
        /// <returns>OperatingSystemInformation object containing details of the installed operating system.</returns>
        public async Task<OperatingSystemInformation> GetOperatingSystemInformation()
        {
            return await this.Get<OperatingSystemInformation>(OsInfoApi);
        }

        /// <summary>
        /// Sets the device's name
        /// </summary>
        /// <param name="name">The name to assign to the device.</param>
        /// <param name="reboot">True to reboot the device after setting the name, false otherwise.</param>
        /// <remarks>The new name does not take effect until the device has been restarted.</remarks>
        /// <returns>Task tracking setting the device name completion.</returns>
        public async Task SetDeviceName(
            string name,
            bool reboot = true)
        {
            await this.Post(
                MachineNameApi,
                string.Format("name={0}", Utilities.Hex64Encode(name)));

            // Names do not take effect until after a reboot.
            if (reboot)
            {
                await this.Reboot();
            }

            // TODO - wait until device has rebooted then reconnect
        }

        #region Data contract

        /// <summary>
        /// Device name object.
        /// </summary>
        [DataContract]
        public class DeviceName
        {
            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            [DataMember(Name = "ComputerName")]
            public string Name { get; set; }
        }

        /// <summary>
        /// Device family object.
        /// </summary>
        [DataContract]
        public class DeviceOsFamily
        {
            /// <summary>
            /// Gets or sets the device family name.
            /// </summary>
            [DataMember(Name = "DeviceType")]
            public string Family { get; set; }
        }

        /// <summary>
        /// Operating system information.
        /// </summary>
        [DataContract]
        public class OperatingSystemInformation
        {
            /// <summary>
            ///  Gets or sets the OS name.
            /// </summary>
            [DataMember(Name = "ComputerName")]
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the language
            /// </summary>
            [DataMember(Name = "Language")]
            public string Language { get; set; }

            /// <summary>
            /// Gets or sets the edition
            /// </summary>
            [DataMember(Name = "OsEdition")]
            public string OsEdition { get; set; }

            /// <summary>
            /// Gets or sets the edition Id
            /// </summary>
            [DataMember(Name = "OsEditionId")]
            public uint OsEditionId { get; set; }

            /// <summary>
            /// Gets or sets the OS version
            /// </summary>
            [DataMember(Name = "OsVersion")]
            public string OsVersionString { get; set; }

            /// <summary>
            /// Gets or sets the raw platform type
            /// </summary>
            [DataMember(Name = "Platform")]
            public string PlatformName { get; set; }

            /// <summary>
            /// Gets the platform
            /// </summary>
            public DevicePortalPlatforms Platform
            {
                get
                {
                    DevicePortalPlatforms platform = DevicePortalPlatforms.Unknown;

                    try
                    {
                        switch (this.PlatformName)
                        {
                            case "Xbox One":
                                platform = DevicePortalPlatforms.XboxOne;
                                break;

                            case "Dragonboard 401c":
                                platform = DevicePortalPlatforms.IoTDragonboard410c;
                                break;

                            case "Minnowboard Max":
                                platform = DevicePortalPlatforms.IoTMinnowboardMax;
                                break;

                            case "Raspberry Pi 2":
                                platform = DevicePortalPlatforms.IoTRaspberryPi2;
                                break;

                            case "Raspberry Pi 3":
                                platform = DevicePortalPlatforms.IoTRaspberryPi3;
                                break;

                            default:
                                platform = (DevicePortalPlatforms)Enum.Parse(typeof(DevicePortalPlatforms), this.PlatformName);
                                break;
                        }
                    }
                    catch
                    {
                    }

                    return platform;
                }
            }
        }
        #endregion // Data contract
    }
}
