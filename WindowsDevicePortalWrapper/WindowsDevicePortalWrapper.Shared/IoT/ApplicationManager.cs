//----------------------------------------------------------------------------------------------
// <copyright file="ApplicationManager.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <content>
    /// Wrappers for Application Management.
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// IoT device application list API.
        /// </summary>
        public static readonly string AppsListApi = "api/iot/appx/default";

        /// <summary>
        /// IoT device headless application list API.
        /// </summary>
        public static readonly string HeadlessAppsListApi = "api/iot/appx/listHeadlessApps";

        /// <summary>
        /// IoT device headless startup application API.
        /// </summary>
        public static readonly string HeadlessStartupAppApi = "api/iot/appx/startupHeadlessApp";

        /// <summary>
        /// IoT device package activation API.
        /// </summary>
        public static readonly string ActivatePackageApi = "api/iot/appx/app";

        /// <summary>
        /// Gets List of apps.
        /// </summary>
        /// <returns>Object containing the list of applications.</returns>
        public async Task<AppsListInfo> GetAppsListInfoAsync()
        {
            return await this.GetAsync<AppsListInfo>(AppsListApi);
        }

        /// <summary>
        /// Gets list of headless apps.
        /// </summary>
        /// <returns>Object containing the list of headless applications.</returns>
        public async Task<HeadlessAppsListInfo> GetHeadlessAppsListInfoAsync()
        {
            return await this.GetAsync<HeadlessAppsListInfo>(HeadlessAppsListApi);
        }

        /// <summary>
        /// Sets selected app as the startup app.
        /// </summary>
        /// <param name="appId">App Id.</param>
        /// <returns>Task tracking completion of the REST call.</returns>
        public async Task UpdateStartupAppAsync(string appId)
        {
            await this.PostAsync(
                 AppsListApi,
                string.Format("appid={0}", Utilities.Hex64Encode(appId)));
        }

        /// <summary>
        /// Sets the selected app as the headless startup app.
        /// </summary>
        /// <param name="appId">App Id.</param>
        /// <returns>Task tracking completion of the REST call.</returns>
        public async Task UpdateHeadlessStartupAppAsync(string appId)
        {
            await this.PostAsync(
                 HeadlessStartupAppApi,
                string.Format("appid={0}", Utilities.Hex64Encode(appId)));
        }

        /// <summary>
        /// Removes the selected app from the headless startup app list.
        /// </summary>
        /// <param name="appId">App Id.</param>
        /// <returns>Task tracking completion of the REST call.</returns>
        public async Task RemoveHeadlessStartupAppAsync(string appId)
        {
            await this.DeleteAsync(
                 HeadlessStartupAppApi,
                string.Format("appid={0}", Utilities.Hex64Encode(appId)));
        }

        /// <summary>
        /// Activiates the selected app package.
        /// </summary>
        /// <param name="appId">App Id.</param>
        /// <returns>Task tracking completion of the REST call.</returns>
        public async Task ActivatePackageAsync(string appId)
        {
            await this.PostAsync(
                 ActivatePackageApi,
                string.Format("appid={0}", Utilities.Hex64Encode(appId)));
        }
        #region Data contract

        /// <summary>
        /// Application list info.
        /// </summary>
        [DataContract]
        public class AppsListInfo
        {
            /// <summary>
            /// Gets the default application 
            /// </summary>
            [DataMember(Name = "DefaultApp")]
            public string DefaultApp { get; private set; }

            /// <summary>
            /// Gets the application packages
            /// </summary>
            [DataMember(Name = "AppPackages")]
            public List<AppPackage> AppPackages { get; private set; }
        }

        /// <summary>
        /// Application package.
        /// </summary>
        [DataContract]
        public class AppPackage
        {
            /// <summary>
            /// Gets a value indicating whether the app is the startup app
            /// </summary>
            [DataMember(Name = "IsStartup")]
            public bool IsStartup { get; private set; }

            /// <summary>
            /// Gets the complate package name
            /// </summary>
            [DataMember(Name = "PackageFullName")]
            public string PackageFullName { get; private set; }
        }

        /// <summary>
        /// Headless app list information.
        /// </summary>
        [DataContract]
        public class HeadlessAppsListInfo
        {
            /// <summary>
            /// Gets the list of headless application packages
            /// </summary>
            [DataMember(Name = "AppPackages")]
            public List<AppPackage> AppPackages { get; private set; }
        }
       
        #endregion // Data contract
    }
}
