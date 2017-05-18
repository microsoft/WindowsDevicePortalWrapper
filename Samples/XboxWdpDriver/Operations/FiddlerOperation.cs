//----------------------------------------------------------------------------------------------
// <copyright file="FiddlerOperation.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Tools.WindowsDevicePortal;

namespace XboxWdpDriver
{
    /// <summary>
    /// Helper for Fiddler related operations
    /// </summary>
    public class FiddlerOperation
    {
        /// <summary>
        /// Usage message for this operation
        /// </summary>
        private const string FiddlerUsageMessage = "Usage:\n" +
            "  /state:<on or off> [/reboot] [/proxyaddress:<proxy address> /proxyport:<proxy port> /certpath:<path to cert file>]\n" +
            "        Whether to enable or disable Fiddler. Enabling and disabling Fiddler\n" +
            "        requires a reboot. You can specify the /reboot flag to do the reboot\n" +
            "        automatically. If Fiddler is being enabled, proxyaddress and proxyport\n" +
            "        are both required. If Fiddler has not been configured on this console\n" +
            "        previously, then the cert file is also required.";

        /// <summary>
        /// Main entry point for handling a Fiddler operation
        /// </summary>
        /// <param name="portal">DevicePortal reference for communicating with the device.</param>
        /// <param name="parameters">Parsed command line parameters.</param>
        public static void HandleOperation(DevicePortal portal, ParameterHelper parameters)
        {
            if (parameters.HasFlag(ParameterHelper.HelpFlag))
            {
                Console.WriteLine(FiddlerUsageMessage);
                return;
            }

            string state = parameters.GetParameterValue("state");

            if (string.IsNullOrEmpty(state))
            {
                Console.WriteLine("/state parameter is required.");
                Console.WriteLine();
                Console.WriteLine(FiddlerUsageMessage);
                return;
            }

            try
            {
                if (string.Equals(state, "on", StringComparison.OrdinalIgnoreCase))
                {
                    string proxyAddress = parameters.GetParameterValue("proxyaddress");
                    string proxyPort = parameters.GetParameterValue("proxyport");

                    if (string.IsNullOrEmpty(proxyAddress) || string.IsNullOrEmpty(proxyPort))
                    {
                        Console.WriteLine("/proxyaddress and /proxyport are required for enabling Fiddler.");
                        Console.WriteLine();
                        Console.WriteLine(FiddlerUsageMessage);
                        return;
                    }

                    Task fiddlerEnableTask = portal.EnableFiddlerTracingAsync(proxyAddress, proxyPort, parameters.GetParameterValue("certpath"));
                    fiddlerEnableTask.Wait();
                    Console.WriteLine("Fiddler enabled.");
                }
                else if (string.Equals(state, "off", StringComparison.OrdinalIgnoreCase))
                {
                    Task fiddlerDisableTask = portal.DisableFiddlerTracingAsync();
                    fiddlerDisableTask.Wait();
                    Console.WriteLine("Fiddler disabled.");
                }
                else
                {
                    Console.WriteLine("Unknown state parameter: {0}. Must be 'on' or 'off'.", state);
                    Console.WriteLine();
                    Console.WriteLine(FiddlerUsageMessage);
                    return;
                }

                if (parameters.HasFlag("reboot"))
                {
                    Task rebootTask = portal.RebootAsync();
                    rebootTask.Wait();
                    Console.WriteLine("Console rebooting...");
                }
                else
                {
                    Console.WriteLine("A reboot is required before this takes effect.");
                }
            }
            catch (AggregateException e)
            {
                if (e.InnerException is DevicePortalException)
                {
                    DevicePortalException innerException = e.InnerException as DevicePortalException;

                    Console.WriteLine(string.Format("Exception encountered: {0}, hr = 0x{1:X} : {2}", innerException.StatusCode, innerException.HResult, innerException.Reason));
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