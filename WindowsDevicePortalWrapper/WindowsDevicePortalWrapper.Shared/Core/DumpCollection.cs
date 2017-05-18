//----------------------------------------------------------------------------------------------
// <copyright file="DumpCollection.cs" company="Microsoft Corporation">
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
    /// Wrappers for crash dump collection methods.
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// API to retrieve list of the available bugcheck minidumps.
        /// </summary>
        public static readonly string AvailableBugChecksApi = "api/debug/dump/kernel/dumplist";

        /// <summary>
        /// API to download a bugcheck minidump file.
        /// </summary>
        public static readonly string BugcheckFileApi = "api/debug/dump/kernel/dump";

        /// <summary>
        /// API to control bugcheck minidump settings.
        /// </summary>
        public static readonly string BugcheckSettingsApi = "api/debug/dump/kernel/crashcontrol";

        /// <summary>
        /// API to retrieve a live kernel dump.
        /// </summary>
        public static readonly string LiveKernelDumpApi = "api/debug/dump/livekernel";

        /// <summary>
        /// API to retrieve a live dump from a running user mode process.
        /// </summary>
        public static readonly string LiveProcessDumpApi = "api/debug/dump/usermode/live";

        /// <summary>
        /// Get a list of dumpfiles on the system. 
        /// </summary>
        /// <returns>List of Dumpfiles.  Use GetDumpFile to download the file. </returns>
        public async Task<List<Dumpfile>> GetDumpfileListAsync()
        {
            DumpFileList dfl = await this.GetAsync<DumpFileList>(AvailableBugChecksApi);
            return dfl.DumpFiles;
        }

        /// <summary>
        /// Download a dumpfile from the system. 
        /// </summary>
        /// <param name="crashdump"> Dumpfile object to be downloaded</param>
        /// <returns>Stream of the dump file </returns>
        public async Task<Stream> GetDumpFileAsync(Dumpfile crashdump)
        {
            string queryString = BugcheckFileApi + string.Format("?filename={0}", crashdump.Filename);
            Uri uri = Utilities.BuildEndpoint(
                this.deviceConnection.Connection,
                queryString);

            return await this.GetAsync(uri);
        }

        /// <summary>
        /// Takes a live kernel dump of the device.  This does not reboot the device. 
        /// </summary>
        /// <returns>Stream of the kernel dump</returns>
        public async Task<Stream> GetLiveKernelDumpAsync()
        {
            Uri uri = Utilities.BuildEndpoint(
                this.deviceConnection.Connection,
                LiveKernelDumpApi);

            return await this.GetAsync(uri);
        }

        /// <summary>
        /// Take a live dump of a process on the system. 
        /// </summary>
        /// <param name="pid"> PID of the process to get a live dump of.</param>
        /// <returns>Stream of the process live dump</returns>
        public async Task<Stream> GetLiveProcessDumpAsync(int pid)
        {
            string queryString = LiveProcessDumpApi + string.Format("?pid={0}", pid);
            Uri uri = Utilities.BuildEndpoint(
                this.deviceConnection.Connection,
                queryString);

            return await this.GetAsync(uri);
        }

        /// <summary>
        /// Get information on how and when dump files are saved on the device. 
        /// </summary>
        /// <returns>Dumpfile settings object.  This can be edited and returned to the device to alter the settings.</returns>
        public async Task<DumpFileSettings> GetDumpFileSettingsAsync()
        {
            return await this.GetAsync<DumpFileSettings>(BugcheckSettingsApi);
        }

        /// <summary>
        /// Set how and when dump files are saved on the device. 
        /// </summary>
        /// <param name="dfs">Altered DumpFileSettings object to set on the device</param>
        /// <returns>Task tracking completion of the request</returns>
        public async Task SetDumpFileSettingsAsync(DumpFileSettings dfs)
        {
            string queryParams = string.Format(
                    "autoreboot={0}&overwrite={1}&dumptype={2}&maxdumpcount={3}",
                    dfs.AutoReboot ? "1" : "0",
                    dfs.Overwrite ? "1" : "0",
                    (int)dfs.DumpType,
                    dfs.MaxDumpCount);

            await this.PostAsync(BugcheckSettingsApi, queryParams);
        }

        #region Data Contract

        /// <summary>
        /// DumpFileSettings object.  Used to get and set how and when a dump is saved on the device. 
        /// </summary>
        [DataContract]
        public class DumpFileSettings
        {
            /// <summary>
            /// The 3 types of dumps  that can be saved on the device (or not saved at all). 
            /// </summary>
            public enum DumpTypes
            {
                /// <summary>
                /// Don't collect device crash dumps
                /// </summary>
                Disabled = 0,

                /// <summary>
                /// Collect all in use memory
                /// </summary>
                CompleteMemoryDump = 1,

                /// <summary>
                /// Don't include usermode memory in the dump
                /// </summary>
                KernelDump = 2,

                /// <summary>
                /// Limited kernel dump
                /// </summary>
                Minidump = 3
            }

            /// <summary>
            /// Gets or sets a value indicating whether the device should restart after a crash dump is taken.
            /// </summary>
            [DataMember(Name = "autoreboot")]
            public bool AutoReboot { get; set; }

            /// <summary>
            /// Gets or sets the type of dump to be saved when a bugcheck occurs.  
            /// </summary>
            [DataMember(Name = "dumptype")]
            public DumpTypes DumpType { get; set; }

            /// <summary>
            /// Gets or sets the max number of dumps to be saved on the device. 
            /// </summary>
            [DataMember(Name = "maxdumpcount")]
            public int MaxDumpCount { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether new dumps should overwrite older dumps. 
            /// </summary>
            [DataMember(Name = "overwrite")]
            public bool Overwrite { get; set; }
        }

        /// <summary>
        /// Gets a list of kernel dumps on the device. 
        /// </summary>
        [DataContract]
        public class DumpFileList
        {
            /// <summary>
            /// Gets a list of kernel dumps on the device. 
            /// </summary>
            [DataMember(Name = "DumpFiles")]
            public List<Dumpfile> DumpFiles { get; private set; }
        }

        /// <summary>
        /// Represents a dumpfile stored on the device. 
        /// </summary>
        [DataContract]
        public class Dumpfile
        {
            /// <summary>
            /// Gets the timestamp of the crash as a string.
            /// </summary>
            [DataMember(Name = "FileDate")]
            public string FileDateAsString
            {
                get; private set;
            }

            /// <summary>
            /// Gets the timestamp of the crash.
            /// </summary>
            public DateTime FileDate
            {
                get
                {
                    return DateTime.Parse(this.FileDateAsString);
                }
            }

            /// <summary>
            /// Gets the filename of the crash file. 
            /// </summary>
            [DataMember(Name = "FileName")]
            public string Filename
            {
                get; private set;
            }

            /// <summary>
            /// Gets the size of the crash dump, in bytes
            /// </summary>
            [DataMember(Name = "FileSize")]
            public uint FileSizeInBytes
            {
                get; private set;
            }
        }
        #endregion Data Contract
    }
}
