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
        private static readonly String _wifiInterfacesApi = "api/wifi/interfaces";
        private static readonly String _wifiNetworkApi = "api/wifi/network";
        private static readonly String _wifiNetworksApi = "api/wifi/networks";

        public async Task ConnectToWifiNetwork(Guid networkAdapter,
                                            String ssid,
                                            String networkKey)
        {
            String payload = String.Format("interface={0}&ssid={1}&op=connect&createprofile=yes&key={2}",
                                        networkAdapter.ToString(),
                                        Utilities.Hex64Encode(ssid), 
                                        Utilities.Hex64Encode(networkKey));

            await Post(_wifiNetworkApi,
                                payload);
        }

        public async Task<WifiInterfaces> GetWifiInterfaces()
        {
            return await Get<WifiInterfaces>(_wifiInterfacesApi);
        }

        public async Task<WifiNetworks> GetWifiNetworks(Guid interfaceGuid)
        {
            return await Get<WifiNetworks>(_wifiNetworksApi, 
                                        String.Format("interface={0}", interfaceGuid.ToString()));
        }
    }

#region Data contract

    [DataContract]
    public class WifiInterface
    {
        [DataMember(Name="Description")]
        public String Description { get; set; }

        [DataMember(Name="GUID")]
        public Guid Guid { get; set; }
        
        [DataMember(Name="Index")]
        public Int32 Index { get; set; }

        [DataMember(Name = "ProfilesList")]
        public List<WifiNetworkProfile> Profiles { get; set; }
    }

    [DataContract]
    public class WifiInterfaces
    {
        [DataMember(Name="Interfaces")]
        public List<WifiInterface> Interfaces { get; set; }
    }

    [DataContract]
    public class WifiNetworks
    {
        [DataMember(Name="AvailableNetworks")]
        public List<WifiNetworkInfo> AvailableNetworks { get; set; }
    }

    [DataContract]
    public class WifiNetworkInfo
    {
        [DataMember(Name="AlreadyConnected")]
        public Boolean IsConnected { get; set; }

        [DataMember(Name="AuthenticationAlgorithm")]
        public String AuthenticationAlgorithm { get; set; }

        [DataMember(Name="Channel")]
        public Int32 Channel { get; set; }

        [DataMember(Name="CipherAlgorithm")]
        public String CipherAlgorithm { get; set; }

        [DataMember(Name="Connectable")]
        public Boolean IsConnectable { get; set; }
    
        [DataMember(Name="InfrastructureType")]
        public String InfrastructureType { get; set; }

        [DataMember(Name="ProfileAvailable")]
        public Boolean IsProfileAvailable { get; set; }
    
        [DataMember(Name="ProfileName")]
        public String ProfileName { get; set; }
 
        [DataMember(Name="SSID")]
        public String Ssid { get; set; }
    
        [DataMember(Name="SecurityEnabled")]
        public Boolean IsSecurityEnabled { get; set; }
    
        [DataMember(Name="SignalQuality")]
        public Int32 SignalQuality { get; set; }

        [DataMember(Name = "BSSID")]
        public List<Int32> Bssid { get; set; }

        [DataMember(Name = "PhysicalTypes")]
        public List<String> NetworkTypes { get; set; }
    }

    [DataContract]
    public class WifiNetworkProfile
    {
        [DataMember(Name="GroupPolicyProfile")]
        public Boolean IsGroupPolicyProfile { get; set; }

        [DataMember(Name="Name")]
        public String Name { get; set; }

        [DataMember(Name="PerUserProfile")]
        public Boolean IsPerUserProfile { get; set; }
    }
#endregion // Data contract
}
