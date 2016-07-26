//----------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Tools.WindowsDevicePortal;
using static Microsoft.Tools.WindowsDevicePortal.DevicePortal;

namespace XboxWdpDriver
{
    /// <summary>
    /// Main entry point for the test command line class.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// String listing the available operations.
        /// </summary>
        private static readonly string AvailableOperationsText = "Supported operations are the following:\n" +
                "info\n" +
                "xbluser\n" +
                "install\n" +
                "reboot\n" +
                "processes\n" +
                "systemPerf\n" +
                "config\n" +
                "file";

        /// <summary>
        /// Usage string
        /// </summary>
        private static readonly string GeneralUsageMessage = "Usage: /ip:<system-ip or hostname> /user:<WDP username> /pwd:<WDP password> [/op:<operation type> [operation parameters]]";

        /// <summary>
        /// Operation types
        /// </summary>
        private enum OperationType
        {
            /// <summary>
            /// No operation (just connects to the console).
            /// </summary>
            None,

            /// <summary>
            /// Info operation.
            /// </summary>
            InfoOperation,

            /// <summary>
            /// User operation.
            /// </summary>
            UserOperation,

            /// <summary>
            /// Install Appx Package or loose folder operation.
            /// </summary>
            InstallOperation,

            /// <summary>
            /// Reboot console operation.
            /// </summary>
            RebootOperation,

            /// <summary>
            /// List processes operation.
            /// </summary>
            ListProcessesOperation,

            /// <summary>
            /// Get the system performance operation.
            /// </summary>
            GetSystemPerfOperation,

            /// <summary>
            /// Get or set Xbox Settings.
            /// </summary>
            XboxSettings,

            /// <summary>
            /// Does remote file operations.
            /// </summary>
            FileOperation,
        }

