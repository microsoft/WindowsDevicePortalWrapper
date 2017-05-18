//----------------------------------------------------------------------------------------------
// <copyright file="AppOperation.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Microsoft.Tools.WindowsDevicePortal;
using static Microsoft.Tools.WindowsDevicePortal.DevicePortal;

namespace XboxWdpDriver
{
    /// <summary>
    /// Helper for App related operations (List, Suspend, Resume, Launch, Terminate, Uninstall)
    /// </summary>
    public class AppOperation
    {
        /// <summary>
        /// Usage message for this operation
        /// </summary>
        private const string AppUsageMessage = "Usage:\n" +
            "  /subop:list\n" +
            "        Lists all installed packages on the console.\n" +
    // Suspend and resume are currently not supported. The endpoints are not very
    // reliable yet on Xbox One and are completely unavailable on other platforms.
    // We'll revisit these two operations in the future.
            //"  /subop:suspend /pfn:<packageFullName>\n" +
            //"        Suspends the requested application.\n" +
            //"  /subop:resume /pfn:<packageFullName>\n" +
            //"        Resumes the requested application.\n" +
            "  /subop:launch /pfn:<packageFullName> /aumid:<appId>\n" +
            "        Starts the requested application.\n" +
            "  /subop:terminate /pfn:<packageFullName>\n" +
            "        Stops the requested application.\n" +
            "  /subop:uninstall /pfn:<packageFullName>\n" +
            "        Removes or unregisters the given application from the console.\n";

        /// <summary>
        /// Main entry point for handling a Config operation
        /// </summary>
        /// <param name="portal">DevicePortal reference for communicating with the device.</param>
        /// <param name="parameters">Parsed command line parameters.</param>
        public static void HandleOperation(DevicePortal portal, ParameterHelper parameters)
        {
            if (parameters.HasFlag(ParameterHelper.HelpFlag))
            {
                Console.WriteLine(AppUsageMessage);
                return;
            }

            string operationType = parameters.GetParameterValue("subop");

            if (string.IsNullOrWhiteSpace(operationType))
            {
                Console.WriteLine("Missing subop parameter");
                Console.WriteLine();
                Console.WriteLine(AppUsageMessage);
                return;
            }

            operationType = operationType.ToLowerInvariant();

            try
            {
                if (operationType.Equals("list"))
                {
                    Task<AppPackages> packagesTask = portal.GetInstalledAppPackagesAsync();

                    packagesTask.Wait();
                    Console.WriteLine(packagesTask.Result);
                }
                else
                {
                    string packageFullName = parameters.GetParameterValue("pfn");

                    if (string.IsNullOrEmpty(packageFullName))
                    {
                        Console.WriteLine("The Package Full Name is required as the /pfn<packageFullName> parameter for this operation.");
                        Console.WriteLine();
                        Console.WriteLine(AppUsageMessage);
                        return;
                    }

                    if (operationType.Equals("suspend"))
                    {
                        Console.WriteLine("Suspend isn't currently supported, but will be in the future.");
                    }
                    else if (operationType.Equals("resume"))
                    {
                        Console.WriteLine("Resume isn't currently supported, but will be in the future.");
                    }
                    else if (operationType.Equals("launch"))
                    {
                        string aumid = parameters.GetParameterValue("aumid");
                        if (string.IsNullOrEmpty(aumid))
                        {
                            Console.WriteLine("The appId (AUMID) is required as the /aumid:<appId> parameter for the launch operation.");
                            Console.WriteLine();
                            Console.WriteLine(AppUsageMessage);
                            return;
                        }

                        Task launchTask = portal.LaunchApplicationAsync(aumid, packageFullName);
                        launchTask.Wait();

                        Console.WriteLine("Application launched.");
                    }
                    else if (operationType.Equals("terminate"))
                    {
                        Task terminateTask = portal.TerminateApplicationAsync(packageFullName);
                        terminateTask.Wait();

                        Console.WriteLine("Application terminated.");
                    }
                    else if (operationType.Equals("uninstall"))
                    {
                        Task uninstallTask = portal.UninstallApplicationAsync(packageFullName);
                        uninstallTask.Wait();

                        Console.WriteLine("Application uninstalled.");
                    }
                }
            }
            catch (AggregateException e)
            {
                if (e.InnerException is DevicePortalException)
                {
                    DevicePortalException innerException = e.InnerException as DevicePortalException;

                    Console.WriteLine(string.Format("Exception encountered: 0x{0:X} : {1}", innerException.HResult, innerException.Reason));
                }
                else if (e.InnerException is OperationCanceledException)
                {
                    Console.WriteLine("The operation was cancelled.");
                }
                else
                {
                    Console.WriteLine(string.Format("Unexpected exception encountered: {0}", e.Message));
                }

                return;
            }
        }
    }
}