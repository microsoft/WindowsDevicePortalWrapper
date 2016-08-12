using Microsoft.Tools.WindowsDevicePortal;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using static Microsoft.Tools.WindowsDevicePortal.DevicePortal;

namespace DeviceLab
{
    public class DevicePortalViewModel : BindableBase
    {
        //-------------------------------------------------------------------
        //  Private Members
        //-------------------------------------------------------------------
        #region Private Members
        private IDiagnosticSink diagnostics;

        private string diagnosticMoniker
        {
            get
            {
                return !string.IsNullOrWhiteSpace(this.deviceName) ? this.deviceName : this.Address;
            }
        }
        #endregion // Private Members
        
        //-------------------------------------------------------------------
        //  Constructors
        //-------------------------------------------------------------------
        #region Constructors
        public DevicePortalViewModel(DevicePortal portal, IDiagnosticSink diags)
        {
            this.diagnostics = diags;
            this.portal = portal;
            this.Ready = true;
            Task t = GetDeviceNameAsync();
            HookupWebSocket();
        }
        #endregion // Cosntructors

        //-------------------------------------------------------------------
        //  Properties
        //-------------------------------------------------------------------
        #region Properties
        #region Portal
        private DevicePortal portal;

        private DevicePortal Portal
        {
            get { return this.portal; }
            set
            {
                SetProperty(ref this.portal, value);
                OnPropertyChanged("Address");
                OnPropertyChanged("DeviceFamily");
                OnPropertyChanged("OperatingSystemVersion");
                OnPropertyChanged("Platform");
                OnPropertyChanged("PlatformName");
            }
        }
        #endregion // Portal

        #region Ready
        private bool ready;
        public bool Ready
        {
            get
            {
                return this.ready;
            }
            private set
            {
                SetProperty(ref this.ready, value);
            }
        }
        #endregion // Ready

        #region DeviceName
        private string deviceName;
        public string DeviceName
        {
            get
            {
                return string.IsNullOrWhiteSpace(this.deviceName) ? "<unknown>" : this.deviceName;
            }

            private set
            {
                SetProperty(ref this.deviceName, value);
            }
        }
        #endregion //DeviceName

        #region DeviceNameEntry
        private string deviceNameEntry;
        public string DeviceNameEntry
        {
            get
            {
                return this.deviceNameEntry;
            }

            set
            {
                SetProperty(ref this.deviceNameEntry, value);
            }
        }
        #endregion // DeviceNameEntry

        #region Address
        public string Address
        {
            get
            {
                return this.portal == null ? "<unknown>" : this.portal.Address;
            }
        }
        #endregion // Address

        #region DeviceFamily
        public string DeviceFamily
        {
            get
            {
                return this.portal == null ? "<unknown>" : this.portal.DeviceFamily;
            }
        }
        #endregion // DeviceFamily

        #region OperatingSystemVersion
        public string OperatingSystemVersion
        {
            get
            {
                return this.portal == null ? "<unknown>" : this.portal.OperatingSystemVersion;
            }
        }
        #endregion // OperatingSystemVersion

        #region Platform
        public string Platform
        {
            get
            {
                return this.portal == null ? "<unknown>" : this.portal.Platform.ToString();
            }
        }
        #endregion // Platform

        #region PlatformName
        #endregion // PlatformName
        public string PlatformName
        {
            get
            {
                return this.portal == null ? "<unknown>" : this.portal.PlatformName;
            }
        }

        #region // CPULoad
        private int cpuLoad;
        public string CPULoad
        {
            get
            {
                return this.cpuLoad.ToString();
            }
        }
        #endregion // CPULoad
        #endregion // Properties

        //-------------------------------------------------------------------
        //  Commands
        //-------------------------------------------------------------------
        #region Commands

        #region RenameCommand
        private DelegateCommand renameCommand;
        public ICommand RenameCommand
        {
            get
            {
                if(this.renameCommand == null)
                {
                    this.renameCommand = DelegateCommand.FromAsyncHandler(ExecuteRenameAsync, CanExecuteRename);
                    this.renameCommand.ObservesProperty(() => this.Ready);
                    this.renameCommand.ObservesProperty(() => this.DeviceNameEntry);
                }
                return this.renameCommand;
            }
        }

        private bool CanExecuteRename()
        {
            return
                this.Ready &&
                !string.IsNullOrWhiteSpace(this.deviceNameEntry);
        }

