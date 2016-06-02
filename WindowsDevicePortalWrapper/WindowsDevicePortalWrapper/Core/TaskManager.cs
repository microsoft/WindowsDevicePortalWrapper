// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE.TXT in the project root license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal
{
    public partial class DevicePortal
    {
        private static readonly String _taskManagerApi = "api/taskmanager/app";

        /// <summary>
        /// Starts running the specified application.
        /// </summary>
        /// <param name="appid">Application ID</param>
        /// <param name="packageName">The name of the application package.</param>
        /// <returns>Process identifier for the application instance.</returns>
        public async Task<Int32> LaunchApplication(String appid,
                                                String packageName)
        {
            String payload = String.Format("appid={0}&package={1}", 
                                        Utilities.Hex64Encode(appid), 
                                        Utilities.Hex64Encode(packageName));

            await Post(_taskManagerApi, 
                                payload);

            RunningProcesses runningApps = await GetRunningProcesses();

            Int32 processId = 0;
            foreach (DeviceProcessInfo process in runningApps.Processes)    
            {
                if (0 == String.Compare(process.PackageFullName, packageName))
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
        public async Task TerminateApplication(String packageName)
        {
            await Delete(_taskManagerApi,
                                String.Format("package={0}", Utilities.Hex64Encode(packageName)));
        }

    }
}
