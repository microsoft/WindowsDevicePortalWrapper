using Microsoft.Tools.WindowsDevicePortal;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace TestApp
{
    class Program : IDisposable
    {
        private ManualResetEvent _mreConnected = new ManualResetEvent(false);
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
            portal.ConnectionStatus += app.ConnectionStatusHandler;

            app._mreConnected.Reset();
            Task t = portal.Connect();
            app._mreConnected.WaitOne();

            if (operation == OperationType.InfoOperation)
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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(Boolean disposing)
        {
            if (disposing)
            { 
                _mreConnected?.Dispose();
                _mreConnected = null;
            }
        }

        private void ConnectionStatusHandler(Object sender, DeviceConnectionStatusEventArgs args)
        {
            if (_verbose)
            {
                Console.WriteLine(args.Message);
            }

            if ((args.Status == DeviceConnectionStatus.Connected) ||
                (args.Status == DeviceConnectionStatus.Failed))
            {
                _mreConnected.Set();
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
