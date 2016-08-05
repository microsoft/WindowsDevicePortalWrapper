//----------------------------------------------------------------------------------------------
// <copyright file="DeviceManager.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <content>
    /// Wrappers for device management methods.
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// API to retrieve list of installed devices.
        /// </summary>
        public static readonly string InstalledDevicesApi = "api/devicemanager/devices";

        /// <summary>
        /// Get a listing of installed devices
        /// </summary>
        /// <returns>List of installed devices</returns>
        public async Task<List<Device>> GetDeviceList()
        {
            DeviceList deviceList = await this.Get<DeviceList>(InstalledDevicesApi);
            return deviceList.Devices;
        }

        #region Data contract
        /// <summary>
        /// Object representing a device entry
        /// </summary>
        [DataContract]
        public class DeviceList
        {
            /// <summary>
            /// Gets or sets the Device Class
            /// </summary>
            [DataMember(Name = "DeviceList")]
            public List<Device> Devices { get; set; }
        }

        /// <summary>
        /// Object representing a device entry
        /// </summary>
        [DataContract]
        public class Device
        {
            /// <summary>
            /// Gets or sets the Device Class
            /// </summary>
            [DataMember(Name = "Class")]
            public string Class { get; set; }

            /// <summary>
            /// Gets or sets the Device Description
            /// </summary>
            [DataMember(Name = "Description")]
            public string Description { get; set; }

            /// <summary>
            /// Gets or sets the friendly (human-readable) name for the device.  Usually more descriptive than Description. Does not apply to all Devices.
            /// </summary>
            [DataMember(Name = "FriendlyName")]
            public string FriendlyName { get; set; }

            /// <summary>
            /// Gets or sets the Device ID
            /// </summary>
            [DataMember(Name = "ID")]
            public string ID { get; set; }

            /// <summary>
            /// Gets or sets the Device Manufacturer
            /// </summary>
            [DataMember(Name = "Manufacturer")]
            public string Manufacturer { get; set; }

            /// <summary>
            /// Gets or sets the Device ParentID, used for pairing 
            /// </summary>
            [DataMember(Name = "ParentID")]
            public string ParentID { get; set; }

            /// <summary>
            /// Gets or sets the Device Problem Code
            /// </summary>
            [DataMember(Name = "ProblemCode")]
            public int ProblemCode { get; set; }

            /// <summary>
            /// Gets or sets the Device Status Code
            /// </summary>
            [DataMember(Name = "StatusCode")]
            public int StatusCode { get; set; }
        }
        #endregion
    }
}
