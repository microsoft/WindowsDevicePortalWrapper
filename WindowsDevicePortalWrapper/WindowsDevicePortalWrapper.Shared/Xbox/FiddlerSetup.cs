//----------------------------------------------------------------------------------------------
// <copyright file="FiddlerSetup.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <content>
    /// Fiddler setup Wrappers
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// Endpoint for enabling and disabling a Fiddler proxy.
        /// </summary>
        public static readonly string FiddlerSetupApi = "ext/fiddler";

        /// <summary>
        /// Enables Fiddler on the console with the specified proxy.
        /// </summary>
        /// <param name="proxyAddress">The address of the proxy.</param>
        /// <param name="proxyPort">The port the proxy is listening on.</param>
        /// <param name="certFilePath">An optional path to the cert file to use.</param>
        /// <returns>Task tracking completion. A reboot will be required before the tracing begins.</returns>
        public async Task EnableFiddlerTracingAsync(string proxyAddress, string proxyPort, string certFilePath = null)
        {
            if (this.Platform != DevicePortalPlatforms.XboxOne)
            {
                throw new NotSupportedException("This method is only supported on Xbox One.");
            }

            Dictionary<string, string> queryStringList = new Dictionary<string, string>();
            queryStringList.Add("proxyAddress", proxyAddress);
            queryStringList.Add("proxyPort", proxyPort);

            if (!string.IsNullOrEmpty(certFilePath))
            {
                List<string> certFileList = new List<string>();
                certFileList.Add(certFilePath);

                queryStringList.Add("updateCert", "true");

                await this.PostAsync(FiddlerSetupApi, certFileList, Utilities.BuildQueryString(queryStringList));
            }
            else
            {
                await this.PostAsync(FiddlerSetupApi, Utilities.BuildQueryString(queryStringList));
            }
        }

        /// <summary>
        /// Disables Fiddler on the console.
        /// </summary>
        /// <returns>Task tracking completion. A reboot will be required before tracing stops.</returns>
        public async Task DisableFiddlerTracingAsync()
        {
            if (this.Platform != DevicePortalPlatforms.XboxOne)
            {
                throw new NotSupportedException("This method is only supported on Xbox One.");
            }

            await this.DeleteAsync(FiddlerSetupApi);
        }
    }
}
