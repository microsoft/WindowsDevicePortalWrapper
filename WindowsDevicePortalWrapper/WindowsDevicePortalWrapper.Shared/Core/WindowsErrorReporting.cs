//----------------------------------------------------------------------------------------------
// <copyright file="WindowsErrorReporting.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <content>
    /// Wrappers for DNS methods
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
    }
}
