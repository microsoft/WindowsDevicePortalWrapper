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
        

        public static readonly string IoTOsInfoApi = "api/iot/device/information";
        public static readonly string TimezoneInfoApi = "api/iot/device/timezones";
        public static readonly string SetTimezoneInfoApi = "api/iot/device/settimezone";
        public static readonly string DateTimeInfoApi = "api/iot/device/datetime";
        public static readonly string ControllerDriverApi = "api/iot/device/controllersdriver";
        public static readonly string DisplayResolutionApi = "api/iot/device/displayresolution";
        public static readonly string DisplayOrientationApi = "api/iot/device/displayorientation";
        public static readonly string RemoteDebuggingPinApi = "api/iot/device/remotedebuggingpin";
        public static readonly string DeviceNameApi = "api/iot/device/name";
        public async Task<IoTOSInfo> GetIoTOSInfo()
        {
            var info = await this.Get<IoTOSInfo>(IoTOsInfoApi);
            return info;
        }


        public async Task<TimezoneInfo> GetTimezoneInfo()
        {
            var info = await this.Get<TimezoneInfo>(TimezoneInfoApi);
            return info;
        }
        public async Task<DateTimeInfo> GetDateTimeInfo()
        {
            var info = await this.Get<DateTimeInfo>(DateTimeInfoApi);
            return info;
        }
        public async Task<controllerDriverInfo> GetControllerDriverInfo()
        {
            var info = await this.Get<controllerDriverInfo>(ControllerDriverApi);
            return info;
        }
        public async Task<DisplayOrientationInfo> GetDisplayOrientationInfo()
        {
            var info = await this.Get<DisplayOrientationInfo>(DisplayOrientationApi);
            return info;
        }

        public async Task<DisplayResolutionInfo> GetDisplayResolutionInfo()
        {
            var info = await this.Get<DisplayResolutionInfo>(DisplayResolutionApi);
            return info;
        }
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
            /// Gets or sets the language
            /// </summary>
            [DataMember(Name = "DeviceModel")]
            public string Model { get; set; }

            /// <summary>
            ///  Gets or sets the OS name.
            /// </summary>
            [DataMember(Name = "DeviceName")]
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the edition
            /// </summary>
            [DataMember(Name = "OSVersion")]
            public string OSVersion { get; set; }
        }

        [DataContract]
        public class TimezoneInfo
        {
            [DataMember(Name = "Current")]
            public Current1 Current;

            [DataMember(Name = "Timezones")]
            public Timezones[] Timezones;

        }
        [DataContract]
        public partial class Current1 {
            [DataMember(Name = "Description")]
            public string description { get; set; }

            [DataMember(Name = "Index")]
            public int index { get; set; }

             [DataMember(Name = "Name")]
            public string name { get; set; }
            
        }

        [DataContract]
        public partial class Timezones
        {
            [DataMember(Name = "Description")]
            public string description { get; set; }

            [DataMember(Name = "Index")]
            public int index { get; set; }
            
            [DataMember(Name = "Name")]
            public string name { get; set; }
            
        }

        [DataContract]
        public class DateTimeInfo
        {
            [DataMember(Name = "Current")]
            public Current2 Current;
        }

        [DataContract]
        public partial class Current2{

            [DataMember(Name = "Day")]
            public int day { get; set; }

            [DataMember(Name = "Hour")]
            public int hour { get; set; }

            [DataMember(Name = "Minute")]
            public int min { get; set; }

            [DataMember(Name = "Month")]
            public int month { get; set; }

            [DataMember(Name ="Second")]
            public int sec { get; set; }

            [DataMember(Name = "Year")]
            public int year { get; set; }

        }

        [DataContract]
        public class controllerDriverInfo
        {
            
            [DataMember(Name = "CurrentDriver")]
            public string CurrentDriver  { get; set; }

         
            [DataMember(Name = "ControllersDrivers")]
            public string[] ControllersDrivers { get; set; }


        }


        [DataContract]
        public class DisplayOrientationInfo {

            [DataMember]
            public int Orientation { get; set; }

        }

        [DataContract]
        public class DisplayResolutionInfo
        {

            [DataMember]
            public Current3 Current;
            [DataMember]
            public Resolutions[] Resolutions;


        }
        [DataContract]
        public partial class Current3
        {
            [DataMember]
            public string Resolution { get; set; }

        }
        public partial class Resolutions {

            [DataMember]
            public string Resolution { get; set; }
            [DataMember]
            public int Index { get; set; }

        }
        [DataContract]
        public class DeviceNameInfo
        {
             [DataMember(Name = "DeviceName")]
            public string devicename { get; set; }
            
            public override string ToString()
            {
                return this.devicename;
            }
        }

        #endregion // Data contract
    }
}
