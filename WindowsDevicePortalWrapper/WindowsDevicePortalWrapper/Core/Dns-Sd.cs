//----------------------------------------------------------------------------------------------
// <copyright file="Dns-Sd.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <content>
    /// Wrappers for DNS methods
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// API to add or delete a tag to the DNS-SD advertisement.
        /// </summary>
        private static readonly string TagApi = "api/dns-sd/tag";

        /// <summary>
        /// API to retrieve or delete the currently applied tags for the device.
        /// </summary>
        private static readonly string TagsApi = "api/dns-sd/tags";
    }
}
