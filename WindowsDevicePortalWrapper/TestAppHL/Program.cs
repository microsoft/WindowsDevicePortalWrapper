//----------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Tools.WindowsDevicePortal;
using static Microsoft.Tools.WindowsDevicePortal.DevicePortal;

namespace TestAppHL
{
    /// <summary>
    /// Application class.
    /// </summary>
    internal class Program : IDisposable
    {
        /// <summary>
        /// Usage string
        /// </summary>
        private const string GeneralUsageMessage = "Usage: /ip:<system-ip or hostname> /user:<WDP username> /pwd:<WDP password> [/ssid:<network ssid> /key:<network key>]";

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
        /// The network access point to which the device should be connected.
        /// </summary>
        private string ssid = null;

        /// <summary>
        /// The security key to use when connecting to the network access point.
        /// </summary>
        private string key = null;

        /// <summary>
        /// Event used to indicate that the connection process is complete.
        /// </summary>
        private ManualResetEvent mreConnected = new ManualResetEvent(false);

        /// <summary>
        /// Event used to indicate that the application install process is complete.
        /// </summary>
        private ManualResetEvent mreAppInstall = new ManualResetEvent(false);

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
                Console.WriteLine();
                Console.WriteLine(GeneralUsageMessage);
                return;
            }

            DevicePortal portal = new DevicePortal(
                new DevicePortalConnection(
                    app.ipAddress, 
                    app.userName, 
                    app.password));
            portal.ConnectionStatus += app.ConnectionStatusHandler;
            portal.AppInstallStatus += app.AppInstallStatusHandler;

            app.mreConnected.Reset();
            Console.WriteLine("Connecting...");
            Task t = portal.Connect(
                app.ssid, 
                app.key);
            app.mreConnected.WaitOne();

            Console.WriteLine("Connected to: " + portal.Address);
            Console.WriteLine("OS version: " + portal.OperatingSystemVersion);
            Console.WriteLine("Device family: " + portal.DeviceFamily);
            Console.WriteLine("Platform: " + portal.PlatformName + " (" + portal.Platform.ToString() + ")");

            Task<string> getNameTask = portal.GetDeviceName();
            getNameTask.Wait();

            Task setIpdTask = portal.SetInterPupilaryDistance(67.5f);
            setIpdTask.Wait();

            Task<float> getIpdTask = portal.GetInterPupilaryDistance();
            getIpdTask.Wait();
            Console.WriteLine("IPD: " + getIpdTask.Result.ToString());

            Task<BatteryState> batteryTask = portal.GetBatteryState();
            batteryTask.Wait();
            Console.WriteLine("Battery level: " + batteryTask.Result.Level);

            Task<PowerState> powerTask = portal.GetPowerState();
            powerTask.Wait();
            Console.WriteLine("In low power state: " + powerTask.Result.InLowPowerState);

            while (true)
            {
                System.Threading.Thread.Sleep(0);
            }
        }

        /// <summary>
        /// Cleans up the object's data.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Cleans up the object's data.
        /// </summary>
        /// <param name="disposing">True if managed objects should be disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            { 
                this.mreConnected?.Dispose();
                this.mreConnected = null;

                this.mreAppInstall?.Dispose();
                this.mreAppInstall = null;
            }
        }

        /// <summary>
        /// Handler for the ApplicationInstallStatus event.
        /// </summary>
        /// <param name="sender">The object sending the event.</param>
        /// <param name="args">The event data.</param>
        private void AppInstallStatusHandler(
            object sender, 
            ApplicationInstallStatusEventArgs args)
        {
            Console.WriteLine(args.Message);

            if (args.Status != ApplicationInstallStatus.InProgress)
            {
                this.mreAppInstall.Set();
            }
        }

        /// <summary>
        /// Handler for the ConnectionStatus event.
        /// </summary>
        /// <param name="sender">The object sending the event.</param>
        /// <param name="args">The event data.</param>
        private void ConnectionStatusHandler(
            object sender, 
            DeviceConnectionStatusEventArgs args)
        {
            Console.WriteLine(args.Message);

            if ((args.Status == DeviceConnectionStatus.Connected) ||
                (args.Status == DeviceConnectionStatus.Failed))
            {
               this.mreConnected.Set();
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

        /// <summary>
        /// Parses the application's command line.
        /// </summary>
        /// <param name="args">Array of command line arguments.</param>
        private void ParseCommandLine(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i].ToLower();
                if (!arg.StartsWith("/'") && !arg.StartsWith("-"))
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
                else if (arg.StartsWith("ssid:"))
                {
                    this.ssid = this.GetArgData(args[i]);
                }
                else if (arg.StartsWith("key:"))
                {
                    this.key = this.GetArgData(args[i]);
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
    }
}
