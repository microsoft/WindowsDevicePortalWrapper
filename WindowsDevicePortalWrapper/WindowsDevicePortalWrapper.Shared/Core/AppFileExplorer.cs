//----------------------------------------------------------------------------------------------
// <copyright file="AppFileExplorer.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;

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
        public static readonly string GetFileApi = "api/filesystem/apps/file";

        /// <summary>
        /// API to rename a file in a folder.
        /// </summary>
        public static readonly string RenameFileApi = "api/filesystem/apps/rename";

        /// <summary>
        /// API to retrieve the list of files in a folder.
        /// </summary>
        public static readonly string GetFilesApi = "api/filesystem/apps/files";

        /// <summary>
        /// API to retrieve the list of accessible top-level folders.
        /// </summary>
        public static readonly string KnownFoldersApi = "api/filesystem/apps/knownfolders";

        /// <summary>
        /// Calls the API to retrieve the list of known folders.
        /// </summary>
        /// <returns>List of known folders available on this device.</returns>
        public async Task<KnownFolders> GetKnownFolders()
        {
            return await this.Get<KnownFolders>(KnownFoldersApi);
        }

        /// <summary>
        /// Calls the API to retrieve the contents of a given folder.
        /// </summary>
        /// <param name="knownFolderId">The known folder id for the root of the path.</param>
        /// <param name="subPath">An optional subpath to the folder.</param>
        /// <param name="packageFullName">The package full name if using LocalAppData.</param>
        /// <returns>Contents of the requested folder.</returns>
        public async Task<FolderContents> GetFolderContents(
            string knownFolderId, 
            string subPath = null, 
            string packageFullName = null)
        {
            Dictionary<string, string> payload = this.BuildCommonFilePayload(knownFolderId, subPath, packageFullName);

            return await this.Get<FolderContents>(GetFilesApi, Utilities.BuildQueryString(payload));
        }

        /// <summary>
        /// Calls the API to download a file.
        /// </summary>
        /// <param name="knownFolderId">The known folder id for the root of the path.</param>
        /// <param name="filename">The name of the file we are downloading.</param>
        /// <param name="subPath">An optional subpath to the folder.</param>
        /// <param name="packageFullName">The package full name if using LocalAppData.</param>
        /// <returns>Stream to the downloaded file.</returns>
        public async Task<Stream> GetFile(
            string knownFolderId,
            string filename,
            string subPath = null,
            string packageFullName = null)
        {
            Dictionary<string, string> payload = this.BuildCommonFilePayload(knownFolderId, subPath, packageFullName);

            payload.Add("filename", filename);

            Uri uri = Utilities.BuildEndpoint(
                this.deviceConnection.Connection,
                GetFileApi,
                Utilities.BuildQueryString(payload));

            return await this.Get(uri);
        }

        /// <summary>
        /// Calls the API to upload a file.
        /// </summary>
        /// <param name="knownFolderId">The known folder id for the root of the path.</param>
        /// <param name="filepath">The path to the file we are uploading.</param>
        /// <param name="subPath">An optional subpath to the folder.</param>
        /// <param name="packageFullName">The package full name if using LocalAppData.</param>
        /// <returns>Task tracking completion of the upload request.</returns>
        public async Task UploadFile(
            string knownFolderId,
            string filepath,
            string subPath = null,
            string packageFullName = null)
        {
            Dictionary<string, string> payload = this.BuildCommonFilePayload(knownFolderId, subPath, packageFullName);

            List<string> files = new List<string>();
            files.Add(filepath);

            await this.Post(GetFileApi, files, Utilities.BuildQueryString(payload));
        }

        /// <summary>
        /// Calls the API to delete a file.
        /// </summary>
        /// <param name="knownFolderId">The known folder id for the root of the path.</param>
        /// <param name="filename">The name of the file we are deleting.</param>
        /// <param name="subPath">An optional subpath to the folder.</param>
        /// <param name="packageFullName">The package full name if using LocalAppData.</param>
        /// <returns>Task tracking completion of the delete request.</returns>
        public async Task DeleteFile(
            string knownFolderId,
            string filename,
            string subPath = null,
            string packageFullName = null)
        {
            Dictionary<string, string> payload = this.BuildCommonFilePayload(knownFolderId, subPath, packageFullName);

            payload.Add("filename", filename);

            await this.Delete(GetFileApi, Utilities.BuildQueryString(payload));
        }

        /// <summary>
        /// Calls the API to rename a file.
        /// </summary>
        /// <param name="knownFolderId">The known folder id for the root of the path.</param>
        /// <param name="filename">The name of the file we are renaming.</param>
        /// <param name="newFilename">The new name for this file.</param>
        /// <param name="subPath">An optional subpath to the folder.</param>
        /// <param name="packageFullName">The package full name if using LocalAppData.</param>
        /// <returns>Task tracking completion of the rename request.</returns>
        public async Task RenameFile(
            string knownFolderId,
            string filename,
            string newFilename,
            string subPath = null,
            string packageFullName = null)
        {
            Dictionary<string, string> payload = this.BuildCommonFilePayload(knownFolderId, subPath, packageFullName);

            payload.Add("filename", filename);
            payload.Add("newfilename", newFilename);

            await this.Post(RenameFileApi, Utilities.BuildQueryString(payload));
        }

        /// <summary>
        /// Do some common parsing and validation of file explorer parameters.
        /// </summary>
        /// <param name="knownFolderId">The known folder id.</param>
        /// <param name="subPath">The optional subpath for the folder.</param>
        /// <param name="packageFullName">The packagefullname if using LocalAppData.</param>
        /// <returns>Dictionary of param name to value.</returns>
        private Dictionary<string, string> BuildCommonFilePayload(string knownFolderId, string subPath, string packageFullName)
        {
            Dictionary<string, string> payload = new Dictionary<string, string>();

            payload.Add("knownfolderid", knownFolderId);

            if (!string.IsNullOrEmpty(subPath))
            {
                if (!subPath.StartsWith("/"))
                {
                    subPath = subPath.Insert(0, "/");
                }

                payload.Add("path", subPath);
            }

            if (!string.IsNullOrEmpty(packageFullName))
            {
                payload.Add("packagefullname", packageFullName);
            }
            else if (string.Equals(knownFolderId, "LocalAppData", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("LocalAppData requires a packageFullName be provided.");
            }

            return payload;
        }

        #region Data contract

        /// <summary>
        /// Known Folders object.
        /// </summary>
        [DataContract]
        public class KnownFolders
        {
            /// <summary>
            /// Gets or sets the list of known folders.
            /// </summary>
            [DataMember(Name = "KnownFolders")]
            public List<string> Folders { get; set; }

            /// <summary>
            /// Overridden ToString method providing a user readable
            /// list of known folders.
            /// </summary>
            /// <returns>String representation of the object.</returns>
            public override string ToString()
            {
                if (this.Folders == null)
                {
                    return string.Empty;
                }

                string contents = string.Empty;
                foreach (string folder in this.Folders)
                {
                    contents += folder + '\n';
                }

                return contents;
            }
        }

        /// <summary>
        /// Folder contents object.
        /// </summary>
        [DataContract]
        public class FolderContents
        {
            /// <summary>
            /// Gets or sets the list of folders and files in this folder.
            /// </summary>
            [DataMember(Name = "Items")]
            public List<FileOrFolderInformation> Contents { get; set; }

            /// <summary>
            /// Overridden ToString method providing a user readable
            /// display of a folder's contents. Tries to match the formatting
            /// of regular DIR commands.
            /// </summary>
            /// <returns>String representation of the object.</returns>
            public override string ToString()
            {
                if (this.Contents == null)
                {
                    return string.Empty;
                }

                string contents = string.Empty;
                foreach (FileOrFolderInformation fileOrFolder in this.Contents)
                {
                    contents += fileOrFolder.ToString();
                }

                return contents;
            }
        }

        /// <summary>
        /// Details about a folder or file.
        /// </summary>
        [DataContract]
        public class FileOrFolderInformation
        {
            /// <summary>
            /// Gets or sets the current directory.
            /// </summary>
            [DataMember(Name = "CurrentDir")]
            public string CurrentDir { get; set; }

            /// <summary>
            /// Gets or sets the current directory.
            /// </summary>
            [DataMember(Name = "DateCreated")]
            public long DateCreated { get; set; }

            /// <summary>
            /// Gets or sets the Id.
            /// </summary>
            [DataMember(Name = "Id")]
            public string Id { get; set; }

            /// <summary>
            /// Gets or sets the Name.
            /// </summary>
            [DataMember(Name = "Name")]
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the SubPath (equivalent to CurrentDir for files).
            /// </summary>
            [DataMember(Name = "SubPath")]
            public string SubPath { get; set; }

            /// <summary>
            /// Gets or sets the Type.
            /// </summary>
            [DataMember(Name = "Type")]
            public int Type { get; set; }

            /// <summary>
            /// Gets or sets the size of the file (0 for folders).
            /// </summary>
            [DataMember(Name = "FileSize")]
            public long SizeInBytes { get; set; }

            /// <summary>
            /// Overridden ToString method providing a user readable
            /// display of a file or folder. Tries to match the formatting
            /// of regular DIR commands.
            /// </summary>
            /// <returns>String representation of the object.</returns>
            public override string ToString()
            {
                DateTime timestamp = DateTime.FromFileTime(this.DateCreated);

                // Check if this is a folder.
                if (!string.Equals(this.SubPath, this.CurrentDir))
                {
                    return string.Format("{0,-24:MM/dd/yyyy  HH:mm tt}{1,-14} {2}\n", timestamp, "<DIR>", this.Name);
                }
                else
                {
                    return string.Format("{0,-24:MM/dd/yyyy  HH:mm tt}{1,14:n0} {2}\n", timestamp, this.SizeInBytes, this.Name);
                }
            }
        }

        #endregion
    }
}
