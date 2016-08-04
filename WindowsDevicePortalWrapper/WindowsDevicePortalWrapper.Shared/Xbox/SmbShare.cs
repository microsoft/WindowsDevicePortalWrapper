//----------------------------------------------------------------------------------------------
// <copyright file="SmbShare.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <content>
    /// SMBShare Wrappers
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// Endpoint for SMB share info
        /// </summary>
        private static readonly string GetSmbShareInfoApi = "ext/smb/developerfolder";

        /// <summary>
        /// Gets the SMB Share info for the device
        /// </summary>
        /// <returns>The SMB path, username, and password.</returns>
        public async Task<SmbInfo> GetSmbShareInfo()
        {
            if (this.Platform != DevicePortalPlatforms.XboxOne)
            {
                throw new NotSupportedException("This method is only supported on Xbox One.");
            }

            return await this.Get<SmbInfo>(GetSmbShareInfoApi);
        }

        #region Data contract

        /// <summary>
        /// SMB Info representation
        /// </summary>
        [DataContract]
        public class SmbInfo
        {
            /// <summary>
            /// Gets or sets path
            /// </summary>
            [DataMember(Name = "Path")]
            public string Path { get; set; }

            /// <summary>
            /// Gets or sets Username
            /// </summary>
            [DataMember(Name = "Username")]
            public string Username { get; set; }

            /// <summary>
            /// Gets or sets Password
            /// </summary>
            [DataMember(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            /// Returns a string representation of this SMB info object
            /// </summary>
            /// <returns>String representation of the SMB Info object</returns>
            public override string ToString()
            {
                return this.Path;
            }
        }
        #endregion // Data contract
    }
}
