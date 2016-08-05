//----------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
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
        /// Usage string
        /// </summary>
        private const string GeneralUsageMessage = "Usage: /address:<URL for device (eg. https://10.0.0.1:11443)> /user:<WDP username> /pwd:<WDP password> [/endpoint:<api to call>] [/directory:<directory to save mock data file(s)>";

        /// <summary>
        /// Endpoints for REST calls to populate. Feel free to override this list (especially locally) to
        /// facilitate generating a large number of mock files all simultaneously.
        /// </summary>
        private static readonly Endpoint[] Endpoints = 
        {
            new Endpoint(HttpMethods.Get, DevicePortal.DeviceFamilyApi),
            new Endpoint(HttpMethods.Get, DevicePortal.MachineNameApi),
            new Endpoint(HttpMethods.Get, DevicePortal.OsInfoApi),
            new Endpoint(HttpMethods.Get, DevicePortal.BatteryStateApi),
            new Endpoint(HttpMethods.Get, DevicePortal.PowerStateApi),
            new Endpoint(HttpMethods.Get, DevicePortal.IpConfigApi),
            new Endpoint(HttpMethods.Get, DevicePortal.SystemPerfApi),
            new Endpoint(HttpMethods.Get, DevicePortal.RunningProcessApi),
            new Endpoint(HttpMethods.WebSocket, DevicePortal.SystemPerfApi),
            new Endpoint(HttpMethods.WebSocket, DevicePortal.RunningProcessApi),

            // HoloLens specific endpoints
            new Endpoint(HttpMethods.Get, DevicePortal.HolographicIpdApi),
            new Endpoint(HttpMethods.Get, DevicePortal.HolographicServicesApi),
            new Endpoint(HttpMethods.Get, DevicePortal.HolographicWebManagementHttpSettingsApi),
            new Endpoint(HttpMethods.Get, DevicePortal.MrcFileListApi),
            new Endpoint(HttpMethods.Get, DevicePortal.MrcStatusApi),
            new Endpoint(HttpMethods.Get, DevicePortal.ThermalStageApi),

            // Xbox One specific endpoints
            new Endpoint(HttpMethods.Get, DevicePortal.XboxLiveUserApi),
            new Endpoint(HttpMethods.Get, DevicePortal.XboxSettingsApi),
            new Endpoint(HttpMethods.Get, DevicePortal.XboxLiveSandboxApi),
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

            if (!parameters.HasParameter(ParameterHelper.FullAddress) || !parameters.HasParameter(ParameterHelper.WdpUser) || !parameters.HasParameter(ParameterHelper.WdpPassword))
            {
                Console.WriteLine("Missing one or more required parameter(s). Must provide address, user, and pwd");
                Console.WriteLine();
                Console.WriteLine(GeneralUsageMessage);
                return;
            }

            IDevicePortalConnection connection = new DefaultDevicePortalConnection(parameters.GetParameterValue(ParameterHelper.FullAddress), parameters.GetParameterValue(ParameterHelper.WdpUser), parameters.GetParameterValue(ParameterHelper.WdpPassword));
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
                HttpMethods httpMethod = HttpMethods.Get;

                if (parameters.HasParameter("method"))
                {
                    // This is case sensitive. Since it's only used while generating mocks which is a development time action,
                    // that seems okay. If we want to revisit I'd prefer keeping the casing of the enum and using a switch or
                    // if/else block to manually convert.
                    httpMethod = (HttpMethods)Enum.Parse(typeof(HttpMethods), parameters.GetParameterValue("method"));
                }

                string endpoint = parameters.GetParameterValue("endpoint");

                Task saveResponseTask = portal.SaveEndpointResponseToFile(endpoint, directory, httpMethod);
                saveResponseTask.Wait();
            }
            else
            {
                foreach (Endpoint endpoint in Endpoints)
                {
                    HttpMethods httpMethod = endpoint.Method;
                    string finalEndpoint = endpoint.Value;

                    try
                    {
                        Task saveResponseTask = portal.SaveEndpointResponseToFile(finalEndpoint, directory, httpMethod);
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

            Console.WriteLine("Data generated in directory {0}.", directory);
            Console.WriteLine();
            Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            Console.WriteLine("Please make sure to remove any personally identifiable information from the\n" +
                              "response(s) (such as alias/emails, ip addresses, and machine names) before\n" +
                              "adding them as mock responses!");
            Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");

            // If a debugger is attached, don't close but instead loop here until
            // closed.
            while (Debugger.IsAttached)
            {
                Thread.Sleep(0);
            }
        }

        /// <summary>
        /// Encapsulation of an endpoint and its HTTP method.
        /// </summary>
        private class Endpoint
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Endpoint"/> class.
            /// </summary>
            /// <param name="method">The HTTP method this endpoint should use.</param>
            /// <param name="endpoint">The actual endpoint.</param>
            public Endpoint(HttpMethods method, string endpoint)
            {
                this.Method = method;
                this.Value = endpoint;
            }

            /// <summary>
            /// Gets or sets the HTTP Method.
            /// </summary>
            public HttpMethods Method { get; set; }

            /// <summary>
            /// Gets or sets the endpoint value.
            /// </summary>
            public string Value { get; set; }

            /// <summary>
            /// Overridden ToString method.
            /// </summary>
            /// <returns>Human readable representation of an Endpoint.</returns>
            public override string ToString()
            {
                return this.Method.ToString() + " : " + this.Value;
            }
        }
    }
}
