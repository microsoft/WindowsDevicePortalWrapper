//----------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Tools.WindowsDevicePortal;
using static Microsoft.Tools.WindowsDevicePortal.DevicePortal;

namespace MockDataGenerator
{
    /// <summary>
    /// Main entry point for the test command line class.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// WebSocket operation prefix
        /// </summary>
        private const string WebSocketOpertionPrefix = "WebSocket/";

        /// <summary>
        /// Usage string
        /// </summary>
        private const string GeneralUsageMessage = "Usage: /ip:<system-ip or hostname> /user:<WDP username> /pwd:<WDP password> [/endpoint:<api to call>] [/directory:<directory to save mock data file(s)>";

        /// <summary>
        /// Endpoints for REST calls to populate
        /// </summary>
        private static readonly string[] Endpoints = 
        {
            DevicePortal.DeviceFamilyApi,
            DevicePortal.MachineNameApi,
            DevicePortal.OsInfoApi,
            DevicePortal.XboxLiveUserApi,
            DevicePortal.XboxSettingsApi,
            DevicePortal.SystemPerfApi,
            DevicePortal.RunningProcessApi,
            WebSocketOpertionPrefix + DevicePortal.SystemPerfApi,
            WebSocketOpertionPrefix + DevicePortal.RunningProcessApi
        };

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

            if (parameters.HasFlag(ParameterHelper.HelpFlag))
            {
                Console.WriteLine(GeneralUsageMessage);
                return;
            }

            if (!parameters.HasParameter(ParameterHelper.IpOrHostname) || !parameters.HasParameter(ParameterHelper.WdpUser) || !parameters.HasParameter(ParameterHelper.WdpPassword))
            {
                Console.WriteLine("Missing one or more required parameter(s). Must provide ip, user, and pwd");
                Console.WriteLine();
                Console.WriteLine(GeneralUsageMessage);
                return;
            }

            DevicePortalConnection connection = new DevicePortalConnection(parameters.GetParameterValue(ParameterHelper.IpOrHostname), parameters.GetParameterValue(ParameterHelper.WdpUser), parameters.GetParameterValue(ParameterHelper.WdpPassword));
            DevicePortal portal = new DevicePortal(connection);

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

                return;
            }

            string directory = "MockData";

            if (parameters.HasParameter("directory"))
            {
                directory = parameters.GetParameterValue("directory");
            }

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (parameters.HasParameter("endpoint"))
            {
                HttpOperations httpOperation = HttpOperations.Get;
                string endpoint = parameters.GetParameterValue("endpoint");

                if (endpoint.StartsWith(WebSocketOpertionPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    httpOperation = HttpOperations.WebSocket;
                    endpoint = endpoint.Substring(WebSocketOpertionPrefix.Length);
                }

                Task saveResponseTask = portal.SaveEndpointResponseToFile(endpoint, directory, httpOperation);
                saveResponseTask.Wait();
            }
            else
            {
                foreach (string endpoint in Endpoints)
                {
                    string finalEndpoint = endpoint;
                    HttpOperations httpOperation = HttpOperations.Get;

                    if (endpoint.StartsWith(WebSocketOpertionPrefix, StringComparison.OrdinalIgnoreCase))
                    {
                        httpOperation = HttpOperations.WebSocket;
                        finalEndpoint = endpoint.Substring(WebSocketOpertionPrefix.Length);
                    }

                    try
                    {
                        Task saveResponseTask = portal.SaveEndpointResponseToFile(finalEndpoint, directory, httpOperation);
                        saveResponseTask.Wait();
                    }
                    catch (Exception e)
                    {
                        // Print an error message if possible but continue on.
                        // Not all APIs are available on all device types.
                        if (e.InnerException is DevicePortalException)
                        {
                            DevicePortalException exception = e.InnerException as DevicePortalException;

                            Console.WriteLine(string.Format("Failed to generate .dat for {0} with status {1} ({2}).", endpoint, exception.HResult, exception.Reason));
                        }
                    }
                }
            }

            Console.WriteLine("Data generated in directory {0}. Please make sure to remove any personally identifiable information from the response(s) before adding them as mock responses.", directory);
        }
    }
}
