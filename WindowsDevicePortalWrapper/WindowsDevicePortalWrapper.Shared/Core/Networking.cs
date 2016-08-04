//----------------------------------------------------------------------------------------------
// <copyright file="Networking.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <content>
    /// Wrappers for Networking methods.
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// API for getting IP config data.
        /// </summary>
        public static readonly string IpConfigApi = "api/networking/ipconfig";

        /// <summary>
        /// Gets the IP configuration data of the device.
        /// </summary>
        /// <returns>object containing details of the device's network configuration.</returns>
        public async Task<IpConfiguration> GetIpConfig()
        {
            return await this.Get<IpConfiguration>(IpConfigApi);
        }

        #region Data contract

        /// <summary>
        /// DHCP object.
        /// </summary>
        [DataContract]
        public class Dhcp
        {
            /// <summary>
            ///  Gets or sets the time at which the lease will expire, in ticks.
            /// </summary>
            [DataMember(Name = "LeaseExpires")]
            public long LeaseExpiresRaw { get; set; }

            /// <summary>
            /// Gets or sets the time at which the lease was obtained, in ticks.
            /// </summary>
            [DataMember(Name = "LeaseObtained")]
            public long LeaseObtainedRaw { get; set; }

            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            [DataMember(Name = "Address")]
            public IpAddressInfo Address { get; set; }

            /// <summary>
            /// Gets the lease expiration time.
            /// </summary>
            public DateTimeOffset LeaseExpires
            {
                get { return new DateTimeOffset(new DateTime(this.LeaseExpiresRaw)); }
            }

            /// <summary>
            /// Gets the lease obtained time.
            /// </summary>
            public DateTimeOffset LeaseObtained
            {
                get { return new DateTimeOffset(new DateTime(this.LeaseObtainedRaw)); }
            }
        }

        /// <summary>
        /// IP Address info
        /// </summary>
        [DataContract]
        public class IpAddressInfo
        {
            /// <summary>
            /// Gets or sets the address
            /// </summary>
            [DataMember(Name = "IpAddress")]
            public string Address { get; set; }

            /// <summary>
            /// Gets or sets the subnet mask
            /// </summary>
            [DataMember(Name = "Mask")]
            public string SubnetMask { get; set; }
        }

        /// <summary>
        /// IP Configuration object
        /// </summary>
        [DataContract]
        public class IpConfiguration
        {
            /// <summary>
            /// Gets or sets the list of networking adapters
            /// </summary>
            [DataMember(Name = "Adapters")]
            public List<NetworkAdapterInfo> Adapters { get; set; }
        }

        /// <summary>
        /// Networking adapter info
        /// </summary>
        [DataContract]
        public class NetworkAdapterInfo
        {
            /// <summary>
            /// Gets or sets the description
            /// </summary>
            [DataMember(Name = "Description")]
            public string Description { get; set; }

            /// <summary>
            /// Gets or sets the hardware address
            /// </summary>
            [DataMember(Name = "HardwareAddress")]
            public string MacAddress { get; set; }

            /// <summary>
            /// Gets or sets the index
            /// </summary>
            [DataMember(Name = "Index")]
            public int Index { get; set; }

            /// <summary>
            /// Gets or sets the name
            /// </summary>
            [DataMember(Name = "Name")]
            public Guid Id { get; set; }

            /// <summary>
            /// Gets or sets the type
            /// </summary>
            [DataMember(Name = "Type")]
            public string AdapterType { get; set; }

            /// <summary>
            /// Gets or sets DHCP info
            /// </summary>
            [DataMember(Name = "DHCP")]
            public Dhcp Dhcp { get; set; }

            // TODO - WINS

            /// <summary>
            /// Gets or sets Gateway info
            /// </summary>
            [DataMember(Name = "Gateways")]
            public List<IpAddressInfo> Gateways { get; set; }

            /// <summary>
            /// Gets or sets the list of IP addresses
            /// </summary>
            [DataMember(Name = "IpAddresses")]
            public List<IpAddressInfo> IpAddresses { get; set; }
        }
        #endregion // Data contract
    }
}
