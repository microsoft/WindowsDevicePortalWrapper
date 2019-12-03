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
            /// HoloLens 2 platform
            /// </summary>
            HoloLens2,

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

            /// <summary>
            /// A virtual machine. This may or may not be an emulator.
            /// </summary>
            VirtualMachine
        }

        /// <summary>
        /// Gets the family name (ex: Windows.Holographic) of the device.
        /// </summary>
        /// <returns>String containing the device's family.</returns>
        public async Task<string> GetDeviceFamilyAsync()
        {
            DeviceOsFamily deviceFamily = await this.GetAsync<DeviceOsFamily>(DeviceFamilyApi).ConfigureAwait(false);
            return deviceFamily.Family;
        }

        /// <summary>
        /// Gets the name of the device.
        /// </summary>
        /// <returns>String containing the device's name.</returns>
        public async Task<string> GetDeviceNameAsync()
        {
            DeviceName deviceName = await this.GetAsync<DeviceName>(MachineNameApi);
            return deviceName.Name;
        }

        /// <summary>
        /// Gets information about the device's operating system.
        /// </summary>
        /// <returns>OperatingSystemInformation object containing details of the installed operating system.</returns>
        public Task<OperatingSystemInformation> GetOperatingSystemInformationAsync()
        {
            return this.GetAsync<OperatingSystemInformation>(OsInfoApi);
        }

        /// <summary>
        /// Sets the device's name
        /// </summary>
        /// <param name="name">The name to assign to the device.</param>
        /// <remarks>The new name does not take effect until the device has been restarted.</remarks>
        /// <returns>Task tracking setting the device name completion.</returns>
        public Task SetDeviceNameAsync(string name)
        {
            return this.PostAsync(
                MachineNameApi,
                string.Format("name={0}", Utilities.Hex64Encode(name)));
        }

        #region Data contract

        /// <summary>
        /// Device name object.
        /// </summary>
        [DataContract]
        public class DeviceName
        {
            /// <summary>
            /// Gets the name.
            /// </summary>
            [DataMember(Name = "ComputerName")]
            public string Name { get; private set; }
        }

        /// <summary>
        /// Device family object.
        /// </summary>
        [DataContract]
        public class DeviceOsFamily
        {
            /// <summary>
            /// Gets the device family name.
            /// </summary>
            [DataMember(Name = "DeviceType")]
            public string Family { get; private set; }
        }

        /// <summary>
        /// Operating system information.
        /// </summary>
        [DataContract]
        public class OperatingSystemInformation
        {
            /// <summary>
            ///  Gets the OS name.
            /// </summary>
            [DataMember(Name = "ComputerName")]
            public string Name { get; private set; }

            /// <summary>
            /// Gets the language
            /// </summary>
            [DataMember(Name = "Language")]
            public string Language { get; private set; }

            /// <summary>
            /// Gets the edition
            /// </summary>
            [DataMember(Name = "OsEdition")]
            public string OsEdition { get; private set; }

            /// <summary>
            /// Gets the edition Id
            /// </summary>
            [DataMember(Name = "OsEditionId")]
            public uint OsEditionId { get; private set; }

            /// <summary>
            /// Gets the OS version
            /// </summary>
            [DataMember(Name = "OsVersion")]
            public string OsVersionString { get; private set; }

            /// <summary>
            /// Gets the raw platform type
            /// </summary>
            [DataMember(Name = "Platform")]
            public string PlatformName { get; private set; }

            /// <summary>
            /// Gets the platform
            /// </summary>
            public DevicePortalPlatforms Platform
            {
                get
                {
                    try
                    {
                        // MinnowBoard Max model no. can change based on firmware
                        if (this.PlatformName.Contains("Minnowboard Max"))
                        {
                            return DevicePortalPlatforms.IoTMinnowboardMax;
                        }

                        // Xbox One platform names may refer to devkit
                        if (this.PlatformName.Contains("Xbox One"))
                        {
                            return DevicePortalPlatforms.XboxOne;
                        }

                        switch (this.PlatformName)
                        {
                            case "SBC":
                                return DevicePortalPlatforms.IoTDragonboard410c;

                            case "Raspberry Pi 2":
                                return DevicePortalPlatforms.IoTRaspberryPi2;

                            case "Raspberry Pi 3":
                                return DevicePortalPlatforms.IoTRaspberryPi3;

                            case "Virtual Machine":
                                return DevicePortalPlatforms.VirtualMachine;

                            case "HoloLens 2":
                                return DevicePortalPlatforms.HoloLens2;

                            default:
                                return (DevicePortalPlatforms)Enum.Parse(typeof(DevicePortalPlatforms), this.PlatformName);
                        }
                    }
                    catch
                    {
                        switch (this.OsEdition)
                        {
                            case "Enterprise":
                            case "Home":
                            case "Professional":
                                return DevicePortalPlatforms.Windows;

                            case "Mobile":
                                return DevicePortalPlatforms.Mobile;

                            default:
                                return DevicePortalPlatforms.Unknown;
                        }
                    }
                }
            }
        }
        #endregion // Data contract
    }
}
