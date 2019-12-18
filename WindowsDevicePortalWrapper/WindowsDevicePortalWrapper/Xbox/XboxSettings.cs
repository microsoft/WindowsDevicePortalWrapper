//----------------------------------------------------------------------------------------------
// <copyright file="XboxSettings.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using static Microsoft.Tools.WindowsDevicePortal.DevicePortal;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <content>
    /// XboxSettings Wrappers
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// Endpoint for Xbox settings management REST calls.
        /// </summary>
        public static readonly string XboxSettingsApi = "ext/settings";

        /// <summary>
        /// Gets the Xbox Settings info for all settings which can be controlled on the device.
        /// </summary>
        /// <returns>XboxSettingList object containing a List of XboxSetting objects representing the settings on the device.</returns>
        public async Task<XboxSettingList> GetXboxSettingsAsync()
        {
            if (this.Platform != DevicePortalPlatforms.XboxOne)
            {
                throw new NotSupportedException("This method is only supported on Xbox One.");
            }

            return await this.GetAsync<XboxSettingList>(XboxSettingsApi);
        }

        /// <summary>
        /// Gets the value for a single setting.
        /// </summary>
        /// <param name="settingName">Name of the setting we want to retrieve.</param>
        /// <returns>XboxSetting object containing a information about the settings on the device.</returns>
        public async Task<XboxSetting> GetXboxSettingAsync(string settingName)
        {
            if (this.Platform != DevicePortalPlatforms.XboxOne)
            {
                throw new NotSupportedException("This method is only supported on Xbox One.");
            }

            return await this.GetAsync<XboxSetting>(Path.Combine(XboxSettingsApi, settingName));
        }

        /// <summary>
        /// Updates info for the given Xbox setting.
        /// </summary>
        /// <param name="setting">Setting to be updated.</param>
        /// <returns>Task for tracking async completion.</returns>
        public async Task<XboxSetting> UpdateXboxSettingAsync(XboxSetting setting)
        {
            if (this.Platform != DevicePortalPlatforms.XboxOne)
            {
                throw new NotSupportedException("This method is only supported on Xbox One.");
            }

            return await this.PutAsync<XboxSetting, XboxSetting>(Path.Combine(XboxSettingsApi, setting.Name), setting);
        }

        #region Data contract

        /// <summary>
        /// List of settings.
        /// </summary>
        [DataContract]
        public class XboxSettingList
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="XboxSettingList"/> class.
            /// </summary>
            public XboxSettingList()
            {
                this.Settings = new List<XboxSetting>();
            }

            /// <summary>
            ///  Gets the Settings list.
            /// </summary>
            [DataMember(Name = "Settings")]
            public List<XboxSetting> Settings { get; private set; }

            /// <summary>
            /// Returns a string representation of a Settings list.
            /// </summary>
            /// <returns>String representation of a settings list.</returns>
            public override string ToString()
            {
                string settingString = string.Empty;
                foreach (XboxSetting setting in this.Settings)
                {
                    settingString += setting + "\n";
                }

                return settingString;
            }
        }

        /// <summary>
        /// XboxSetting object
        /// </summary>
        [DataContract]
        public class XboxSetting
        {
            /// <summary>
            /// Gets or sets the name identifying this setting.
            /// </summary>
            [DataMember(Name = "Name", EmitDefaultValue = false)]
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the value for this setting.
            /// </summary>
            [DataMember(Name = "Value", EmitDefaultValue = false)]
            public string Value { get; set; }

            /// <summary>
            /// Gets the category for this setting.
            /// </summary>
            [DataMember(Name = "Category", EmitDefaultValue = false)]
            public string Category { get; private set; }

            /// <summary>
            /// Gets whether changing this setting.
            /// requires a reboot to take effect.
            /// </summary>
            [DataMember(Name = "RequiresReboot", EmitDefaultValue = false)]
            public string RequiresReboot { get; private set; }

            /// <summary>
            /// Returns a string representation of a Setting.
            /// </summary>
            /// <returns>String representation of a setting.</returns>
            public override string ToString()
            {
                return string.Format("{0}: {1}", this.Name, this.Value);
            }
        }
        #endregion // Data contract
    }
}
