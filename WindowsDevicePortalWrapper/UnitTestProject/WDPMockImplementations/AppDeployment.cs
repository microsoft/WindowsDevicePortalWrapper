//----------------------------------------------------------------------------------------------
// <copyright file="AppDeployment.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Tools.WindowsDevicePortal.Tests;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <content>
    /// MOCK implementation of App Deployment methods.
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// API for getting installation status.
        /// </summary>
        /// <returns>The status</returns>
        public async Task<ApplicationInstallStatus> GetInstallStatusAsync()
        {
            ApplicationInstallStatus status = ApplicationInstallStatus.Completed;

            return await Task.FromResult(status);
        }
    }
}
