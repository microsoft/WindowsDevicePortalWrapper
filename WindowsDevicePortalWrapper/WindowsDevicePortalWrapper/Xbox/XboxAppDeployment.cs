//----------------------------------------------------------------------------------------------
// <copyright file="XboxAppDeployment.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <content>
    /// Register Application Wrappers
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// REST endpoint for registering a package from a loose folder
        /// </summary>
        public static readonly string RegisterPackageApi = "api/app/packagemanager/register";

        /// <summary>
        /// REST endpoint for uploading a folder to the DevelopmentFiles loose folder.
        /// </summary>
        public static readonly string UploadPackageFolderApi = "api/app/packagemanager/upload";

        /// <summary>
        /// Registers a loose app on the console
        /// </summary>
        /// <param name="folderName">Relative folder path where the app can be found.</param>
        /// <returns>Task for tracking async completion.</returns>
        public async Task RegisterApplicationAsync(string folderName)
        {
            if (this.Platform != DevicePortalPlatforms.XboxOne)
            {
                throw new NotSupportedException("This method is only supported on Xbox One.");
            }

            await this.PostAsync(
                RegisterPackageApi,
                string.Format("folder={0}", Utilities.Hex64Encode(folderName))).ConfigureAwait(false);

            // Poll the status until complete.
            ApplicationInstallStatus status = ApplicationInstallStatus.InProgress;
            do
            {
                await Task.Delay(TimeSpan.FromMilliseconds(500)).ConfigureAwait(false);

                status = await this.GetInstallStatusAsync().ConfigureAwait(false);
            }
            while (status == ApplicationInstallStatus.InProgress);
        }

        /// <summary>
        /// Uploads a folder to the DevelopmentFiles loose folder.
        /// </summary>
        /// <param name="sourceFolder">The source folder to upload.</param>
        /// <param name="destinationFolder">The destination path to upload it to.</param>
        /// <returns>Task for tracking async completion.</returns>
        public async Task UploadPackageFolderAsync(string sourceFolder, string destinationFolder)
        {
            if (this.Platform != DevicePortalPlatforms.XboxOne)
            {
                throw new NotSupportedException("This method is only supported on Xbox One.");
            }

            List<string> files = new List<string>();
            files.AddRange(Directory.GetFiles(sourceFolder));

            await this.PostAsync(
                UploadPackageFolderApi,
                files,
                string.Format("destinationFolder={0}", Utilities.Hex64Encode(destinationFolder)));
        }
    }
}
