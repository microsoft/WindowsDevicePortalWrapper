//----------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Tools.WindowsDevicePortal;

namespace TestApp
{
    /// <summary>
    /// Main entry point for the test command line class.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Usage string
        /// </summary>
        private const string GeneralUsageMessage = "Usage: /ip:<system-ip or hostname> /user:<WDP username> /pwd:<WDP password> [/op:<operation type> [operation parameters]]";

        /// <summary>
        /// Event used to indicate that the running processes on the device have been received.
        /// </summary>
        private ManualResetEvent processesReceived = new ManualResetEvent(false);

        /// <summary>
        /// The security key to use when connecting to the network access point.
        /// </summary>
        private DevicePortal.DeviceProcesses deviceProcesses = null;

        /// <summary>
        /// Operation types
        /// </summary>
        private enum OperationType
        {
            /// <summary>
            /// Info operation
            /// </summary>
            InfoOperation,

            /// <summary>
            /// User operation
            /// </summary>
            UserOperation,

            /// <summary>
            /// Install Appx Package or loose folder operation
            /// </summary>
            InstallOperation,

            /// <summary>
            /// Reboot console operation
            /// </summary>
            RebootOperation,

            /// <summary>
            /// Processes operation
            /// </summary>
            ProcessesOperation,
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
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine();
                Console.WriteLine(GeneralUsageMessage);
                return;
            }

            OperationType operation = OperationType.InfoOperation;

            if (parameters.HasParameter(ParameterHelper.Operation))
            {
                try
                {
                    operation = OperationStringToEnum(parameters.GetParameterValue("op"));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine();
                    Console.WriteLine(GeneralUsageMessage);
                    return;
                }
            }

            if (!parameters.HasParameter(ParameterHelper.IpOrHostname) || !parameters.HasParameter(ParameterHelper.WdpUser) || !parameters.HasParameter(ParameterHelper.WdpPassword))
            {
                Console.WriteLine("Missing one or more required parameter(s). Must provide ip, user, and pwd");
                Console.WriteLine();
                Console.WriteLine(GeneralUsageMessage);
                return;
            }

            DevicePortal portal = new DevicePortal(new DevicePortalConnection(parameters.GetParameterValue(ParameterHelper.IpOrHostname), parameters.GetParameterValue(ParameterHelper.WdpUser), parameters.GetParameterValue(ParameterHelper.WdpPassword)));

            Task connectTask = portal.Connect(null, null, false);
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
            else if (operation == OperationType.ProcessesOperation)
            {
                portal.ProcessesMessageReceived += app.ProcessesReceivedHandler;

                Task startListeningForProcessesTask = portal.StartListeningForProcesses();
                startListeningForProcessesTask.Wait();

                app.processesReceived.WaitOne();

                Task stopListeningForProcessesTask = portal.StopListeningForProcesses();
                stopListeningForProcessesTask.Wait();

                foreach (DevicePortal.ProcessInfo process in app.deviceProcesses.Processes)
                {
                    if (!string.IsNullOrEmpty(process.ImageName))
                    {
                        Console.WriteLine(process.ImageName);
                    }
                }
            }
        }

        /// <summary>
        /// Helper for converting from operation string to enum
        /// </summary>
        /// <param name="operation">string representation of the operation type.</param>
        /// <returns>enum representation of the operation type.</returns>
        private static OperationType OperationStringToEnum(string operation)
        {
            if (operation.Equals("info", StringComparison.InvariantCultureIgnoreCase))
            {
                return OperationType.InfoOperation;
            }
            else if (operation.Equals("xbluser", StringComparison.InvariantCultureIgnoreCase))
            {
                return OperationType.UserOperation;
            }
            else if (operation.Equals("install", StringComparison.InvariantCultureIgnoreCase))
            {
                return OperationType.InstallOperation;
            }
            else if (operation.Equals("reboot", StringComparison.InvariantCultureIgnoreCase))
            {
                return OperationType.RebootOperation;
            }
            else if (operation.Equals("processes", StringComparison.InvariantCultureIgnoreCase))
            {
                return OperationType.ProcessesOperation;
            }

            throw new Exception("Unknown Operation Type. Supported operations are the following:\n" +
                "info\n" +
                "xbluser\n");
        }

        /// <summary>
        /// Handler for the ProcessesMessageReceived event.
        /// </summary>
        /// <param name="sender">The object sending the event.</param>
        /// <param name="args">The event data.</param>
        private void ProcessesReceivedHandler(
            object sender,
            WebSocketMessageReceivedEventArgs<DevicePortal.DeviceProcesses> args)
        {
            if (args.Message != null)
            {
                this.deviceProcesses = args.Message;
                this.processesReceived.Set();
            }
        }
    }
}
