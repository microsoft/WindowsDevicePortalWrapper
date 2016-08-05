//----------------------------------------------------------------------------------------------
// <copyright file="ListProcessesOperation.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Microsoft.Tools.WindowsDevicePortal;
using static Microsoft.Tools.WindowsDevicePortal.DevicePortal;
using System.Threading;

namespace XboxWdpDriver
{
    /// <summary>
    /// Helper for Listing processes
    /// </summary>
    public class ListProcessesOperation
    {
        /// <summary>
        /// Main entry point for handling listing processes
        /// </summary>
        /// <param name="portal">DevicePortal reference for communicating with the device.</param>
        /// <param name="parameters">Parsed command line parameters.</param>
        public static void HandleOperation(DevicePortal portal, ParameterHelper parameters)
        {
            RunningProcesses runningProcesses = null;
            if (parameters.HasFlag(ParameterHelper.Listen))
            {
                ManualResetEvent runningProcessesReceived = new ManualResetEvent(false);

                WebSocketMessageReceivedEventHandler<RunningProcesses> runningProcessesReceivedHandler =
                    delegate (DevicePortal sender, WebSocketMessageReceivedEventArgs<RunningProcesses> runningProccesesArgs)
                    {
                        if (runningProccesesArgs.Message != null)
                        {
                            runningProcesses = runningProccesesArgs.Message;
                            runningProcessesReceived.Set();
                        }
                    };

                portal.RunningProcessesMessageReceived += runningProcessesReceivedHandler;

                Task startListeningForProcessesTask = portal.StartListeningForRunningProcesses();
                startListeningForProcessesTask.Wait();

                runningProcessesReceived.WaitOne();

                Task stopListeningForProcessesTask = portal.StopListeningForRunningProcesses();
                stopListeningForProcessesTask.Wait();

                portal.RunningProcessesMessageReceived -= runningProcessesReceivedHandler;
            }
            else
            {
                Task<DevicePortal.RunningProcesses> getRunningProcessesTask = portal.GetRunningProcesses();
                runningProcesses = getRunningProcessesTask.Result;
            }

            foreach (DeviceProcessInfo process in runningProcesses.Processes)
            {
                if (!string.IsNullOrEmpty(process.Name))
                {
                    Console.WriteLine(process.Name);
                }
            }
        }
    }
}