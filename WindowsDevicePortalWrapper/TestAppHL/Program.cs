﻿using Microsoft.Tools.WindowsDevicePortal;
using static Microsoft.Tools.WindowsDevicePortal.DevicePortal;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TestApp
{
    class Program : IDisposable
    {
        private string ipAddress = null;
        private string userName = null;
        private string password = null;
        private string ssid = null;
        private string key = null;

        private ManualResetEvent mreConnected = new ManualResetEvent(false);
        private ManualResetEvent mreAppInstall = new ManualResetEvent(false);

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

            DevicePortal portal = new DevicePortal(new DevicePortalConnection(app.ipAddress, app.userName, app.password));
            portal.ConnectionStatus += app.ConnectionStatusHandler;
            portal.AppInstallStatus += app.AppInstallStatusHandler;

            app.mreConnected.Reset();
            Console.WriteLine("Connecting...");
            Task t = portal.Connect(app.ssid, app.key);
            app.mreConnected.WaitOne();

            Console.WriteLine("Connected to: " + portal.Address);
            Console.WriteLine("OS version: " + portal.OperatingSystemVersion);
            Console.WriteLine("Device family: " + portal.DeviceFamily);
//            Console.WriteLine("Device name: " + portal.GetDeviceName)
            Console.WriteLine("Platform: " + portal.Platform.ToString());

            Task <string> getNameTask = portal.GetDeviceName();
            getNameTask.Wait();

            Task <Single> getIpdTask = portal.GetInterPupilaryDistance();
            getIpdTask.Wait();
            Console.WriteLine("IPD: " + getIpdTask.Result.ToString());

            Task<BatteryState> batteryTask = portal.GetBatteryState();
            batteryTask.Wait();
            Console.WriteLine("Battery level: " + batteryTask.Result.Level);

            Task<PowerState> powerTask = portal.GetPowerState();
            powerTask.Wait();
            Console.WriteLine("In low power state: " + powerTask.Result.InLowPowerState);

            Task<Guid> activeSchemeTask = portal.GetActivePowerScheme();
            activeSchemeTask.Wait();
            Console.WriteLine("Active power scheme id: " + activeSchemeTask.Result.ToString());

            Task photoTask = portal.TakeMrcPhoto();
            photoTask.Wait();
            photoTask = portal.TakeMrcPhoto(includeColorCamera: false);
            photoTask.Wait();
            photoTask = portal.TakeMrcPhoto(includeHolograms: false);
            photoTask.Wait();

            Task startTask = portal.StartMrcRecording();
            startTask.Wait();
            System.Threading.Thread.Sleep(5000);
            Task stopTask = portal.StopMrcRecording();
            stopTask.Wait();
            startTask = portal.StartMrcRecording(includeMicrophone: false,
                                                includeAudio: false);
            startTask.Wait();
            System.Threading.Thread.Sleep(5000);
            stopTask = portal.StopMrcRecording();
            stopTask.Wait();
            startTask = portal.StartMrcRecording(includeColorCamera: false,
                                                includeAudio: false);
            startTask.Wait();
            System.Threading.Thread.Sleep(5000);
            stopTask = portal.StopMrcRecording();
            stopTask.Wait();

            Task<MrcFileList> fileListTask = portal.GetMrcFileList();
            fileListTask.Wait();
            MrcFileList mrcFileList = fileListTask.Result;
            Console.WriteLine(string.Format("Found {0} MRC files on your device", mrcFileList.Files.Count));
            foreach (MrcFileInformation fileInfo in mrcFileList.Files)
            {
                Console.WriteLine(string.Format("{0} : {1} {2} bytes", fileInfo.FileName, fileInfo.Created, fileInfo.FileSize));

                // TODO: Save the thumbnail
                // TODO: Download / save the file

                Task deleteTask = portal.DeleteMrcFile(fileInfo.FileName);
                deleteTask.Wait();
            }

            while(true)
            {
                System.Threading.Thread.Sleep(0);
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
                this.mreConnected?.Dispose();
                this.mreConnected = null;

                this.mreAppInstall?.Dispose();
                this.mreAppInstall = null;
            }
        }

        private void ConnectionStatusHandler(Object sender, DeviceConnectionStatusEventArgs args)
        {
            Console.WriteLine(args.Message);

            if ((args.Status == DeviceConnectionStatus.Connected) ||
                (args.Status == DeviceConnectionStatus.Failed))
            {
               this.mreConnected.Set();
            }
        }

        private void AppInstallStatusHandler(Object sender, ApplicationInstallStatusEventArgs args)
        {
            Console.WriteLine(args.Message);

            if (args.Status != ApplicationInstallStatus.InProgress)
            {
                this.mreAppInstall.Set();
            }
        }

        private string GetArgData(string arg)
        {
            Int32 idx = arg.IndexOf(':');
            return arg.Substring(idx+1);
        }

        private void ParseCommandLine(string[] args)
        {
            for (Int32 i = 0; i < args.Length; i++)
            {
                string arg = args[i].ToLower();
                if (!arg.StartsWith("/'") && !arg.StartsWith("-"))
                {
                    throw new Exception(string.Format("Unrecognized argument: {0}", args[i]));
                }

                arg = arg.Substring(1);

                if (arg.StartsWith("ip:"))
                {
                    this.ipAddress = GetArgData(args[i]);
                }
                else if (arg.StartsWith("user:"))
                {
                    this.userName = GetArgData(args[i]);
                }
                else if (arg.StartsWith("pwd:"))
                {
                    this.password = GetArgData(args[i]);
                }
                else if (arg.StartsWith("ssid:"))
                {
                    this.ssid = GetArgData(args[i]);
                }
                else if (arg.StartsWith("key:"))
                {
                    this.key = GetArgData(args[i]);
                }
                else
                {
                    throw new Exception(string.Format("Unrecognized argument: {0}", args[i]));
                }
            }

            // We require at least a user name and password to proceed.
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
            {
                throw new Exception("You must specify a user name and a password");
            }
        }
    }
}
