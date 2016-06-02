using Microsoft.Tools.WindowsDevicePortal;
using System;
using System.Threading.Tasks;

namespace TestApp
{
    class Program
    {
        private String _ipAddress = null;
        private String _userName = null;
        private String _password = null;

        static void Main(string[] args)
        {
            Program app = new Program();

            try
            {
                app.ParseCommandLine(args);
            }
            catch(Exception e)
            {
                // TODO: Make a usage display
                Console.WriteLine(e.Message);
                return;
            }

            DevicePortal portal = new DevicePortal(new DevicePortalConnection(app._ipAddress, app._userName, app._password)); 
            Console.WriteLine("Connecting...");
            Task connectTask = portal.Connect();
            connectTask.Wait();
            Console.WriteLine("Connected to: " + portal.Address);
            Console.WriteLine("OS version: " + portal.OperatingSystemVersion);
            Console.WriteLine("Platform: " + portal.Platform.ToString());

            Task <String> getNameTask = portal.GetDeviceName();
            getNameTask.Wait();
            Console.WriteLine("Device name: " + getNameTask.Result);

            while(true)
            {
                System.Threading.Thread.Sleep(0);
            }
        }

        private void ParseCommandLine(String[] args)
        {
            for (Int32 i = 0; i < args.Length; i++)
            {
                String arg = args[i].ToLower();
                if (!arg.StartsWith("/'") && !arg.StartsWith("-"))
                {
                    throw new Exception(String.Format("Unrecognized argument: {0}", args[i]));
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
                // TODO: ssid, networkKey
                else
                {
                    throw new Exception(String.Format("Unrecognized argument: {0}", args[i]));
                }
            }

            // We require at least a user name and password to proceed.
            if (String.IsNullOrWhiteSpace(_userName) || String.IsNullOrWhiteSpace(_password))
            {
                    throw new Exception("You must specify a user name and a password");
            }
        }

        private String GetArgData(String arg)
        {
            Int32 idx = arg.IndexOf(':');
            return arg.Substring(idx+1);
        }
    }
}
