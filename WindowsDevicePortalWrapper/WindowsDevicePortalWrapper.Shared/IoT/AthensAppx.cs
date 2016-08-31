//----------------------------------------------------------------------------------------------
// <copyright file="AthensAppx.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal
{

    public partial class DevicePortal
    {
        public static readonly string AppsListApi = "api/iot/appx/default";
        public static readonly string HeadlessAppsListApi = "api/iot/appx/listHeadlessApps";
        public static readonly string HeadlessStartupAppApi = "api/iot/appx/startupHeadlessApp";
        public static readonly string ActivatePackageApi = "api/iot/appx/app";

        /// <summary>
        /// Gets List of apps.
        /// </summary>
        public async Task<AppsListInfo> GetAppsListInfo()
        {
            return await this.Get<AppsListInfo>(AppsListApi);
        }

       /// <summary>
        /// Gets list of headless apps.
        /// </summary>
        public async Task<HeadlessAppsListInfo> GetHeadlessAppListInfo()
        {
            return await this.Get<HeadlessAppsListInfo>(HeadlessAppsListApi);
        }
        
        /// <summary>
        /// Sets selected app as the startup app.
        /// </summary>
        public async Task UpdateStartupApp(string appId)
        {
            await this.Post(
                 AppsListApi,
                string.Format("appid={0}", Utilities.Hex64Encode(appId)));
        }
        
        /// <summary>
        /// Sets the selected app as the headless startup app.
        /// </summary>
        public async Task UpdateHeadlessStartupApp(string appId)
        {
            await this.Post(
                 HeadlessStartupAppApi,
                string.Format("appid={0}", Utilities.Hex64Encode(appId)));
        }

        /// <summary>
        /// Removes the selected app from the headless startup app list.
        /// </summary>
        public async Task RemoveHeadlessStartupApp(string appId)
        {
            await this.Delete(
                 HeadlessStartupAppApi,
                string.Format("appid={0}", Utilities.Hex64Encode(appId)));
        }

        /// <summary>
        /// Activiates the selected app package.
        /// </summary>
        public async Task ActivatePackage(string appId)
        {
            await this.Delete(
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
            /// Gets or sets the default application 
            /// </summary>
            [DataMember(Name = "DefaultApp")]
            public string DefaultApp { get; set; }

            /// <summary>
            /// Gets or sets the application packages
            /// </summary>
            [DataMember(Name = "AppPackages")]
            public AppPackage[] AppPackages { get; set; }

        }
        [DataContract]
        public partial class AppPackage
        {
            /// <summary>
            /// Gets or sets an app as the startup app
            /// </summary>
            [DataMember(Name = "IsStartup")]
            public bool IsStartup { get; set; }

            /// <summary>
            /// Gets the complate package name
            /// </summary>
            [DataMember(Name = "PackageFullName")]
            public string PackageFullName { get; set; }
        }

        /// <summary>
        /// Headless app list information.
        /// </summary>
        [DataContract]
        public class HeadlessAppsListInfo
        {
            /// <summary>
            /// Gets or sets the list of headless application packages
            /// </summary>
            [DataMember(Name = "AppPackages")]
            public AppPackage[] AppPackages { get; set; }
        }
       
        #endregion // Data contract
    }
}
