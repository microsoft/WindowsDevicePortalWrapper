//----------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Microsoft.Tools.WindowsDevicePortal;

namespace TestAppIoT
{
    /// <summary>
    /// Application class.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// The IP address of the device.
        /// </summary>
        private string ipAddress = null;

        /// <summary>
        /// The user name used when connecting to the device.
        /// </summary>
        private string userName = null;

        /// <summary>
        /// The password used when connecting to the device.
        /// </summary>
        private string password = null;

        /// <summary>
        /// The application entry point.
        /// </summary>
        /// <param name="args">Array of command line arguments.</param>
        public static void Main(string[] args)
        {
            Program app = new Program();

            try
            {
                app.ParseCommandLine(args);
            }
            catch (Exception e)
            {
                // TODO: Make a usage display
                Console.WriteLine(e.Message);
                return;
            }

            DevicePortal portal = new DevicePortal(new DevicePortalConnection(app.ipAddress, app.userName, app.password)); 

            Console.WriteLine("Connecting...");
            Task t = portal.Connect(updateConnection: false);
            t.Wait();
            Console.WriteLine("Connected to: " + portal.Address);
            Console.WriteLine("OS version: " + portal.OperatingSystemVersion);
            Console.WriteLine("Device family: " + portal.DeviceFamily);
            Console.WriteLine("Platform: " + portal.PlatformName + " (" + portal.Platform.ToString() + ")");

            while (true)
            {
                System.Threading.Thread.Sleep(1);
            }
        }

        /// <summary>
        /// Parses the application's command line.
        /// </summary>
        /// <param name="args">Array of command line arguments.</param>
        private void ParseCommandLine(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i].ToLower();
                if (!arg.StartsWith("/") && !arg.StartsWith("-"))
                {
                    throw new Exception(string.Format("Unrecognized argument: {0}", args[i]));
                }

                arg = arg.Substring(1);

                if (arg.StartsWith("ip:"))
                {
                    this.ipAddress = this.GetArgData(args[i]);
                }
                else if (arg.StartsWith("user:"))
                {
                    this.userName = this.GetArgData(args[i]);
                }
                else if (arg.StartsWith("pwd:"))
                {
                    this.password = this.GetArgData(args[i]);
                }
                else
                {
                    throw new Exception(string.Format("Unrecognized argument: {0}", args[i]));
                }
            }

            // We require at least a user name and password to proceed.
            if (string.IsNullOrWhiteSpace(this.userName) || string.IsNullOrWhiteSpace(this.password))
            {
                throw new Exception("You must specify a user name and a password");
            }
        }

        /// <summary>
        /// Gets the value from a key:value command line argument.
        /// </summary>
        /// <param name="arg">A key:value command line argument.</param>
        /// <returns>String containing the argument value.</returns>
        private string GetArgData(string arg)
        {
            int idx = arg.IndexOf(':');
            return arg.Substring(idx + 1);
        }
    }
}
