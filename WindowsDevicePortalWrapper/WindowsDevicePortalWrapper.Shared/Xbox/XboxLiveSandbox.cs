//----------------------------------------------------------------------------------------------
// <copyright file="XboxLiveSandbox.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <content>
    /// Xbox Live Sandbox Wrappers
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// Endpoint for getting or changing the Xbox Live sandbox for the device.
        /// </summary>
        public static readonly string XboxLiveSandboxApi = "ext/xboxlive/sandbox";

        /// <summary>
        /// Gets the current Xbox Live sandbox value for this device.
        /// </summary>
        /// <returns>The value of the current sandbox this device is in.</returns>
        public async Task<Sandbox> GetXboxLiveSandboxAsync()
        {
            /*
                This method lives with the Xbox wrappers since it's Xbox Live and
                gaming specific, but it functions on multiple device types, so
                there is no check for Xbox platform type here.
            */

            return await this.GetAsync<Sandbox>(XboxLiveSandboxApi);
        }

        /// <summary>
        /// Sets the Xbox Live sandbox on the device.
        /// </summary>
        /// <param name="newSandbox">The new sandbox to move this device into.</param>
        /// <returns>Task tracking completion. A reboot will be required may be required before the sandbox change takes effect on some devices.</returns>
        public async Task<Sandbox> SetXboxLiveSandboxAsync(string newSandbox)
        {
            /*
                This method lives with the Xbox wrappers since it's Xbox Live and
                gaming specific, but it functions on multiple device types, so
                there is no check for Xbox platform type here.
            */

            Sandbox sandbox = new Sandbox();
            sandbox.Value = newSandbox;

            return await this.PutAsync<Sandbox, Sandbox>(XboxLiveSandboxApi, sandbox);
        }

        #region Data contract

        /// <summary>
        /// Xbox Live Sandbox representation
        /// </summary>
        [DataContract]
        public class Sandbox
        {
            /// <summary>
            /// Gets or sets the sandbox value
            /// </summary>
            [DataMember(Name = "Sandbox")]
            public string Value { get; set; }

            /// <summary>
            /// Returns a string representation of this Sandbox object
            /// </summary>
            /// <returns>String representation of the Sandbox object</returns>
            public override string ToString()
            {
                return this.Value;
            }
        }
        #endregion // Data contract
    }
}
