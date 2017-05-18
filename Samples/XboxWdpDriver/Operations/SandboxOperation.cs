//----------------------------------------------------------------------------------------------
// <copyright file="SandboxOperation.cs" company="Microsoft Corporation">
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
    /// Helper for Sandbox related operations
    /// </summary>
    public class SandboxOperation
    {
        /// <summary>
        /// Usage message for this operation
        /// </summary>
        private const string SandboxUsageMessage = "Usage:\n" +
            "  [/value:<desired value> [/reboot]]\n" +
            "        Gets or sets the current Xbox Live sandbox value. Changing the current\n" +
            "        sandbox requires a reboot, which can be done automatically be specifying\n" +
            "        the /reboot flag.";

        /// <summary>
        /// Main entry point for handling a Sandbox operation
        /// </summary>
        /// <param name="portal">DevicePortal reference for communicating with the device.</param>
        /// <param name="parameters">Parsed command line parameters.</param>
        public static void HandleOperation(DevicePortal portal, ParameterHelper parameters)
        {
            if (parameters.HasFlag(ParameterHelper.HelpFlag))
            {
                Console.WriteLine(SandboxUsageMessage);
                return;
            }

            string desiredValue = parameters.GetParameterValue("value");

            if (string.IsNullOrEmpty(desiredValue))
            {
                Task<Sandbox> getSandboxTask = portal.GetXboxLiveSandboxAsync();
                getSandboxTask.Wait();

                Console.WriteLine(getSandboxTask.Result);
            }
            else
            {
                Task<Sandbox> setSandboxTask = portal.SetXboxLiveSandboxAsync(desiredValue);
                setSandboxTask.Wait();

                Console.WriteLine("{0} -> {1}", setSandboxTask.Result, desiredValue);

                if (parameters.HasFlag("reboot"))
                {
                    Task rebootTask = portal.RebootAsync();
                    rebootTask.Wait();
                    Console.WriteLine("Console rebooting...");
                }
                else
                {
                    Console.WriteLine("A reboot is required before this setting takes effect.");
                }
            }
        }
    }
}