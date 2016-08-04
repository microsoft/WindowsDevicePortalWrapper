//----------------------------------------------------------------------------------------------
// <copyright file="Dns-Sd.cs" company="Microsoft Corporation">
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
    /// Wrappers for DNS methods
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// API to add or delete a tag to the DNS-SD advertisement.
        /// </summary>
        public static readonly string TagApi = "api/dns-sd/tag";

        /// <summary>
        /// API to retrieve or delete the currently applied tags for the device.
        /// </summary>
        public static readonly string TagsApi = "api/dns-sd/tags";

        /// <summary>
        /// Gets the name of the device.
        /// </summary>
        /// <returns>Array of strings, each one an individual tag.</returns>
        public async Task<List<string>> GetServiceTags()
        {
            ServiceTags tags = await this.Get<ServiceTags>(TagsApi);
            return tags.Tags;
        }

        /// <summary>
        /// Adds a tag to this device's DNS-SD broadcast. 
        /// </summary>
        /// <param name="tagValue">The tag to assign to the device.</param>
        /// <returns>Task tracking adding the tag.</returns>
        public async Task AddServiceTag(string tagValue)
        {
            await this.Post(
                TagApi,
                string.Format("tagValue={0}", tagValue));
        }

        /// <summary>
        /// Delete all tags from the device's DNS-SD broadcast. 
        /// </summary>
        /// <returns>Task tracking deletion of all tags.</returns>
        public async Task DeleteAllTags()
        {
            await this.Delete(TagsApi);
        }

        /// <summary>
        /// Delete a specific tag from the device's DNS-SD broadcast. 
        /// </summary>
        /// <param name="tagValue">The tag to delete from the device broadcast.</param>
        /// <returns>Task tracking deletion of the tag.</returns>
        public async Task DeleteTag(string tagValue)
        {
            await this.Delete(
                TagApi,
                string.Format("tagValue={0}", tagValue));
        }

        #region Data contract

        /// <summary>
        /// Service tags object
        /// </summary>
        [DataContract]
        public class ServiceTags
        {
            /// <summary>
            /// Gets or sets the DNS-SD service tags
            /// </summary>
            [DataMember(Name = "tags")]
            public List<string> Tags
            {
                get; set;
            }

            #endregion Data contract
        }
    }
}
