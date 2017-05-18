//----------------------------------------------------------------------------------------------
// <copyright file="SystemPerfOperation.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Tools.WindowsDevicePortal;
using static Microsoft.Tools.WindowsDevicePortal.DevicePortal;

namespace XboxWdpDriver
{
    /// <summary>
    /// Helper for System Perf related operations
    /// </summary>
    public class SystemPerfOperation
    {
        /// <summary>
        /// Main entry point for handling a System Perf operation
        /// </summary>
        /// <param name="portal">DevicePortal reference for communicating with the device.</param>
        /// <param name="parameters">Parsed command line parameters.</param>
        public static void HandleOperation(DevicePortal portal, ParameterHelper parameters)
        {
            SystemPerformanceInformation systemPerformanceInformation = null;
            if (parameters.HasFlag(ParameterHelper.Listen))
            {
                ManualResetEvent systemPerfReceived = new ManualResetEvent(false);

                WebSocketMessageReceivedEventHandler<SystemPerformanceInformation> systemPerfReceivedHandler =
                    delegate(DevicePortal sender, WebSocketMessageReceivedEventArgs<SystemPerformanceInformation> sysPerfInfoArgs)
                    {
                        if (sysPerfInfoArgs.Message != null)
                        {
                            systemPerformanceInformation = sysPerfInfoArgs.Message;
                            systemPerfReceived.Set();
                        }
                    };

                portal.SystemPerfMessageReceived += systemPerfReceivedHandler;

                Task startListeningForSystemPerfTask = portal.StartListeningForSystemPerfAsync();
                startListeningForSystemPerfTask.Wait();

                systemPerfReceived.WaitOne();

                Task stopListeningForSystemPerfTask = portal.StopListeningForRunningProcessesAsync();
                stopListeningForSystemPerfTask.Wait();

                portal.SystemPerfMessageReceived -= systemPerfReceivedHandler;
            }
            else
            {
                Task<SystemPerformanceInformation> getRunningProcessesTask = portal.GetSystemPerfAsync();
                systemPerformanceInformation = getRunningProcessesTask.Result;
            }

            Console.WriteLine("Available Pages: " + systemPerformanceInformation.AvailablePages);
            Console.WriteLine("Commit Limit: " + systemPerformanceInformation.CommitLimit);
            Console.WriteLine("Commited Pages: " + systemPerformanceInformation.CommittedPages);
            Console.WriteLine("CPU Load: " + systemPerformanceInformation.CpuLoad);
            Console.WriteLine("IoOther Speed: " + systemPerformanceInformation.IoOtherSpeed);
            Console.WriteLine("IoRead Speed: " + systemPerformanceInformation.IoReadSpeed);
            Console.WriteLine("IoWrite Speed: " + systemPerformanceInformation.IoWriteSpeed);
            Console.WriteLine("Non-paged Pool Pages: " + systemPerformanceInformation.NonPagedPoolPages);
            Console.WriteLine("Paged Pool Pages: " + systemPerformanceInformation.PagedPoolPages);
            Console.WriteLine("Page Size: " + systemPerformanceInformation.PageSize);
            Console.WriteLine("Total Installed Kb: " + systemPerformanceInformation.TotalInstalledKb);
            Console.WriteLine("Total Pages: " + systemPerformanceInformation.TotalPages);
        }
    }
}