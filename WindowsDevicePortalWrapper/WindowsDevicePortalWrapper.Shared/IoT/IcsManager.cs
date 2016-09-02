//----------------------------------------------------------------------------------------------
// <copyright file="IcsManager.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal
{

    public partial class DevicePortal
    {
        public static readonly string IcsInterfacesApi = "api/iot/ics/interfaces";
        public static readonly string IcSharingApi = "api/iot/ics/sharing";

        /// <summary>
        /// Gets the internet connection sharing(ICS) interfaces .
        /// </summary>
        /// <returns>String containing the internet connection sharing(ICS) interfaces.</returns>
        public async Task<IscInterfacesInfo> GetIcsInterfacesInfo()
        {
            return await this.Get<IscInterfacesInfo>(IcsInterfacesApi);
        }

        /// <summary>
        /// Starts the internet connection sharing(ICS).
        /// </summary>
        public async Task IcSharingStart(string privateInterface, string publicInterface)
        {
            await this.Post(
                IcSharingApi, string.Format("PrivateInterface={0}&PublicInterface={1}", Utilities.Hex64Encode(privateInterface), Utilities.Hex64Encode(publicInterface)));
        }

        /// <summary>
        ///  Stops the internet connection sharing(ICS).
        /// </summary>
        public async Task IcSharingStop(string privateInterface, string publicInterface)
        {
            await this.Delete(
                IcSharingApi, string.Format("PrivateInterface={0}&PublicInterface={1}", Utilities.Hex64Encode(privateInterface), Utilities.Hex64Encode(publicInterface)));
        }

        #region Data contract

        /// <summary>
        ///  internet connection sharing(ICS) interfaces.
        /// </summary>
        [DataContract]
        public class IscInterfacesInfo
        {
            /// <summary>
            /// Gets the internet connection sharing(ICS) private interfaces.
            /// </summary>
            [DataMember(Name = "PrivateInterfaces")]
            public string[] PrivateInterfaces { get; private set; }

            /// <summary>
            ///  Gets the internet connection sharing(ICS) public interfaces.
            /// </summary>
            [DataMember(Name = "PublicInterfaces")]
            public string[] PublicInterfaces { get; private set; }
        }
        #endregion // Data contract
    }
}
