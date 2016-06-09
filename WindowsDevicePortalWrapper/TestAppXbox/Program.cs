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
        private String _ipAddress = null;
        private String _userName = null;
        private String _password = null;

        private OperationType _operation = OperationType.InfoOperation;
        private List<String> _operationArgs = new List<String>();

        private String _ssid = null;
        private String _key = null;

        private ManualResetEvent _mreConnected = new ManualResetEvent(false);

        private enum OperationType
        {
            InfoOperation,
            UserOperation,
        }

        static void Main(string[] args)
        {
            Program app = new Program();

            try
            {
                app.ParseCommandLine(args);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }

            DevicePortal portal = new DevicePortal(new DevicePortalConnection(app._ipAddress, app._userName, app._password));
            portal.ConnectionStatus += app.ConnectionStatusHandler;

            app._mreConnected.Reset();
            Console.WriteLine("Connecting...");
            Task t = portal.Connect(app._ssid, app._key);
            app._mreConnected.WaitOne();

            Console.WriteLine("Connected to: " + portal.Address);

            if (app._operation == OperationType.InfoOperation)
            {
                Console.WriteLine("OS version: " + portal.OperatingSystemVersion);
                Console.WriteLine("Platform: " + portal.Platform.ToString());

                Task<String> getNameTask = portal.GetDeviceName();
                getNameTask.Wait();
                Console.WriteLine("Device name: " + getNameTask.Result);
            }
            else if (app._operation == OperationType.UserOperation)
            {
                UserOperation.HandleOperation(portal, app._operationArgs);
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
            Console.WriteLine(args.Message);

            if ((args.Status == DeviceConnectionStatus.Connected) ||
                (args.Status == DeviceConnectionStatus.Failed))
            {
                _mreConnected.Set();
            }
        }

        private String GetArgData(String arg)
        {
            Int32 idx = arg.IndexOf(':');
            return arg.Substring(idx+1);
        }

        private const String GeneralUsageMessage = "Usage: /ip:<system-ip or hostname> /user:<WDP username> /pwd:<WDP password> [/op:<operation type> [operation parameters]]";

        private void ParseCommandLine(String[] args)
        {
            if (args.Length < 3)
            {
                throw new Exception("Missing required parameters.\n" + GeneralUsageMessage);
            }
            Int32 currentArg = 0;

            // Parse up to the optional operation type
            for (currentArg = 0; currentArg < args.Length && currentArg < 4; ++currentArg)
            {
                String arg = args[currentArg].ToLower();
                if (!arg.StartsWith("/") && !arg.StartsWith("-"))
                {
                    throw new Exception(String.Format("Unrecognized argument: {0}\n{1}", arg, GeneralUsageMessage));
                }

                arg = arg.Substring(1);

                if (arg.StartsWith("ip:"))
                {
                    _ipAddress = GetArgData(arg);
                }
                else if (arg.StartsWith("user:"))
                {
                    _userName = GetArgData(arg);
                }
                else if (arg.StartsWith("pwd:"))
                {
                    _password = GetArgData(arg);
                }
                else if (arg.StartsWith("op:"))
                {
                    String operation = GetArgData(arg);
                    _operation = OperationStringToEnum(operation);
                }
                else
                {
                    throw new Exception(String.Format("Unrecognized argument: {0}\n{1}", arg, GeneralUsageMessage));
                }
            }

            // Add remaining args to our operation arg list
            for (; currentArg < args.Length; ++currentArg)
            {
                _operationArgs.Add(args[currentArg]);
            }
        }

        private OperationType OperationStringToEnum(string operation)
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
