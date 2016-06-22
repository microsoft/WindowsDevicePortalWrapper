//----------------------------------------------------------------------------------------------
// <copyright file="TaskManager.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsDevicePortal
{
    using System;
    using System.Threading.Tasks;

    /// <content>
    /// Wrappers for Task Manager methods
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// API for managing app state
        /// </summary>
        private static readonly string TaskManagerApi = "api/taskmanager/app";

        /// <summary>
        /// Starts running the specified application.
        /// </summary>
        /// <param name="appid">Application ID</param>
        /// <param name="packageName">The name of the application package.</param>
        /// <returns>Process identifier for the application instance.</returns>
        public async Task<int> LaunchApplication(
            string appid,
            string packageName)
        {
            string payload = string.Format(
                "appid={0}&package={1}",
                Utilities.Hex64Encode(appid), 
                Utilities.Hex64Encode(packageName));

            await Post(
                TaskManagerApi, 
                payload);

            RunningProcesses runningApps = await GetRunningProcesses();

            int processId = 0;
            foreach (DeviceProcessInfo process in runningApps.Processes)    
            {
                if (string.Compare(process.PackageFullName, packageName) == 0)
                {
                    processId = process.ProcessId;
                    break;
                }
            }

            return processId;
        }

        /// <summary>
        /// Stops the specified application from running.
        /// </summary>
        /// <param name="packageName">The name of the application package.</param>
        /// <returns>
        /// Task for tracking termination completion
        /// </returns>
        public async Task TerminateApplication(string packageName)
        {
            await Delete(
                TaskManagerApi,
                string.Format("package={0}", Utilities.Hex64Encode(packageName)));
        }
    }
}
