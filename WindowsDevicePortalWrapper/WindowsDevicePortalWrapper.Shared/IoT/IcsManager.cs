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
    /// <content>
    /// Wrappers for Internet Connection Sharing(ICS) Settings.
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// IOT ICS Interface API.
        /// </summary>
        public static readonly string IcsInterfacesApi = "api/iot/ics/interfaces";

        /// <summary>
        /// IOT ICS Sharing API.
        /// </summary>
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
        /// Starts internet connection sharing(ICS).
        /// </summary>
        /// <param name="privateInterface">Private Interface.</param>
        /// <param name="publicInterface">Public Interface.</param>
        /// <returns>Task tracking completion of the REST call.</returns>
        public async Task IcSharingStart(string privateInterface, string publicInterface)
        {
            await this.Post(
                IcSharingApi, string.Format("PrivateInterface={0}&PublicInterface={1}", Utilities.Hex64Encode(privateInterface), Utilities.Hex64Encode(publicInterface)));
        }

        /// <summary>
        ///  Stops internet connection sharing(ICS).
        /// </summary>
        /// <param name="privateInterface">Private Interface.</param>
        /// <param name="publicInterface">Public Interface.</param>
        /// <returns>Task tracking completion of the REST call.</returns>
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
