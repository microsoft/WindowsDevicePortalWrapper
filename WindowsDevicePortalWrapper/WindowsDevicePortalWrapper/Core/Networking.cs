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
        private static readonly String _ipConfigApi = "api/networking/ipconfig";

        /// <summary>
        /// Gets the IP configuration data of the device.
        /// </summary>
        /// <returns>IpConfiguration object containing details of the device's network configuration.</returns>
        public async Task<IpConfiguration> GetIpConfig()
        {
            return await Get<IpConfiguration>(_ipConfigApi);
        }
    }

#region Data contract
    [DataContract]
    public class Dhcp
    {
        [DataMember(Name="LeaseExpires")]
        public Int64 LeaseExpiresRaw { get; set; }

        [DataMember(Name="LeaseObtained")]
        public Int64 LeaseObtainedRaw { get; set; }

        [DataMember(Name="Address")]
        public IpAddressInfo Address { get; set; }

        public DateTimeOffset LeaseExpires
        {
            get { return new DateTimeOffset(new DateTime(LeaseExpiresRaw)); }
        }

        public DateTimeOffset LeaseObtained
        {
            get { return new DateTimeOffset(new DateTime(LeaseObtainedRaw)); }
        }
    }

    [DataContract]
    public class IpAddressInfo
    {
        [DataMember(Name="IpAddress")]
        public String Address { get; set; }

        [DataMember(Name="Mask")]
        public String SubnetMask { get; set; }
    }

    [DataContract]
    public class IpConfiguration
    {
        [DataMember(Name="Adapters")]
        public List<NetworkAdapterInfo> Adapters { get; set; }
    }

    [DataContract]
    public class NetworkAdapterInfo
    {
        [DataMember(Name="Description")]
        public String Description { get; set; }

        [DataMember(Name="HardwareAddress")]
        public String MacAddress { get; set; }

        [DataMember(Name="Index")]
        public Int32 Index { get; set; }

        [DataMember(Name="Name")]
        public Guid Id { get; set; }

        [DataMember(Name="Type")]
        public String AdapterType { get; set; }

        [DataMember(Name="DHCP")]
        public Dhcp Dhcp { get; set; }

        // BUGBUG - WINS

        [DataMember(Name="Gateways")]
        public List<IpAddressInfo> Gateways { get; set; }

        [DataMember(Name="IpAddresses")]
        public List<IpAddressInfo> IpAddresses { get; set; }
    }
#endregion // Data contract
}
