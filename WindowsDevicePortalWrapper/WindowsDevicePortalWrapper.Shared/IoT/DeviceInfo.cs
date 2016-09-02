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
        /// <summary>
        /// IOT device information API.
        /// </summary>
        public static readonly string IoTOsInfoApi = "api/iot/device/information";

        /// <summary>
        /// IOT device timezone API.
        /// </summary>
        public static readonly string TimezoneInfoApi = "api/iot/device/timezones";

        /// <summary>
        /// IOT device datetime API.
        /// </summary>
        public static readonly string DateTimeInfoApi = "api/iot/device/datetime";

        /// <summary>
        /// IOT device Controller Driver API.
        /// </summary>
        public static readonly string ControllerDriverApi = "api/iot/device/controllersdriver";

        /// <summary>
        /// IOT display resolution API.
        /// </summary>
        public static readonly string DisplayResolutionApi = "api/iot/device/displayresolution";

        /// <summary>
        /// IOT display orientation API.
        /// </summary>
        public static readonly string DisplayOrientationApi = "api/iot/device/displayorientation";

        /// <summary>
        /// IOT device name API.
        /// </summary>
        public static readonly string DeviceNameApi = "api/iot/device/name";

        /// <summary>
        /// IOT Device password API.
        /// </summary>
        public static readonly string ResetPasswordApi = "api/iot/device/password";

        /// <summary>
        /// IOT remote debugging pin API.
        /// </summary>
        public static readonly string NewRemoteDebuggingPinApi = "api/iot/device/remotedebuggingpin";

        /// <summary>
        /// IOT set timezone API.
        /// </summary>
        public static readonly string SetTimeZoneApi = "api/iot/device/settimezone";
        
        /// <summary>
        /// Gets the IoT OS Information.
        /// </summary>
        /// <returns>String containing the OS information.</returns>
        public async Task<IoTOSInfo> GetIoTOSInfo()
        {
            return await this.Get<IoTOSInfo>(IoTOsInfoApi);
        }

        /// <summary>
        /// Gets the Timezone information.
        /// </summary>
        /// <returns>String containing the timezone information.</returns>
        public async Task<TimezoneInfo> GetTimezoneInfo()
        {
            return await this.Get<TimezoneInfo>(TimezoneInfoApi);
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
        public async Task<ControllerDriverInfo> GetControllerDriverInfo()
        {
            return await this.Get<ControllerDriverInfo>(ControllerDriverApi);
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
        /// <param name="name">Name to set for the device.</param>
        /// <returns>Task tracking completion of the REST call.</returns>
        public async Task SetIoTDeviceName(string name)
        {
            await this.Post(DeviceNameApi, string.Format("newdevicename={0}", Utilities.Hex64Encode(name)));
        }

        /// <summary>
        /// Sets a new password.
        /// </summary>
        /// <param name="oldPassword">Old password.</param>
        /// <param name="newPassword">New desired password.</param>
        /// <returns>Task tracking completion of the REST call.</returns>
        public async Task<ErrorInformation> SetNewPassword(string oldPassword, string newPassword)
        {
            return await this.Post<ErrorInformation>(
                ResetPasswordApi,
                string.Format("oldpassword={0}&newpassword={1}", Utilities.Hex64Encode(oldPassword), Utilities.Hex64Encode(newPassword)));
        }

        /// <summary>
        /// Sets a new remote debugging pin.
        /// </summary>
        /// <param name="newPin">New pin.</param>
        /// <returns>Task tracking completion of the REST call.</returns>
        public async Task SetNewRemoteDebuggingPin(string newPin)
        {
            await this.Post(
                 NewRemoteDebuggingPinApi,
                string.Format("newpin={0}", Utilities.Hex64Encode(newPin)));
        }

        /// <summary>
        /// Sets controllers drivers.
        /// </summary>
        /// <param name="newDriver">Driver to set.</param>
        /// <returns>Task tracking completion of the REST call.</returns>
        public async Task<ControllerDriverInfo> SetControllersDrivers(string newDriver)
        {
            return await this.Post<ControllerDriverInfo>(
                 ControllerDriverApi,
                string.Format("newdriver={0}", Utilities.Hex64Encode(newDriver)));
        }

        /// <summary>
        /// Sets Timezone.
        /// </summary>
        /// <param name="index">Timezone index.</param>
        /// <returns>Task tracking completion of the REST call.</returns>
        public async Task<ErrorInformation> SetTimeZone(int index)
        {
            return await this.Post<ErrorInformation>(
                 SetTimeZoneApi,
                string.Format("index={0}", index));
        }

        /// <summary>
        /// Sets display resolution.
        /// </summary>
        /// <param name="displayResolution">New display resolution.</param>
        /// <returns>Task tracking completion of the REST call.</returns>
        public async Task SetDisplayResolution(string displayResolution)
        {
            await this.Post(
                 DisplayResolutionApi,
                string.Format("newdisplayresolution={0}", Utilities.Hex64Encode(displayResolution)));
        }

        /// <summary>
        /// Set display orientation.
        /// </summary>
        /// <param name="displayOrientation">Desired orientation.</param>
        /// <returns>Task tracking completion of the REST call.</returns>
        public async Task SetDisplayOrientation(string displayOrientation)
        {
            await this.Post(
                 DisplayOrientationApi,
                string.Format("newdisplayorientation={0}", Utilities.Hex64Encode(displayOrientation)));
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
            public string Model { get; private set; }

            /// <summary>
            ///  Gets the device name.
            /// </summary>
            [DataMember(Name = "DeviceName")]
            public string Name { get; private set; }

            /// <summary>
            /// Gets the OS version
            /// </summary>
            [DataMember(Name = "OSVersion")]
            public string OSVersion { get; private set; }
        }

        /// <summary>
        /// Timezone information.
        /// </summary>
        [DataContract]
        public class TimezoneInfo
        {
            /// <summary>
            /// Gets the current timezone
            /// </summary>
            [DataMember(Name = "Current")]
            public Timezone CurrentTimeZone { get; private set; }

            /// <summary>
            /// Gets the list of all timezones
            /// </summary>
            [DataMember(Name = "Timezones")]
            public Timezone[] Timezones { get; private set; }
        }

        /// <summary>
        /// Timezone specifications.
        /// </summary>
        [DataContract]
        public partial class Timezone
        {
            /// <summary>
            /// Gets the timezone description
            /// </summary>
            [DataMember(Name = "Description")]
            public string Description { get; private set; }

            /// <summary>
            /// Gets the timezone index
            /// </summary>
            [DataMember(Name = "Index")]
            public int Index { get; private set; }

            /// <summary>
            /// Gets the timezone name
            /// </summary>
            [DataMember(Name = "Name")]
            public string Name { get; private set; }
        }

        /// <summary>
        /// DateTime information.
        /// </summary>
        [DataContract]
        public class DateTimeInfo
        {
            /// <summary>
            /// Gets the current date time
            /// </summary>
            [DataMember(Name = "Current")]
            public DateTimeDescription CurrentDateTime { get; private set; }
        }

        /// <summary>
        /// Current Datetime information.
        /// </summary>
        [DataContract]
        public partial class DateTimeDescription
        {
            /// <summary>
            /// Gets the current day
            /// </summary>
            [DataMember(Name = "Day")]
            public int Day { get; private set; }

            /// <summary>
            /// Gets the current hour 
            /// </summary>
            [DataMember(Name = "Hour")]
            public int Hour { get; private set; }

            /// <summary>
            /// Gets the current minute
            /// </summary>
            [DataMember(Name = "Minute")]
            public int Min { get; private set; }

            /// <summary>
            /// Gets the current month 
            /// </summary>
            [DataMember(Name = "Month")]
            public int Month { get; private set; }

            /// <summary>
            /// Gets the current second 
            /// </summary>
            [DataMember(Name = "Second")]
            public int Sec { get; private set; }

            /// <summary>
            /// Gets the current year 
            /// </summary>
            [DataMember(Name = "Year")]
            public int Year { get; private set; }
        }

        /// <summary>
        /// Controller Driver information.
        /// </summary>
        [DataContract]
        public class ControllerDriverInfo
        {
            /// <summary>
            /// Gets the current driver information
            /// </summary>
            [DataMember(Name = "CurrentDriver")]
            public string CurrentDriver { get; private set; }

            /// <summary>
            /// Gets the list of all the controller drivers information
            /// </summary>         
            [DataMember(Name = "ControllersDrivers")]
            public string[] ControllersDrivers { get; private set; }

            /// <summary>
            /// Gets the request for reboot
            /// </summary>
            [DataMember(Name = "RequestReboot")]
            public string RequestReboot { get; private set; }
        }

        /// <summary>
        /// Dispaly orientation information.
        /// </summary>
        [DataContract]
        public class DisplayOrientationInfo
        {
            /// <summary>
            /// Gets the dispaly orientation information
            /// </summary>
            [DataMember(Name = "Orientation")]
            public int Orientation { get; private set; }
        }

        /// <summary>
        /// Dispaly resolution information.
        /// </summary>
        [DataContract]
        public class DisplayResolutionInfo
        {
            /// <summary>
            /// Gets the current display resolution
            /// </summary>
            [DataMember(Name = "Current")]
            public Resolution CurrentResolution { get; private set; }

            /// <summary>
            /// Gets the list of resolution specifications
            /// </summary>
            [DataMember(Name = "Resolutions")]
            public Resolution[] Resolutions { get; private set; }
        }

        /// <summary>
        /// Dispaly resolution specifications.
        /// </summary>
        [DataContract]
        public partial class Resolution
        {
            /// <summary>
            /// Gets the list of supported display resolutions 
            /// </summary>
            [DataMember(Name = "Resolution")]
            public string ResolutionDetail { get; private set; }

            /// <summary>
            /// Gets the index for the resolution information
            /// </summary>
            [DataMember(Name = "Index")]
            public int Index { get; private set; }
        }

        /// <summary>
        /// Error information if a request fails.
        /// </summary>
        [DataContract]
        public partial class ErrorInformation
        {
            /// <summary>
            /// Gets the error code
            /// </summary>
            [DataMember(Name = "ErrorCode")]
            public int ErrorCode { get; private set; }

            /// <summary>
            /// Gets the status of the request
            /// </summary>
            [DataMember(Name = "Status")]
            public string Status { get; private set; }
        }


        #endregion // Data contract
    }
}
