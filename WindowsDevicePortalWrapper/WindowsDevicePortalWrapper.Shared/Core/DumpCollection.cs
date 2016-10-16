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

        public async Task<List<Dumpfile>> GetDumpfileListAsync()
        {
            DumpFileList dfl = await this.GetAsync<DumpFileList>(AvailableBugChecksApi);
            return dfl.DumpFiles;
        }

        public async Task<Stream> GetDumpFileAsync(Dumpfile crashdump)
        {
            string queryString = BugcheckFileApi + string.Format("?filename={0}", crashdump.Filename);
            Uri uri = Utilities.BuildEndpoint(
                this.deviceConnection.Connection,
                queryString);

            return await this.GetAsync(uri);
        }

        public async Task<Stream> GetLiveKernelDumpAsync()
        {
            Uri uri = Utilities.BuildEndpoint(
                this.deviceConnection.Connection,
                LiveKernelDumpApi);

            return await this.GetAsync(uri);
        }

        public async Task<Stream> GetLiveProcessDumpAsync(int pid)
        {
            string queryString = LiveProcessDumpApi + string.Format("?pid={0}", pid);
            Uri uri = Utilities.BuildEndpoint(
                this.deviceConnection.Connection,
                queryString);

            return await this.GetAsync(uri);
        }

        public async Task<DumpFileSettings> GetDumpFileSettingsAsync()
        {
            return await this.GetAsync<DumpFileSettings>(BugcheckSettingsApi);
        }

        public async Task SetDumpFileSettingsAsync(DumpFileSettings dfs)
        {
            await this.PostAsync(
                BugcheckSettingsApi,
                string.Format(
                    "autoreboot={0}&overwrite={1}&dumptype={2}&maxdumpcount={3}",
                    dfs.AutoReboot?"1":"0", dfs.Overwrite ? "1" : "0", (int)dfs.DumpType, dfs.MaxDumpCount));
        }
    }

    #region Data Contract
    [DataContract]
    public class DumpFileSettings
    {
        [DataMember(Name = "autoreboot")]
        public bool AutoReboot { get; set; }

        [DataMember(Name = "dumptype")]
        public DumpTypes DumpType { get; set; }

        [DataMember(Name = "maxdumpcount")]
        public int MaxDumpCount { get; set; }

        [DataMember(Name = "overwrite")]
        public bool Overwrite { get; set; }

        public enum DumpTypes
        {
            Disabled=0,
            CompleteMemoryDump=1,
            KernelDump=2,
            Minidump=3
        }
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
