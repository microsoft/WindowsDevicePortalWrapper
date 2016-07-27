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
                "file\n" +
                "connect";

        /// <summary>
        /// Usage string
        /// </summary>
        private static readonly string GeneralUsageMessage = "Usage: /x:<system-ip or hostname> /user:<WDP username> /pwd:<WDP password> [/op:<operation type> [operation parameters]]";

        /// <summary>
        /// The registry key that Xbox uses for storing default console information.
        /// </summary>
        private static readonly string DefaultConsoleRegkey = "HKEY_CURRENT_USER\\Software\\Microsoft\\Durango\\WDP\\Consoles";

        /// <summary>
        /// The XTF registry key that Xbox uses for storing default console information.
        /// </summary>
        private static readonly string DefaultXtfConsoleRegkey = "HKEY_CURRENT_USER\\Software\\Microsoft\\Durango\\Xtf\\Consoles";

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

            /// <summary>
            /// Sets the default xbox console to be this one.
            /// Uses the same registry setting as XbConnect tool.
            /// </summary>
            ConnectOperation,
        }

        /// <summary>
        /// Main entry point
        /// </summary>
        /// <param name="args">command line args</param>
        public static void Main(string[] args)
        {
            ParameterHelper parameters = new ParameterHelper();
            Program app = new Program();

            string targetConsole = string.Empty;

            try
            {
                parameters.ParseCommandLine(args);

                OperationType operation = OperationType.None;

                if (parameters.HasParameter(ParameterHelper.Operation))
                {
                    operation = OperationStringToEnum(parameters.GetParameterValue("op"));
                }

                // Allow /ip: to still function, even though we've moved to /x: in the documentation.
                if (parameters.HasParameter(ParameterHelper.IpOrHostnameOld) && !parameters.HasParameter(ParameterHelper.IpOrHostname))
                {
                    targetConsole = parameters.GetParameterValue(ParameterHelper.IpOrHostnameOld);
                }
                else if (parameters.HasParameter(ParameterHelper.IpOrHostname))
                {
                    targetConsole = parameters.GetParameterValue(ParameterHelper.IpOrHostname);
                }

                if (string.IsNullOrEmpty(targetConsole))
                {
                    object regValue;
                    regValue = Microsoft.Win32.Registry.GetValue(DefaultConsoleRegkey, null, null);

                    if (regValue == null)
                    {
                        regValue = Microsoft.Win32.Registry.GetValue(DefaultXtfConsoleRegkey, null, null);
                    }

                    if (regValue is string)
                    {
                        targetConsole = regValue as string;
                    }
                    else
                    {
                        throw new Exception("No default console is currently set. Must provide an ip address or hostname to connect to: /x:<ip or hostname>.");
                    }
                }

                IDevicePortalConnection connection = null;

                try
                {
                    if (!parameters.HasParameter(ParameterHelper.WdpUser) || !parameters.HasParameter(ParameterHelper.WdpPassword))
                    {
                        connection = new DevicePortalConnection(targetConsole);
                    }
                    else
                    {
                        connection = new DevicePortalConnection(targetConsole, parameters.GetParameterValue(ParameterHelper.WdpUser), parameters.GetParameterValue(ParameterHelper.WdpPassword));
                    }
                }
                catch (TypeLoadException)
                {
                    // Windows 7 doesn't support credential storage so we'll get a TypeLoadException
                    if (!parameters.HasParameter(ParameterHelper.WdpUser) || !parameters.HasParameter(ParameterHelper.WdpPassword))
                    {
                        throw new Exception("Credential storage is not supported on your PC. It requires Windows 8+ to run. Please provide the user and password parameters.");
                    }
                    else
                    {
                        string connectionUri = string.Format("https://{0}:11443", targetConsole);
                        connection = new DefaultDevicePortalConnection(connectionUri, parameters.GetParameterValue(ParameterHelper.WdpUser), parameters.GetParameterValue(ParameterHelper.WdpPassword));
                    }
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

                DevicePortal portal = new DevicePortal(connection);

                Task connectTask = portal.Connect(updateConnection: false);
                connectTask.Wait();

                if (portal.ConnectionHttpStatusCode != HttpStatusCode.OK)
                {
                    if (portal.ConnectionHttpStatusCode == HttpStatusCode.Unauthorized)
                    {
                        if (connection.Credentials == null)
                        {
                            Console.WriteLine("The WDP connection was rejected due to missing credentials.\n\nPlease provide the /user:<username> and /pwd:<pwd> parameters on your first call to WDP.");
                        }
                        else
                        {
                            Console.WriteLine("The WDP connection was rejected due to bad credentials.\n\nPlease check the /user:<username> and /pwd:<pwd> parameters.");
                        }
                    }
                    else if (portal.ConnectionHttpStatusCode != 0)
                    {
                        Console.WriteLine(string.Format("Failed to connect to WDP with HTTP Status code: {0}", portal.ConnectionHttpStatusCode));
                    }
                    else
                    {
                        Console.WriteLine("Failed to connect to WDP for unknown reason.\n\nEnsure your address is the system IP or hostname ({0}) and the machine has WDP configured.", targetConsole);
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
                else if (operation == OperationType.ConnectOperation)
                {
                    // User provided a new ip or hostname to set as the default.
                    if (parameters.HasParameter(ParameterHelper.IpOrHostname) || parameters.HasParameter(ParameterHelper.IpOrHostnameOld))
                    {
                        Microsoft.Win32.Registry.SetValue(DefaultConsoleRegkey, null, targetConsole);
                        Console.WriteLine("Default console set to {0}", targetConsole);
                    }
                    else
                    {
                        Console.WriteLine("Connected to Default console: {0}", targetConsole);
                    }
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
            else if (operation.Equals("connect", StringComparison.OrdinalIgnoreCase))
            {
                return OperationType.ConnectOperation;
            }

            throw new Exception("Unknown Operation Type. " + AvailableOperationsText);
        }
    }
}
