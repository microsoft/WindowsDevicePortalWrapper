//----------------------------------------------------------------------------------------------
// <copyright file="AppCrashDumpCollection.cs" company="Microsoft Corporation">
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
    /// Wrappers for app crash dump collection methods.
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// API to retrieve list of the available crash dumps (for sideloaded applications).
        /// </summary>
        public static readonly string AvailableCrashDumpsApi = "api/debug/dump/usermode/dumps";

        /// <summary>
        /// API to download or delete a crash dump file (for a sideloaded application).
        /// </summary>
        public static readonly string CrashDumpFileApi = "api/debug/dump/usermode/crashdump";

        /// <summary>
        /// API to control the crash dump settings for a sideloaded application.
        /// </summary>
        public static readonly string CrashDumpSettingsApi = "api/debug/dump/usermode/crashcontrol";

        /// <summary>
        /// Get a list of app crash dumps on the device. 
        /// </summary>
        /// <returns>List of AppCrashDump objects, which represent crashdumps on the device. </returns>
        public async Task<List<AppCrashDump>> GetAppCrashDumpListAsync()
        {
            AppCrashDumpList cdl = await this.GetAsync<AppCrashDumpList>(AvailableCrashDumpsApi);
            return cdl.CrashDumps;
        }

        /// <summary>
        /// Download a sideloaded app's crash dump.  
        /// </summary>
        /// <param name="crashdump"> The AppCrashDump to download</param>
        /// <returns>Stream of the crash dump</returns>
        public async Task<Stream> GetAppCrashDumpAsync(AppCrashDump crashdump)
        {
            string queryString = CrashDumpFileApi + string.Format("?packageFullName={0}&fileName={1}", crashdump.PackageFullName, crashdump.Filename);
            Uri uri = Utilities.BuildEndpoint(
                this.deviceConnection.Connection,
                queryString);

            return await this.GetAsync(uri);
        }

        /// <summary>
        /// Delete an app crash dump stored on the device. 
        /// </summary>
        /// <param name="crashdump">The crashdump to be deleted</param>
        /// <returns>Task tracking completion of the request.</returns>
        public async Task DeleteAppCrashDumpAsync(AppCrashDump crashdump)
        {
            await this.DeleteAsync(
                CrashDumpFileApi,
                string.Format("packageFullName={0}&fileName={1}", crashdump.PackageFullName, crashdump.Filename));
        }

        /// <summary>
        /// Get the crash settings for a sideloaded app. 
        /// </summary>
        /// <param name="app">The app to get settings for</param>
        /// <returns>The crash settings for the app</returns>
        public async Task<AppCrashDumpSettings> GetAppCrashDumpSettingsAsync(AppPackage app)
        {
            return await this.GetAppCrashDumpSettingsAsync(app.PackageFullName);
        }

        /// <summary>
        /// Get the crash settings for a sideloaded app. 
        /// </summary>
        /// <param name="packageFullname">The app to get settings for</param>
        /// <returns>The crash settings for the app</returns>
        public async Task<AppCrashDumpSettings> GetAppCrashDumpSettingsAsync(string packageFullname)
        {
            return await this.GetAsync<AppCrashDumpSettings>(
                CrashDumpSettingsApi,
                string.Format("packageFullName={0}", packageFullname));
        }

        /// <summary>
        /// Set the crash settings for a sideloaded app. 
        /// </summary>
        /// <param name="app">The app to set crash settings for.</param>
        /// <param name="enable">Whether to enable or disable crash collection for the app. </param>
        /// <returns>Task tracking completion of the request.</returns>
        public async Task SetAppCrashDumpSettingsAsync(AppPackage app, bool enable = true)
        {
            string pfn = app.PackageFullName;
            await this.SetAppCrashDumpSettingsAsync(pfn, enable);
        }

        /// <summary>
        /// Set the crash settings for a sideloaded app. 
        /// </summary>
        /// <param name="packageFullName">The app to set crash settings for.</param>
        /// <param name="enable">Whether to enable or disable crash collection for the app. </param>
        /// <returns>Task tracking completion of the request.</returns>
        public async Task SetAppCrashDumpSettingsAsync(string packageFullName, bool enable = true)
        {
            if (enable)
            {
                await this.PostAsync(
                    CrashDumpSettingsApi,
                string.Format("packageFullName={0}", packageFullName));
            }
            else
            {
                await this.DeleteAsync(
                    CrashDumpSettingsApi,
                string.Format("packageFullName={0}", packageFullName));
            }
        }

        #region Data contract

        /// <summary>
        /// Per-app crash dump settings.
        /// </summary>
        [DataContract]
        public class AppCrashDumpSettings
        {
            /// <summary>
            /// Gets a value indicating whether crash dumps are enabled for the app
            /// </summary>
            [DataMember(Name = "CrashDumpEnabled")]
            public bool CrashDumpEnabled
            {
                get;
                private set;
            }
        }

        /// <summary>
        /// Represents a crash dump collected from a sideloaded app. 
        /// </summary>
        [DataContract]
        public class AppCrashDump 
        {
            /// <summary>
            /// Gets the timestamp of the crash as a string.
            /// </summary>
            [DataMember(Name = "FileDate")]
            public string FileDateAsString
            {
                get;
                private set;
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
                get;
                private set;
            }

            /// <summary>
            /// Gets the size of the crash dump, in bytes
            /// </summary>
            [DataMember(Name = "FileSize")]
            public uint FileSizeInBytes
            {
                get;
                private set;
            }

            /// <summary>
            /// Gets the package full name of the app that crashed. 
            /// </summary>
            [DataMember(Name = "PackageFullName")]
            public string PackageFullName
            {
                get;
                private set;
            }
        }

        /// <summary>
        /// A list of crash dumps.  Internal usage only. 
        /// </summary>
        [DataContract]
        private class AppCrashDumpList
        {
            /// <summary>
            /// Gets a list of crash dumps on the device. 
            /// </summary>
            [DataMember(Name = "CrashDumps")]
            public List<AppCrashDump> CrashDumps
            {
                get;
                private set;
            }
        }
        #endregion Data contract
    }
}
