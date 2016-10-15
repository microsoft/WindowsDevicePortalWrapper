//----------------------------------------------------------------------------------------------
// <copyright file="FileOperation.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Tools.WindowsDevicePortal;
using static Microsoft.Tools.WindowsDevicePortal.DevicePortal;

namespace XboxWdpDriver
{
    /// <summary>
    /// Helper for file related operations
    /// </summary>
    public class FileOperation
    {
        /// <summary>
        /// Usage message for this operation
        /// </summary>
        private const string XblFileUsageMessage = "Usage:\n" +
            "  /subop:knownfolders\n" +
            "        Lists all available known folder ids on the console\n" +
            "  /subop:dir /knownfolderid:<knownfolderid> [/subpath:<subpath>] [/packagefullname:<packageFullName>]\n" +
            "        Lists the directory contents at the given knownfoldid and optionally subpath.\n" +
            "  /subop:download /knownfolderid:<knownfolderid> /filename:<name of the file to download> /destination:<filepath for storing the file> [/subpath:<subpath>] [/packagefullname:<packageFullName>]\n" +
            "        Downloads the requested file to the desired destination.\n" +
            "  /subop:upload /knownfolderid:<knownfolderid> /filepath:<filepath of the file to upload> [/subpath:<subpath>] [/packagefullname:<packageFullName>]\n" +
            "        Uploads a file to the requested folder.\n" +
            "  /subop:rename /knownfolderid:<knownfolderid> /filename:<name of the file to rename> /newfilename:<new filename> [/subpath:<subpath>] [/packagefullname:<packageFullName>]\n" +
            "        Renames a given file.\n" +
            "  /subop:delete /knownfolderid:<knownfolderid> /filename:<name of the file to delete> [/subpath:<subpath>] [/packagefullname:<packageFullName>]\n" +
            "        Deletes the given file.\n";

        /// <summary>
        /// Main entry point for handling a Setting operation
        /// </summary>
        /// <param name="portal">DevicePortal reference for communicating with the device.</param>
        /// <param name="parameters">Parsed command line parameters.</param>
        public static void HandleOperation(DevicePortal portal, ParameterHelper parameters)
        {
            if (parameters.HasFlag(ParameterHelper.HelpFlag))
            {
                Console.WriteLine(XblFileUsageMessage);
                return;
            }

            string operationType = parameters.GetParameterValue("subop");

            if (string.IsNullOrWhiteSpace(operationType))
            {
                Console.WriteLine("Missing subop parameter");
                Console.WriteLine();
                Console.WriteLine(XblFileUsageMessage);
                return;
            }

            operationType = operationType.ToLowerInvariant();

            string knownFolderId = parameters.GetParameterValue("knownfolderid");
            string subPath = parameters.GetParameterValue("subpath");
            string packageFullName = parameters.GetParameterValue("packagefullname");

            try
            {
                if (operationType.Equals("knownfolders"))
                {
                    Task<KnownFolders> getKnownFoldersTask = portal.GetKnownFoldersAsync();

                    getKnownFoldersTask.Wait();
                    Console.WriteLine(getKnownFoldersTask.Result);
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(knownFolderId))
                    {
                        Console.WriteLine("Missing knownfolderid parameter");
                        Console.WriteLine();
                        Console.WriteLine(XblFileUsageMessage);
                        return;
                    }

                    if (operationType.Equals("dir"))
                    {
                        Task<FolderContents> getDirectoryContents = portal.GetFolderContentsAsync(knownFolderId, subPath, packageFullName);

                        getDirectoryContents.Wait();
                        Console.WriteLine(getDirectoryContents.Result);
                    }
                    else if (operationType.Equals("upload"))
                    {
                        string filepath = parameters.GetParameterValue("filepath");

                        if (string.IsNullOrWhiteSpace(filepath))
                        {
                            Console.WriteLine("Missing filepath parameter");
                            Console.WriteLine();
                            Console.WriteLine(XblFileUsageMessage);
                            return;
                        }

                        Task uploadFileTask = portal.UploadFileAsync(knownFolderId, filepath, subPath, packageFullName);

                        uploadFileTask.Wait();
                        Console.WriteLine(string.Format("{0} uploaded.", filepath));
                    }
                    else
                    {
                        string filename = parameters.GetParameterValue("filename");

                        if (string.IsNullOrWhiteSpace(filename))
                        {
                            Console.WriteLine("Missing filename parameter");
                            Console.WriteLine();
                            Console.WriteLine(XblFileUsageMessage);
                            return;
                        }

                        if (operationType.Equals("download"))
                        {
                            string destination = parameters.GetParameterValue("destination");

                            if (string.IsNullOrWhiteSpace(destination))
                            {
                                Console.WriteLine("Missing destination parameter");
                                Console.WriteLine();
                                Console.WriteLine(XblFileUsageMessage);
                                return;
                            }

                            destination += "/" + filename;

                            Task<Stream> getFile = portal.GetFileAsync(knownFolderId, filename, subPath, packageFullName);

                            getFile.Wait();

                            using (FileStream filestream = new FileStream(destination, FileMode.OpenOrCreate, FileAccess.Write))
                            {
                                getFile.Result.CopyTo(filestream);
                            }

                            Console.WriteLine(string.Format("Downloaded {0}.", destination));
                        }
                        else if (operationType.Equals("rename"))
                        {
                            string newfilename = parameters.GetParameterValue("newfilename");

                            if (string.IsNullOrWhiteSpace(newfilename))
                            {
                                Console.WriteLine("Missing newfilename parameter");
                                Console.WriteLine();
                                Console.WriteLine(XblFileUsageMessage);
                                return;
                            }

                            Task renameFileTask = portal.RenameFileAsync(knownFolderId, filename, newfilename, subPath, packageFullName);

                            renameFileTask.Wait();
                            Console.WriteLine(string.Format("Renamed {0} to {1}.", filename, newfilename));
                        }
                        else if (operationType.Equals("delete"))
                        {
                            Task deleteFileTask = portal.DeleteFileAsync(knownFolderId, filename, subPath, packageFullName);

                            deleteFileTask.Wait();
                            Console.WriteLine(string.Format("Deleted {0}.", filename));
                        }
                    }
                }
            }
            catch (AggregateException e)
            {
                if (e.InnerException != null && e.InnerException is DevicePortalException)
                {
                    DevicePortalException exception = e.InnerException as DevicePortalException;

                    Console.WriteLine(string.Format("HTTP Status: {0}, Hresult: 0x{1:X8}. {2}", exception.StatusCode, exception.HResult, exception.Reason));
                }
            }
        }
    }
}