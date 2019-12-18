//----------------------------------------------------------------------------------------------
// <copyright file="WindowsErrorReporting.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsDevicePortal
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Threading.Tasks;

    /// <content>
    /// Wrapper for collecting Windows Error Reports from the device. 
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// API for downloading a Windows error reporting file.
        /// </summary>
        public static readonly string WindowsErrorReportingFileApi = "api/wer/report/file";

        /// <summary>
        /// API for getting the list of files in a Windows error report.
        /// </summary>
        public static readonly string WindowsErrorReportingFilesApi = "api/wer/report/files";

        /// <summary>
        /// API for getting the list of Windows error reports.
        /// </summary>
        public static readonly string WindowsErrorReportsApi = "api/wer/reports";

        /// <summary>
        /// Gets the list of Windows Error Reporting (WER) reports.
        /// </summary>
        /// <returns>The list of Windows Error Reporting (WER) reports.</returns>
        public async Task<WerDeviceReports> GetWindowsErrorReportsAsync()
        {
            this.CheckPlatformSupport();

            return await this.GetAsync<WerDeviceReports>(WindowsErrorReportsApi);
        }

        /// <summary>
        /// Gets the list of files in a Windows Error Reporting (WER) report.
        /// </summary>
        /// <param name="user">The user associated with the report.</param>
        /// <param name="type">The type of report. This can be either 'queried' or 'archived'.</param>
        /// <param name="name">The name of the report.</param>
        /// <returns>The list of files.</returns>
        public async Task<WerFiles> GetWindowsErrorReportingFileListAsync(string user, string type, string name)
        {
            this.CheckPlatformSupport();

            Dictionary<string, string> payload = new Dictionary<string, string>();
            payload.Add("user", user);
            payload.Add("type", type);
            payload.Add("name", Utilities.Hex64Encode(name));

            return await this.GetAsync<WerFiles>(WindowsErrorReportingFilesApi, Utilities.BuildQueryString(payload));
        }

        /// <summary>
        /// Gets the specified file from a Windows Error Reporting (WER) report.
        /// </summary>
        /// <param name="user">The user associated with the report.</param>
        /// <param name="type">The type of report. This can be either 'queried' or 'archived'.</param>
        /// <param name="name">The name of the report.</param>
        /// <param name="file">The name of the file to download from the report.</param>
        /// <returns>Byte array containing the file data</returns>
        public async Task<byte[]> GetWindowsErrorReportingFileAsync(string user, string type, string name, string file)
        {
            this.CheckPlatformSupport();

            Dictionary<string, string> payload = new Dictionary<string, string>();
            payload.Add("user", user);
            payload.Add("type", type);
            payload.Add("name", Utilities.Hex64Encode(name));
            payload.Add("file", Utilities.Hex64Encode(file));

            Uri uri = Utilities.BuildEndpoint(
                this.deviceConnection.Connection,
                WindowsErrorReportingFileApi,
                Utilities.BuildQueryString(payload));

            byte[] werFile = null;
            using (Stream stream = await this.GetAsync(uri))
            {
                using (MemoryStream outStream = new MemoryStream())
                {
                    await outStream.CopyToAsync(outStream);

                    werFile = new byte[outStream.Length];
                    await stream.ReadAsync(werFile, 0, werFile.Length);
                }
            }

            return werFile;
        }

        /// <summary>
        /// Checks if the Windows Error Reporting (WER) APIs are being called on a supported platform.
        /// </summary>
        private void CheckPlatformSupport()
        {
            switch (this.Platform)
            {
                case DevicePortalPlatforms.Mobile:
                case DevicePortalPlatforms.XboxOne:
                    throw new NotSupportedException("This method is only supported on Windows Desktop, HoloLens and IoT platforms.");
            }
        }

        /// <summary>
        /// A list of all files contained within a Windows Error Reporting (WER) report.
        /// </summary>
        [DataContract]
        public class WerFiles
        {
            /// <summary>
            /// Gets a list of all files contained within a Windows Error Reporting (WER) report.
            /// </summary>
            [DataMember(Name = "Files")]
            public List<WerFileInformation> Files { get; private set; }
        }

        /// <summary>
        /// Information about a Windows Error Reporting (WER) report file.
        /// </summary>
        [DataContract]
        public class WerFileInformation
        {
            /// <summary>
            /// Gets the name of the file.
            /// </summary>
            [DataMember(Name = "Name")]
            public string Name { get; private set; }

            /// <summary>
            /// Gets the size of the file (in bytes).
            /// </summary>
            [DataMember(Name = "Size")]
            public int Size { get; private set; }
        }

        /// <summary>
        /// A list of all Windows Error Reporting (WER) reports on a device.
        /// </summary>
        [DataContract]
        public class WerDeviceReports
        {
            /// <summary>
            ///  Gets a list of all Windows Error Reporting (WER) reports on a 
            ///  device.  The SYSTEM user account usually holds the bulk of the 
            ///  error reports. 
            /// </summary>
            [DataMember(Name = "WerReports")]
            public List<WerUserReports> UserReports { get; private set; }

            /// <summary>
            /// Gets system error reports - Convenience accessor for the System error reports - this is 
            /// where most error reports end up. 
            /// </summary>
            public WerUserReports SystemErrorReports
            {
                get
                {
                    return this.UserReports.First(x => x.UserName == "SYSTEM");
                }
            }
        }

        /// <summary>
        /// A list of Windows Error Reporting (WER) reports for a specific user.
        /// </summary>
        [DataContract]
        public class WerUserReports
        {
            /// <summary>
            /// Gets the user name.
            /// </summary>
            [DataMember(Name = "User")]
            public string UserName { get; private set; }

            /// <summary>
            /// Gets a list of Windows Error Reporting (WER) reports
            /// </summary>
            [DataMember(Name = "Reports")]
            public List<WerReportInformation> Reports { get; private set; }
        }

        /// <summary>
        /// Information about a Windows Error Reporting (WER) report.
        /// </summary>
        [DataContract]
        public class WerReportInformation
        {
            /// <summary>
            /// Gets the creation time.
            /// </summary>
            [DataMember(Name = "CreationTime")]
            public ulong CreationTime { get; private set; }

            /// <summary>
            /// Gets the report name (not base64 encoded).
            /// </summary>
            [DataMember(Name = "Name")]
            public string Name { get; private set; }

            /// <summary>
            /// Gets the report type ("Queue" or "Archive").
            /// </summary>
            [DataMember(Name = "Type")]
            public string Type { get; private set; }
        }
    }
}
