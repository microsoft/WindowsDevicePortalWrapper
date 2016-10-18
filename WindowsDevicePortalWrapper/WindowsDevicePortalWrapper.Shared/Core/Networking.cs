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
        public async Task<IpConfiguration> GetIpConfigAsync()
        {
            return await this.GetAsync<IpConfiguration>(IpConfigApi);
        }

        #region Data contract

        /// <summary>
        /// DHCP object.
        /// </summary>
        [DataContract]
        public class Dhcp
        {
            /// <summary>
            ///  Gets the time at which the lease will expire, in ticks.
            /// </summary>
            [DataMember(Name = "LeaseExpires")]
            public long LeaseExpiresRaw { get; private set; }

            /// <summary>
            /// Gets the time at which the lease was obtained, in ticks.
            /// </summary>
            [DataMember(Name = "LeaseObtained")]
            public long LeaseObtainedRaw { get; private set; }

            /// <summary>
            /// Gets the name.
            /// </summary>
            [DataMember(Name = "Address")]
            public IpAddressInfo Address { get; private set; }

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
            /// Gets the address
            /// </summary>
            [DataMember(Name = "IpAddress")]
            public string Address { get; private set; }

            /// <summary>
            /// Gets the subnet mask
            /// </summary>
            [DataMember(Name = "Mask")]
            public string SubnetMask { get; private set; }
        }

        /// <summary>
        /// IP Configuration object
        /// </summary>
        [DataContract]
        public class IpConfiguration
        {
            /// <summary>
            /// Gets the list of networking adapters
            /// </summary>
            [DataMember(Name = "Adapters")]
            public List<NetworkAdapterInfo> Adapters { get; private set; }
        }

        /// <summary>
        /// Networking adapter info
        /// </summary>
        [DataContract]
        public class NetworkAdapterInfo
        {
            /// <summary>
            /// Gets the description
            /// </summary>
            [DataMember(Name = "Description")]
            public string Description { get; private set; }

            /// <summary>
            /// Gets the hardware address
            /// </summary>
            [DataMember(Name = "HardwareAddress")]
            public string MacAddress { get; private set; }

            /// <summary>
            /// Gets the index
            /// </summary>
            [DataMember(Name = "Index")]
            public int Index { get; private set; }

            /// <summary>
            /// Gets the name
            /// </summary>
            [DataMember(Name = "Name")]
            public Guid Id { get; private set; }

            /// <summary>
            /// Gets the type
            /// </summary>
            [DataMember(Name = "Type")]
            public string AdapterType { get; private set; }

            /// <summary>
            /// Gets DHCP info
            /// </summary>
            [DataMember(Name = "DHCP")]
            public Dhcp Dhcp { get; private set; }

            // TODO - WINS

            /// <summary>
            /// Gets Gateway info
            /// </summary>
            [DataMember(Name = "Gateways")]
            public List<IpAddressInfo> Gateways { get; private set; }

            /// <summary>
            /// Gets the list of IP addresses
            /// </summary>
            [DataMember(Name = "IpAddresses")]
            public List<IpAddressInfo> IpAddresses { get; private set; }
        }
        #endregion // Data contract
    }
}