        /// <summary>
        /// Main entry point
        /// </summary>
        /// <param name="args">command line args</param>
        public static void Main(string[] args)
        {
            ParameterHelper parameters = new ParameterHelper();
            Program app = new Program();

            try
            {
                parameters.ParseCommandLine(args);

                OperationType operation = OperationType.None;

                if (parameters.HasParameter(ParameterHelper.Operation))
                {
                    operation = OperationStringToEnum(parameters.GetParameterValue("op"));
                }

                if (!parameters.HasParameter(ParameterHelper.IpOrHostname) || !parameters.HasParameter(ParameterHelper.WdpUser) || !parameters.HasParameter(ParameterHelper.WdpPassword))
                {
                    throw new Exception("Missing one or more required parameter(s). Must provide ip, user, and pwd");
                }

                bool listen = false;
                if (parameters.HasParameter(ParameterHelper.Listen))
                {
                    bool parsedValue = false;
                    if (bool.TryParse(parameters.GetParameterValue(ParameterHelper.Listen), out parsedValue))
                    {
                        listen = parsedValue;
                    }
                }

                DevicePortal portal = new DevicePortal(new DevicePortalConnection(parameters.GetParameterValue(ParameterHelper.IpOrHostname), parameters.GetParameterValue(ParameterHelper.WdpUser), parameters.GetParameterValue(ParameterHelper.WdpPassword)));

                Task connectTask = portal.Connect(updateConnection: false);
                connectTask.Wait();

                if (portal.ConnectionHttpStatusCode != HttpStatusCode.OK)
                {
                    if (portal.ConnectionHttpStatusCode != 0)
                    {
                        Console.WriteLine(string.Format("Failed to connect to WDP with HTTP Status code: {0}", portal.ConnectionHttpStatusCode));
                    }
                    else
                    {
                        Console.WriteLine("Failed to connect to WDP for unknown reason.");
                    }
                }
                else if (operation == OperationType.InfoOperation)
                {
                    Console.WriteLine("OS version: " + portal.OperatingSystemVersion);
                    Console.WriteLine("Platform: " + portal.PlatformName + " (" + portal.Platform.ToString() + ")");

                    Task<string> getNameTask = portal.GetDeviceName();
                    getNameTask.Wait();
                    Console.WriteLine("Device name: " + getNameTask.Result);
                }
                else if (operation == OperationType.UserOperation)
                {
                    UserOperation.HandleOperation(portal, parameters);
                }
                else if (operation == OperationType.InstallOperation)
                {
                    InstallOperation.HandleOperation(portal, parameters);
                }
                else if (operation == OperationType.RebootOperation)
                {
                    Task rebootTask = portal.Reboot();
                    rebootTask.Wait();
                    Console.WriteLine("Rebooting device.");
                }
                else if (operation == OperationType.ListProcessesOperation)
                {
                    RunningProcesses runningProcesses = null;
                    if (listen)
                    {
                        ManualResetEvent runningProcessesReceived = new ManualResetEvent(false);

                        WebSocketMessageReceivedEventHandler<RunningProcesses> runningProcessesReceivedHandler =
                            delegate(object sender, WebSocketMessageReceivedEventArgs<RunningProcesses> runningProccesesArgs)
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
                else if (operation == OperationType.GetSystemPerfOperation)
                {
                    SystemPerformanceInformation systemPerformanceInformation = null;
                    if (listen)
                    {
                        ManualResetEvent systemPerfReceived = new ManualResetEvent(false);

                        WebSocketMessageReceivedEventHandler<SystemPerformanceInformation> systemPerfReceivedHandler =
                            delegate(object sender, WebSocketMessageReceivedEventArgs<SystemPerformanceInformation> sysPerfInfoArgs)
                        {
                            if (sysPerfInfoArgs.Message != null)
                            {
                                systemPerformanceInformation = sysPerfInfoArgs.Message;
                                systemPerfReceived.Set();
                            }
                        };

                        portal.SystemPerfMessageReceived += systemPerfReceivedHandler;

                        Task startListeningForSystemPerfTask = portal.StartListeningForSystemPerf();
                        startListeningForSystemPerfTask.Wait();

                        systemPerfReceived.WaitOne();

                        Task stopListeningForSystemPerfTask = portal.StopListeningForRunningProcesses();
                        stopListeningForSystemPerfTask.Wait();

                        portal.SystemPerfMessageReceived -= systemPerfReceivedHandler;
                    }
                    else
                    {
                        Task<SystemPerformanceInformation> getRunningProcessesTask = portal.GetSystemPerf();
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
                else if (operation == OperationType.XboxSettings)
                {
                    SettingOperation.HandleOperation(portal, parameters);
                }
                else if (operation == OperationType.FileOperation)
                {
                    FileOperation.HandleOperation(portal, parameters);
                }
                else
                {
                    Console.WriteLine("Successfully connected to console but no operation was specified. \n" +
                        "Use the '/op:<operation type>' parameter to run a specified operation.");
                    Console.WriteLine();
                    Console.WriteLine(AvailableOperationsText);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine();
                Console.WriteLine(GeneralUsageMessage);
            }

            // If a debugger is attached, don't close but instead loop here until
            // closed.
            while (Debugger.IsAttached)
            {
                Thread.Sleep(0);
            }
        }

        /// <summary>
        /// Helper for converting from operation string to enum
        /// </summary>
        /// <param name="operation">string representation of the operation type.</param>
        /// <returns>enum representation of the operation type.</returns>
        private static OperationType OperationStringToEnum(string operation)
        {
            if (operation.Equals("info", StringComparison.OrdinalIgnoreCase))
            {
                return OperationType.InfoOperation;
            }
            else if (operation.Equals("xbluser", StringComparison.OrdinalIgnoreCase))
            {
                return OperationType.UserOperation;
            }
            else if (operation.Equals("install", StringComparison.OrdinalIgnoreCase))
            {
                return OperationType.InstallOperation;
            }
            else if (operation.Equals("reboot", StringComparison.OrdinalIgnoreCase))
            {
                return OperationType.RebootOperation;
            }
            else if (operation.Equals("processes", StringComparison.OrdinalIgnoreCase))
            {
                return OperationType.ListProcessesOperation;
            }
            else if (operation.Equals("systemPerf", StringComparison.OrdinalIgnoreCase))
            {
                return OperationType.GetSystemPerfOperation;
            }
            else if (operation.Equals("config", StringComparison.OrdinalIgnoreCase))
            {
                return OperationType.XboxSettings;
            }
            else if (operation.Equals("file", StringComparison.OrdinalIgnoreCase))
            {
                return OperationType.FileOperation;
            }

            throw new Exception("Unknown Operation Type. " + AvailableOperationsText);
        }
    }
}
