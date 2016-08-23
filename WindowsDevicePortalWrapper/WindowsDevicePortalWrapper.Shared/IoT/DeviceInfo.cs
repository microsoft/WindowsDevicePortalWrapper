//----------------------------------------------------------------------------------------------
// <copyright file="DeviceInfo.cs" company="Microsoft Corporation">
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
    /// Wrappers for Device Information.
    /// </content>
    public partial class DevicePortal
    {
        

        public static readonly string IoTOsInfoApi = "api/iot/device/information";
        public static readonly string TimezoneInfoApi = "api/iot/device/timezones";
        public static readonly string SetTimezoneInfoApi = "api/iot/device/settimezone";
        public static readonly string DateTimeInfoApi = "api/iot/device/datetime";
        public static readonly string ControllerDriverApi = "api/iot/device/controllersdriver";
        public static readonly string DisplayResolutionApi = "api/iot/device/displayresolution";
        public static readonly string DisplayOrientationApi = "api/iot/device/displayorientation";
        public static readonly string RemoteDebuggingPinApi = "api/iot/device/remotedebuggingpin";
        public static readonly string DeviceNameApi = "api/iot/device/name";

        /// <summary>
        /// Gets the IoT OS Information.
        /// </summary>
        /// <returns>String containing the OS information.</returns>
        public async Task<IoTOSInfo> GetIoTOSInfo()
        {
            var info = await this.Get<IoTOSInfo>(IoTOsInfoApi);
            return info;
        }
        /// <summary>
        /// Gets the Timezone information.
        /// </summary>
        /// <returns>String containing the timezone information.</returns>
        public async Task<TimezoneInfo> GetTimezoneInfo()
        {
            var info = await this.Get<TimezoneInfo>(TimezoneInfoApi);
            return info;
        }
        /// <summary>
        /// Gets the datetime information.
        /// </summary>
        /// <returns>String containing the datetime information.</returns>
        public async Task<DateTimeInfo> GetDateTimeInfo()
        {
            return await this.Get<DateTimeInfo>(DateTimeInfoApi);
            
        }
        /// <summary>
        /// Gets the controller driver information.
        /// </summary>
        /// <returns>String containing the controller driver information.</returns>
        public async Task<controllerDriverInfo> GetControllerDriverInfo()
        {
            return await this.Get<controllerDriverInfo>(ControllerDriverApi);
           
        }

        /// <summary>
        /// Gets the dispaly orientation information.
        /// </summary>
        /// <returns>String containing the dispaly orientation information.</returns>

        public async Task<DisplayOrientationInfo> GetDisplayOrientationInfo()
        {
            return await this.Get<DisplayOrientationInfo>(DisplayOrientationApi);
            
        }
        /// <summary>
        /// Gets the dispaly resolution information.
        /// </summary>
        /// <returns>String containing the dispaly resolution information.</returns>

        public async Task<DisplayResolutionInfo> GetDisplayResolutionInfo()
        {
            return await this.Get<DisplayResolutionInfo>(DisplayResolutionApi);
            
        }
        /// <summary>
        /// Sets the Device Name.
        /// </summary>

        public async Task SetIoTDeviceName(string name)
        {
       
            await this.Post(DeviceNameApi, string.Format("newdevicename={0}", Utilities.Hex64Encode(name)));
        }

        #region Data contract
        /// <summary>
        /// Operating system information.
        /// </summary>
        [DataContract]
        public class IoTOSInfo
        {
            /// <summary>
            /// Gets the device model
            /// </summary>
            [DataMember(Name = "DeviceModel")]
            public string Model { get; set; }

            /// <summary>
            ///  Gets or sets the device name.
            /// </summary>
            [DataMember(Name = "DeviceName")]
            public string Name { get; set; }

            /// <summary>
            /// Gets the OS version
            /// </summary>
            [DataMember(Name = "OSVersion")]
            public string OSVersion { get; set; }
        }
        /// <summary>
        ///Timezone information.
        /// </summary>
        [DataContract]
       
        public class TimezoneInfo
        {
            /// <summary>
            /// Gets the current timezone
            /// </summary>
            [DataMember(Name = "Current")]
            public Timezones Current;
            /// <summary>
            /// Gets the list of all timezones
            /// </summary>
            [DataMember(Name = "Timezones")]
            public Timezones[] Timezones;

        }
        
        /// <summary>
        ///Timezone information.
        /// </summary>
        [DataContract]
        public partial class Timezones
        {
            /// <summary>
            /// Gets the timezone description
            /// </summary>
            [DataMember(Name = "Description")]
            public string description { get; set; }

            /// <summary>
            /// Gets the timezone index
            /// </summary>
            [DataMember(Name = "Index")]
            public int index { get; set; }

            /// <summary>
            /// Gets the timezone name
            /// </summary>
            [DataMember(Name = "Name")]
            public string name { get; set; }
            
        }
        /// <summary>
        ///DateTime information.
        /// </summary>
        [DataContract]
        public class DateTimeInfo
        {
            [DataMember(Name = "Current")]
            public Current Current;
        }
        /// <summary>
        ///Current Datetime information.
        /// </summary>
        [DataContract]
        public partial class Current
        {
            /// <summary>
            /// Gets the current day information
            /// </summary>
            [DataMember(Name = "Day")]
            public int day { get; set; }

            /// <summary>
            /// Gets the current hour information
            /// </summary>
            [DataMember(Name = "Hour")]
            public int hour { get; set; }

            /// <summary>
            /// Gets the current minute information
            /// </summary>
            [DataMember(Name = "Minute")]
            public int min { get; set; }

            /// <summary>
            /// Gets the current month information
            /// </summary>
            [DataMember(Name = "Month")]
            public int month { get; set; }

            /// <summary>
            /// Gets the current second information
            /// </summary>
            [DataMember(Name ="Second")]
            public int sec { get; set; }

            /// <summary>
            /// Gets the current year information
            /// </summary>
            [DataMember(Name = "Year")]
            public int year { get; set; }

        }
        /// <summary>
        ///Controller Driver information.
        /// </summary>
        [DataContract]
        public class controllerDriverInfo
        {
            /// <summary>
            /// Gets the current driver information
            /// </summary>
            [DataMember(Name = "CurrentDriver")]
            public string CurrentDriver  { get; set; }

            /// <summary>
            /// Gets the list of all the controller drivers information
            /// </summary>         
            [DataMember(Name = "ControllersDrivers")]
            public string[] ControllersDrivers { get; set; }


        }

        /// <summary>
        ///Dispaly orientation information.
        /// </summary>
        [DataContract]
        public class DisplayOrientationInfo {
            /// <summary>
            /// Gets the dispaly orientation information
            /// </summary>
            [DataMember]
            public int Orientation { get; set; }

        }
        /// <summary>
        ///Dispaly resolution information.
        /// </summary>
        [DataContract]
        public class DisplayResolutionInfo
        {
            /// <summary>
            /// Gets the current resolution information
            /// </summary>
            [DataMember]
            public Resolutions Current;
            
            /// <summary>
            /// Gets the list of resolution specifications
            /// </summary>
            [DataMember]
            public Resolutions[] Resolutions;


        }
        
        public partial class Resolutions
        {
            /// <summary>
            /// Gets the resolution information
            /// </summary>
            [DataMember]
            public string Resolution { get; set; }
            
            /// <summary>
            /// Gets the index for the resolution information
            /// </summary>
            [DataMember]
            public int Index { get; set; }

        }
       
        #endregion // Data contract
    }
}
