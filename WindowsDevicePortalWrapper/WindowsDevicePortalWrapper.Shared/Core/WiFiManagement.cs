//----------------------------------------------------------------------------------------------
// <copyright file="WiFiManagement.cs" company="Microsoft Corporation">
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
    /// Wrappers for WiFi management methods.
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// API for getting the WiFi interfaces.
        /// </summary>
        public static readonly string WifiInterfacesApi = "api/wifi/interfaces";

        /// <summary>
        /// API for the controlling the WiFi network.
        /// </summary>
        public static readonly string WifiNetworkApi = "api/wifi/network";

        /// <summary>
        /// API for getting available WiFi networks.
        /// </summary>
        public static readonly string WifiNetworksApi = "api/wifi/networks";

        /// <summary>
        /// Connect to a WiFi network using a given network adapter and SSID. 
        /// </summary>
        /// <param name="networkAdapter">Network adaptor GUID.</param>
        /// <param name="ssid">SSID of the network.</param>
        /// <param name="networkKey">Network key.</param>
        /// <returns>Task tracking connection status.</returns>
        public async Task ConnectToWifiNetworkAsync(
            Guid networkAdapter,
            string ssid,
            string networkKey)
        {
            string payload = string.Format(
                "interface={0}&ssid={1}&op=connect&createprofile=yes&key={2}",
                networkAdapter.ToString(),
                Utilities.Hex64Encode(ssid),
                Utilities.Hex64Encode(networkKey));

            await this.PostAsync(
                WifiNetworkApi,
                payload);
        }

        /// <summary>
        /// Gets WiFi interfaces.
        /// </summary>
        /// <returns>List of WiFi interfaces.</returns>
        public async Task<WifiInterfaces> GetWifiInterfacesAsync()
        {
            return await this.GetAsync<WifiInterfaces>(WifiInterfacesApi);
        }

        /// <summary>
        /// Gets WiFi networks as seen from a WiFi interface.
        /// </summary>
        /// <param name="interfaceGuid">Interface to get networks from.</param>
        /// <returns>List of available networks.</returns>
        public async Task<WifiNetworks> GetWifiNetworksAsync(Guid interfaceGuid)
        {
            return await this.GetAsync<WifiNetworks>(
                WifiNetworksApi,
                string.Format("interface={0}", interfaceGuid.ToString()));
        }

        #region Data contract

        /// <summary>
        /// WiFi interface.
        /// </summary>
        [DataContract]
        public class WifiInterface
        {
            /// <summary>
            /// Gets description.
            /// </summary>
            [DataMember(Name = "Description")]
            public string Description { get; private set; }

            /// <summary>
            /// Gets GUID.
            /// </summary>
            [DataMember(Name = "GUID")]
            public Guid Guid { get; private set; }

            /// <summary>
            /// Gets index.
            /// </summary>
            [DataMember(Name = "Index")]
            public int Index { get; private set; }

            /// <summary>
            /// Gets profiles list.
            /// </summary>
            [DataMember(Name = "ProfilesList")]
            public List<WifiNetworkProfile> Profiles { get; private set; }
        }

        /// <summary>
        /// WiFi interfaces.
        /// </summary>
        [DataContract]
        public class WifiInterfaces
        {
            /// <summary>
            /// Gets the list of interfaces.
            /// </summary>
            [DataMember(Name = "Interfaces")]
            public List<WifiInterface> Interfaces { get; private set; }
        }

        /// <summary>
        /// WiFi networks.
        /// </summary>
        [DataContract]
        public class WifiNetworks
        {
            /// <summary>
            /// Gets the list of available networks.
            /// </summary>
            [DataMember(Name = "AvailableNetworks")]
            public List<WifiNetworkInfo> AvailableNetworks { get; private set; }
        }

        /// <summary>
        /// WiFi network info.
        /// </summary>
        [DataContract]
        public class WifiNetworkInfo
        {
            /// <summary>
            /// Gets a value indicating whether the device is already connected to this network.
            /// </summary>
            [DataMember(Name = "AlreadyConnected")]
            public bool IsConnected { get; private set; }

            /// <summary>
            /// Gets the authentication algorithm.
            /// </summary>
            [DataMember(Name = "AuthenticationAlgorithm")]
            public string AuthenticationAlgorithm { get; private set; }

            /// <summary>
            /// Gets the channel.
            /// </summary>
            [DataMember(Name = "Channel")]
            public int Channel { get; private set; }

            /// <summary>
            /// Gets the cipher algorithm.
            /// </summary>
            [DataMember(Name = "CipherAlgorithm")]
            public string CipherAlgorithm { get; private set; }

            /// <summary>
            /// Gets a value indicating whether this network is connectable
            /// </summary>
            [DataMember(Name = "Connectable")]
            public bool IsConnectable { get; private set; }

            /// <summary>
            /// Gets the infrastructure type - ad hoc or standard. 
            /// </summary>
            [DataMember(Name = "InfrastructureType")]
            public string InfrastructureType { get; private set; }
            
            /// <summary>
            /// Gets a value indicating whether a profile is available.
            /// </summary>
            [DataMember(Name = "ProfileAvailable")]
            public bool IsProfileAvailable { get; private set; }

            /// <summary>
            /// Gets the profile name.
            /// </summary>
            [DataMember(Name = "ProfileName")]
            public string ProfileName { get; private set; }

            /// <summary>
            /// Gets the SSID.
            /// </summary>
            [DataMember(Name = "SSID")]
            public string Ssid { get; private set; }

            /// <summary>
            /// Gets a value indicating whether security is enabled.
            /// </summary>
            [DataMember(Name = "SecurityEnabled")]
            public bool IsSecurityEnabled { get; private set; }

            /// <summary>
            /// Gets the signal quality.
            /// </summary>
            [DataMember(Name = "SignalQuality")]
            public int SignalQuality { get; private set; }

            /// <summary>
            /// Gets the BSSID.
            /// </summary>
            [DataMember(Name = "BSSID")]
            public List<int> Bssid { get; private set; }

            /// <summary>
            /// Gets physical types.
            /// </summary>
            [DataMember(Name = "PhysicalTypes")]
            public List<string> NetworkTypes { get; private set; }
        }

        /// <summary>
        /// WiFi network profile.
        /// </summary>
        [DataContract]
        public class WifiNetworkProfile
        {
            /// <summary>
            /// Gets a value indicating whether this is a group policy profile.
            /// </summary>
            [DataMember(Name = "GroupPolicyProfile")]
            public bool IsGroupPolicyProfile { get; private set; }

            /// <summary>
            /// Gets the name.
            /// </summary>
            [DataMember(Name = "Name")]
            public string Name { get; private set; }

            /// <summary>
            /// Gets a value indicating whether this is a per user profile.
            /// </summary>
            [DataMember(Name = "PerUserProfile")]
            public bool IsPerUserProfile { get; private set; }
        }
        #endregion // Data contract
    }
}
