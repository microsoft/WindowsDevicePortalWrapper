using Microsoft.Tools.WindowsDevicePortal;
using System;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.Tools.WindowsDevicePortal.DevicePortal;

namespace TestApp
{
    class Program : IDisposable
    {
        private string _ipAddress = null;
        private string _userName = null;
        private string _password = null;
        private string _ssid = null;
        private string _key = null;

        private ManualResetEvent _mreConnected = new ManualResetEvent(false);
        private ManualResetEvent _mreAppInstall = new ManualResetEvent(false);

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
            portal.ConnectionStatus += app.ConnectionStatusHandler;
            portal.AppInstallStatus += app.AppInstallStatusHandler;

            app._mreConnected.Reset();
            Console.WriteLine("Connecting...");
            Task t = portal.Connect(app._ssid, app._key);
            app._mreConnected.WaitOne();

            Console.WriteLine("Connected to: " + portal.Address);
            Console.WriteLine("OS version: " + portal.OperatingSystemVersion);
            Console.WriteLine("Platform: " + portal.Platform.ToString());

            Task <string> getNameTask = portal.GetDeviceName();
            getNameTask.Wait();
            Console.WriteLine("Device name: " + getNameTask.Result);

            Task <Single> getIpdTask = portal.GetInterPupilaryDistance();
            getIpdTask.Wait();
            Console.WriteLine("IPD: " + getIpdTask.Result.ToString());

            Task<BatteryState> batteryTask = portal.GetBatteryState();
            batteryTask.Wait();
            Console.WriteLine("Battery level: " + batteryTask.Result.Level);

            Task<PowerState> powerTask = portal.GetPowerState();
            powerTask.Wait();
            Console.WriteLine("In low power state: " + powerTask.Result.InLowPowerState);
                
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
                _mreConnected?.Dispose();
                _mreConnected = null;

                _mreAppInstall?.Dispose();
                _mreAppInstall = null;
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

        private void AppInstallStatusHandler(Object sender, ApplicationInstallStatusEventArgs args)
        {
            Console.WriteLine(args.Message);

            if (args.Status != ApplicationInstallStatus.InProgress)
            {
                _mreAppInstall.Set();
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
                    _ipAddress = GetArgData(args[i]);
                }
                else if (arg.StartsWith("user:"))
                {
                    _userName = GetArgData(args[i]);
                }
                else if (arg.StartsWith("pwd:"))
                {
                    _password = GetArgData(args[i]);
                }
                else if (arg.StartsWith("ssid:"))
                {
                    _ssid = GetArgData(args[i]);
                }
                else if (arg.StartsWith("key:"))
                {
                    _key = GetArgData(args[i]);
                }
                else
                {
                    throw new Exception(string.Format("Unrecognized argument: {0}", args[i]));
                }
            }

            // We require at least a user name and password to proceed.
            if (string.IsNullOrWhiteSpace(_userName) || string.IsNullOrWhiteSpace(_password))
            {
                    throw new Exception("You must specify a user name and a password");
            }
        }
    }
}