        private async Task ExecuteRenameAsync()
        {
            this.Ready = false;
            try
            {
                string newName = this.deviceNameEntry;
                this.DeviceNameEntry = "";
                this.diagnostics.OutputDiagnosticString("[{0}] Attempting to rename device to {1}\n", this.diagnosticMoniker, newName);
                await this.Portal.SetDeviceName(newName);
            }
            catch (Exception exn)
            {
                ReportException("Rename", exn);
            }
            await ExecuteRebootAsync();
            if (this.Ready)
            {
                await GetDeviceNameAsync();
            }
        }

        private async Task GetDeviceNameAsync()
        {
            this.Ready = false;
            try
            {
                this.DeviceName = await this.Portal.GetDeviceName();
            }
            catch (Exception exn)
            {
                ReportException("Rename", exn);
            }
            this.diagnostics.OutputDiagnosticString("[{0}] Retrieved device name.\n", this.diagnosticMoniker);
            this.Ready = true;
        }
        #endregion // RenameCommand

        #region Reboot Command
        private DelegateCommand rebootCommand;
        public ICommand RebootCommand
        {
            get
            {
                if(this.rebootCommand == null)
                {
                    this.rebootCommand = DelegateCommand.FromAsyncHandler(ExecuteRebootAsync, CanExecuteReboot);
                    this.rebootCommand.ObservesProperty(() => this.Ready);
                }
                return this.rebootCommand;
            }
        }

        private bool CanExecuteReboot()
        {
            return this.Ready;
        }

        private async Task ExecuteRebootAsync()
        {
            this.Ready = false;
            try
            {
                this.diagnostics.OutputDiagnosticString("[{0}] Attempting to reboot device.\n", this.diagnosticMoniker);
                await this.Portal.Reboot();
            }
            catch (Exception exn)
            {
                ReportException("Rename", exn);
            }
            await ExecuteReestablishConnectionAsync();
        }
        #endregion // Reboot Command

        #region ReestablishConnectionCommand
        private DelegateCommand reestablishConnectionCommand;
        public ICommand ReestablishConnectionCommand
        {
            get
            {
                if(this.reestablishConnectionCommand == null)
                {
                    this.reestablishConnectionCommand = DelegateCommand.FromAsyncHandler(ExecuteReestablishConnectionAsync, CanExecuteReestablishConnection);
                    this.reestablishConnectionCommand.ObservesProperty(() => this.Ready);
                }
                return this.reestablishConnectionCommand;
            }
        }

        private bool CanExecuteReestablishConnection()
        {
            return this.Ready;
        }

        private async Task ExecuteReestablishConnectionAsync()
        {
            int numTries = 1;

            DeviceConnectionStatusEventHandler handler = (DevicePortal sender, DeviceConnectionStatusEventArgs args) =>
            {
                this.diagnostics.OutputDiagnosticString("[{0}] Connection status update: Status: {1}, Phase: {2}\n", this.diagnosticMoniker, args.Status, args.Phase);
                if (args.Status == DeviceConnectionStatus.Connected)
                {
                    this.diagnostics.OutputDiagnosticString("[{0}] Connection succeeded after {1} tries.\n", this.diagnosticMoniker, numTries);
                }
                else if (args.Status == DeviceConnectionStatus.Failed)
                {
                    this.diagnostics.OutputDiagnosticString("[{0}] Connection failed after {1} tries.\n", this.diagnosticMoniker, numTries);
                }
            };
            this.portal.ConnectionStatus += handler;

            this.Ready = false;
            try
            {
                do
                {
                    await this.portal.Connect();
                    ++numTries;
                } while (this.portal.ConnectionHttpStatusCode != HttpStatusCode.OK);
                this.Ready = true;
            }
            catch (Exception exn)
            {
                ReportException("Rename", exn);
            }
            this.portal.ConnectionStatus -= handler;
        }
        #endregion // ReestablishConnectionCommand



        #endregion // Commands

        private async void HookupWebSocket()
        {
            this.Ready = false;
            this.portal.SystemPerfMessageReceived += OnSystemPerfReceived;
            await this.portal.StartListeningForSystemPerf();
            this.Ready = true;
        }

        private void OnSystemPerfReceived(DevicePortal sender, WebSocketMessageReceivedEventArgs<DevicePortal.SystemPerformanceInformation> args)
        {
            this.cpuLoad = args.Message.CpuLoad;
            OnPropertyChanged("CPULoad");
        }

        private void ReportException(string commandName, Exception exn)
        {
            this.diagnostics.OutputDiagnosticString(
                "[{0}] Exception during {1} command:\n[{0}] {2}\nStackTrace: \n[{0}] {3}\n",
                this.diagnosticMoniker, commandName, exn.Message, exn.StackTrace);
        }
    }
}
