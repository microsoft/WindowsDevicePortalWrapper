//----------------------------------------------------------------------------------------------
// <copyright file="AppFileExplorer.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <content>
    /// Wrappers for App File explorer methods
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// API to upload, download or delete a file in a folder.
        /// </summary>
        private static readonly string GetFileApi = "api/filesystem/apps/file";

        /// <summary>
        /// API to retrieve the list of files in a folder.
        /// </summary>
        private static readonly string GetFilesApi = "api/filesystem/apps/files";

        /// <summary>
        /// API to retrieve the list of accessible top-level folders.
        /// </summary>
        private static readonly string KnownFoldersApi = "api/filesystem/apps/knownfilders";
    }
}
