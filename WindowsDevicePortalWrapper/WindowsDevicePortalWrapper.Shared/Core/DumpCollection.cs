//----------------------------------------------------------------------------------------------
// <copyright file="DumpCollection.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <content>
    /// Wrappers for crash dump collection methods.
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// API to retrieve list of the available bugcheck minidumps.
        /// </summary>
        public static readonly string AvailableBugChecksApi = "api/debug/dump/kernel/dumplish";

        /// <summary>
        /// API to retrieve list of the available crash dumps (for sideloaded applications).
        /// </summary>
        public static readonly string AvailableCrashDumpsApi = "api/debug/dump/usermode/dumps";

        /// <summary>
        /// API to download a bugcheck minidump file.
        /// </summary>
        public static readonly string BugcheckFileApi = "api/debug/dump/kernel/dump";

        /// <summary>
        /// API to control bugcheck minidump settings.
        /// </summary>
        public static readonly string BugcheckSettingsApi = "api/debug/dump/kernel/crashcontrol";

        /// <summary>
        /// API to download or delete a crash dump file (for a sideloaded application).
        /// </summary>
        public static readonly string CrashDumpFileApi = "api/debug/dump/usermode/crashdump";

        /// <summary>
        /// API to control the crash dump settings for a sideloaded application.
        /// </summary>
        public static readonly string CrashDumpSettingsApi = "api/debug/dump/usermode/crashcontrol";

        /// <summary>
        /// API to retrieve a live kernel dump.
        /// </summary>
        public static readonly string LiveKernelDumpApi = "api/debug/dump/livekernel";

        /// <summary>
        /// API to retrieve a live dump from a running user mode process.
        /// </summary>
        public static readonly string LiveProcessDumpApi = "api/debug/dump/usermode/live";
    }
}
