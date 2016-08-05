//----------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Tools.WindowsDevicePortal;

namespace TestApp
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
                Console.WriteLine(e.Message);
                Console.WriteLine("Usage: TestApp.exe [-ip:IP_ADDRESS:PORT] -user:USERNAME -pwd:PASSWORD");
                Console.WriteLine("\t TestApp.exe connects by default to localhost:50443");
                Console.WriteLine("\t -ip:IP_ADDRESS:PORT, connect to Device Portal running at the specified address.");
                return;
            }

            DevicePortal portal = new DevicePortal(new DevicePortalConnection(app.ipAddress, app.userName, app.password)); 
            Console.WriteLine("Connecting...");
            Task connectTask = portal.Connect();
            connectTask.Wait();
            Console.WriteLine("Connected to: " + portal.Address);
            Console.WriteLine("OS version: " + portal.OperatingSystemVersion);
            Console.WriteLine("Device family: " + portal.DeviceFamily);
            Console.WriteLine("Platform: " + portal.PlatformName + " (" + portal.Platform.ToString() + ")");

            Task<string> getNameTask = portal.GetDeviceName();
            getNameTask.Wait();
            Console.WriteLine("Device name: " + getNameTask.Result);

            TestTagListing(portal);

            TestDeviceList(portal);

            while (true)
            {
                System.Threading.Thread.Sleep(0);
            }
        }

        /// <summary>
        /// Tests the DNS-SD APIs
        /// </summary>
        /// <param name="portal">DevicePortal object used for testing</param>
        private static void TestTagListing(DevicePortal portal)
        {
            Task<List<string>> getTagsTask = portal.GetServiceTags();
            getTagsTask.Wait();
            Console.Write("Service Tags: ");
            if (getTagsTask.Result.Count == 0)
            {
                Console.Write("<none>");
            }

            foreach (string s in getTagsTask.Result)
            {
                Console.Write(s + ", ");
            }

            Console.WriteLine(string.Empty);
        }

        /// <summary>
        /// Tests the DeviceManager APIs
        /// </summary>
        /// <param name="portal">DevicePortal object used for testing</param>
        private static void TestDeviceList(DevicePortal portal)
        {
            Task<List<DevicePortal.Device>> getdeviceListTask = portal.GetDeviceList();
            getdeviceListTask.Wait();
            List<DevicePortal.Device> deviceList = getdeviceListTask.Result;

            DevicePortal.Device device = deviceList.Find(x => x.FriendlyName != null); //not all Devices come with a friendly name 
            Console.WriteLine("First Device: {0}", device.Description);
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
