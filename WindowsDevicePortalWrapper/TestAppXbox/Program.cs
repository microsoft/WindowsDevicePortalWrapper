using Microsoft.Tools.WindowsDevicePortal;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace TestApp
{
    class Program
    {
        private bool _verbose = false;

        private enum OperationType
        {
            InfoOperation,
            UserOperation,
        }

        private const String GeneralUsageMessage = "Usage: /ip:<system-ip or hostname> /user:<WDP username> /pwd:<WDP password> [/op:<operation type> [operation parameters]]";

        static void Main(string[] args)
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

            app._verbose = parameters.HasFlag(ParameterHelper.VerboseFlag);

            DevicePortal portal = new DevicePortal(new DevicePortalConnection(parameters.GetParameterValue(ParameterHelper.IpOrHostname), parameters.GetParameterValue(ParameterHelper.WdpUser), parameters.GetParameterValue(ParameterHelper.WdpPassword)));

            Task connectTask = portal.Connect(null, null, false);
            connectTask.Wait();

            if (portal.ConnectionHttpStatusCode != HttpStatusCode.OK)
            {
                if (portal.ConnectionHttpStatusCode != 0)
                {
                    Console.WriteLine(String.Format("Failed to connect to WDP with HTTP Status code: {0}", portal.ConnectionHttpStatusCode));
                }
                else
                {
                    Console.WriteLine("Failed to connect to WDP for unknown reason.");
                }
            }
            else if (operation == OperationType.InfoOperation)
            {
                Console.WriteLine("OS version: " + portal.OperatingSystemVersion);
                Console.WriteLine("Platform: " + portal.Platform.ToString());

                Task<String> getNameTask = portal.GetDeviceName();
                getNameTask.Wait();
                Console.WriteLine("Device name: " + getNameTask.Result);
            }
            else if (operation == OperationType.UserOperation)
            {
                UserOperation.HandleOperation(portal, parameters);
            }
        }

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

            throw new Exception("Unknown Operation Type. Supported operations are the following:\n" +
                "info\n" +
                "xbluser\n");
        }
    }
}
