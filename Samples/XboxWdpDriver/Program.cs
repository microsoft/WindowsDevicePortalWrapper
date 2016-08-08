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
                                "   config\n" +
                                "   connect\n" +
                                "   fiddler\n" +
                                "   file\n" +
                                "   info\n" +
                                "   install\n" +
                                "   processes\n" +
                                "   reboot\n" +
                                "   sandbox\n" +
                                "   screenshot\n" +
                                "   systemPerf\n" +
                                "   xbluser";

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
        /// Operation types. These should be arranged alphabetically (other than None)
        /// for ease of use.
        /// </summary>
        private enum OperationType
        {
            /// <summary>
            /// No operation (just connects to the console).
            /// </summary>
            None,

            /// <summary>
            /// Get or set Xbox Settings.
            /// </summary>
            ConfigOperation,

            /// <summary>
            /// Sets the default xbox console to be this one.
            /// Uses the same registry setting as XbConnect tool.
            /// </summary>
            ConnectOperation,

            /// <summary>
            /// Manages enabling and disabling a Fiddler proxy for the console.
            /// </summary>
            FiddlerOperation,

            /// <summary>
            /// Does remote file operations.
            /// </summary>
            FileOperation,

            /// <summary>
            /// Info operation.
            /// </summary>
            InfoOperation,

            /// <summary>
            /// Install Appx Package or loose folder operation.
            /// </summary>
            InstallOperation,

            /// <summary>
            /// List processes operation.
            /// </summary>
            ListProcessesOperation,

            /// <summary>
            /// Reboot console operation.
            /// </summary>
            RebootOperation,

            /// <summary>
            /// Gets or sets the Xbox Live sandbox for the console.
            /// </summary>
            SandboxOperation,

            /// <summary>
            /// Takes a screenshot from the current Xbox One console.
            /// </summary>
            ScreenshotOperation,

            /// <summary>
            /// Get the system performance operation.
            /// </summary>
            SystemPerfOperation,

            /// <summary>
            /// User operation.
            /// </summary>
            XblUserOperation,
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
                else
                {
                    // If the operation is more than a couple lines, it should
                    // live in its own file. These are arranged alphabetically
                    // for ease of use.
                    switch(operation)
                    {
                        case OperationType.ConfigOperation:
                            ConfigOperation.HandleOperation(portal, parameters);
                            break;

                        case OperationType.ConnectOperation:
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
                            break;

                        case OperationType.FiddlerOperation:
                            FiddlerOperation.HandleOperation(portal, parameters);
                            break;

                        case OperationType.FileOperation:
                            FileOperation.HandleOperation(portal, parameters);
                            break;

                        case OperationType.InfoOperation:
                            Console.WriteLine("OS version: " + portal.OperatingSystemVersion);
                            Console.WriteLine("Platform: " + portal.PlatformName + " (" + portal.Platform.ToString() + ")");

                            Task<string> getNameTask = portal.GetDeviceName();
                            getNameTask.Wait();
                            Console.WriteLine("Device name: " + getNameTask.Result);
                            break;

                        case OperationType.InstallOperation:
                            // Ensure we have an IP since SMB might need it for path generation.
                            parameters.AddParameter(ParameterHelper.IpOrHostname, targetConsole);

                            InstallOperation.HandleOperation(portal, parameters);
                            break;

                        case OperationType.ListProcessesOperation:
                            ListProcessesOperation.HandleOperation(portal, parameters);
                            break;

                        case OperationType.RebootOperation:
                            Task rebootTask = portal.Reboot();
                            rebootTask.Wait();
                            Console.WriteLine("Rebooting device.");
                            break;

                        case OperationType.SandboxOperation:
                            SandboxOperation.HandleOperation(portal, parameters);
                            break;

                        case OperationType.ScreenshotOperation:
                            ScreenshotOperation.HandleOperation(portal, parameters);
                            break;

                        case OperationType.SystemPerfOperation:
                            SystemPerfOperation.HandleOperation(portal, parameters);
                            break;

                        case OperationType.XblUserOperation:
                            UserOperation.HandleOperation(portal, parameters);
                            break;

                        default:
                            Console.WriteLine("Successfully connected to console but no operation was specified. \n" +
                                "Use the '/op:<operation type>' parameter to run a specified operation.");
                            Console.WriteLine();
                            Console.WriteLine(AvailableOperationsText);
                            break;
                    }
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
            if (operation.Equals("config", StringComparison.OrdinalIgnoreCase))
            {
                return OperationType.ConfigOperation;
            }
            else if (operation.Equals("connect", StringComparison.OrdinalIgnoreCase))
            {
                return OperationType.ConnectOperation;
            }
            else if (operation.Equals("fiddler", StringComparison.OrdinalIgnoreCase))
            {
                return OperationType.FiddlerOperation;
            }
            else if (operation.Equals("file", StringComparison.OrdinalIgnoreCase))
            {
                return OperationType.FileOperation;
            }
            else if (operation.Equals("info", StringComparison.OrdinalIgnoreCase))
            {
                return OperationType.InfoOperation;
            }
            else if (operation.Equals("install", StringComparison.OrdinalIgnoreCase))
            {
                return OperationType.InstallOperation;
            }
            else if (operation.Equals("processes", StringComparison.OrdinalIgnoreCase))
            {
                return OperationType.ListProcessesOperation;
            }
            else if (operation.Equals("reboot", StringComparison.OrdinalIgnoreCase))
            {
                return OperationType.RebootOperation;
            }
            else if (operation.Equals("sandbox", StringComparison.OrdinalIgnoreCase))
            {
                return OperationType.SandboxOperation;
            }
            else if (operation.Equals("screenshot", StringComparison.OrdinalIgnoreCase))
            {
                return OperationType.ScreenshotOperation;
            }
            else if (operation.Equals("systemPerf", StringComparison.OrdinalIgnoreCase))
            {
                return OperationType.SystemPerfOperation;
            }
            else if (operation.Equals("xbluser", StringComparison.OrdinalIgnoreCase))
            {
                return OperationType.XblUserOperation;
            }

            throw new Exception("Unknown Operation Type. " + AvailableOperationsText);
        }
    }
}
