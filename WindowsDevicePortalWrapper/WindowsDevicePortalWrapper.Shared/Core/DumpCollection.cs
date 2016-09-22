//----------------------------------------------------------------------------------------------
// <copyright file="DumpCollection.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;

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
        public static readonly string AvailableBugChecksApi = "api/debug/dump/kernel/dumplist";

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

        public async Task<CrashDump[]> GetAppCrashDumps()
        {
            CrashDumpList cdl = await this.Get<CrashDumpList>(AvailableCrashDumpsApi);
            return cdl.CrashDumps;
        }

        public async Task<Stream> GetAppCrashDump(CrashDump crashdump)
        {
            string queryString = CrashDumpFileApi + string.Format("packageFullname={0}&fileName={1}", crashdump.PackageFullName, crashdump.Filename);
            Uri uri = Utilities.BuildEndpoint(
                this.deviceConnection.Connection,
                queryString);

            return await this.Get(uri);
        }

        public async Task DeleteAppCrashDump(CrashDump crashdump)
        {
            await this.Delete(CrashDumpFileApi,
                string.Format("packageFullname={0}&fileName={1}", crashdump.PackageFullName, crashdump.Filename));
        }

        public async Task<CrashDumpSettings> GetAppCrashDumpSettings(AppPackage app)
        {
            return await this.GetAppCrashDumpSettings(app.PackageFullName);
        }

        public async Task<CrashDumpSettings> GetAppCrashDumpSettings(string packageFullname)
        {
            return await this.Get<CrashDumpSettings>(
                CrashDumpSettingsApi,
                string.Format("packageFullname={0}", packageFullname));
        }

        public async Task SetAppCrashDumpSettings(AppPackage app, bool enable = true)
        {
            string pfn = app.PackageFullName;
            if (enable)
            {
                await this.Post(
                    CrashDumpSettingsApi,
                string.Format("packageFullname={0}", pfn));
            } else
            {
                await this.Delete(
                    CrashDumpSettingsApi,
                string.Format("packageFullname={0}", pfn));
            }
        }

        #region Data contract

        /// <summary>
        /// Per-app crash dump settings.
        /// </summary>
        [DataContract]
        public class CrashDumpSettings
        {
            /// <summary>
            /// Gets whether crash dumps are enabled for the app
            /// </summary>
            [DataMember(Name = "CrashDumpEnabled")]
            public bool CrashDumpEnabled
            {
                get; private set;
            }
        }


        public class CrashDump
        {
            /// <summary>
            /// Gets whether crash dumps are enabled for the app
            /// </summary>
            [DataMember(Name = "FileDate")]
            public DateTime FileDate
            {
                get; private set;
            }

            /// <summary>
            /// Gets whether crash dumps are enabled for the app
            /// </summary>
            [DataMember(Name = "FileName")]
            public string Filename
            {
                get; private set;
            }

            /// <summary>
            /// Gets whether crash dumps are enabled for the app
            /// </summary>
            [DataMember(Name = "FileSize")]
            public int FileSizeInBytes
            {
                get; private set;
            }

            /// <summary>
            /// Gets whether crash dumps are enabled for the app
            /// </summary>
            [DataMember(Name = "PackageFullName")]
            public bool PackageFullName
            {
                get; private set;
            }
}

        private class CrashDumpList
        {
            [DataMember(Name = "CrashDumps")]
            public CrashDump[] CrashDumps
            {
                get; private set;
            }
        }
        #endregion Data contract
    }
}
