//----------------------------------------------------------------------------------------------
// <copyright file="XboxMediaCapture.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <content>
    /// MediaCapture Wrappers
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// Endpoint for getting a screenshot.
        /// </summary>
        public static readonly string GetXboxScreenshotApi = "ext/screenshot";

        /// <summary>
        /// Takes a current screenshot of the device.
        /// </summary>
        /// <returns>A stream of the screenshot in PNG form.</returns>
        public async Task<Stream> TakeXboxScreenshotAsync()
        {
            if (this.Platform != DevicePortalPlatforms.XboxOne)
            {
                throw new NotSupportedException("This method is only supported on Xbox One.");
            }

            Uri uri = Utilities.BuildEndpoint(
                this.deviceConnection.Connection,
                GetXboxScreenshotApi);

            return await this.GetAsync(uri);
        }
    }
}
