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
    public class DevicePortalViewModel : DevicePortalCommandModel
    {
        //-------------------------------------------------------------------
        //  Constructors
        //-------------------------------------------------------------------
        #region Constructors
        public DevicePortalViewModel(DevicePortal portal, IDiagnosticSink diags)
            : base(portal, diags)
        {
            this.ConnectionRetryAttempts = 5;
        }
        #endregion // Cosntructors

        //-------------------------------------------------------------------
        //  Properties
        //-------------------------------------------------------------------
        #region Properties
        #region Diagnostic Moniker
        private string diagnosticMoniker
        {
            get
            {
                return !string.IsNullOrWhiteSpace(this.deviceName) ? this.deviceName : this.Address;
            }
        }
        #endregion // Diagnostic Moniker

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
        public string PlatformName
        {
            get
            {
                return this.portal == null ? "<unknown>" : this.portal.PlatformName;
            }
        }
        #endregion // PlatformName

        #region CPULoad
        private int cpuLoad;
        public string CPULoad
        {
            get
            {
                return this.cpuLoad.ToString();
            }
        }
        #endregion // CPULoad

        #region ConnectionRetryAttempts
        private int connectionRetryAttempts;
        public int ConnectionRetryAttempts
        {
            get
            {
                return this.connectionRetryAttempts;
            }

            set
            {
                SetProperty(ref this.connectionRetryAttempts, value);
            }
        }
        #endregion // ConnectionRetryAttempts
        #endregion // Properties

        //-------------------------------------------------------------------
        //  Commands
        //-------------------------------------------------------------------
        #region Commands

        #region RenameCommand
        private CommandSequence renameCommand;
        public ICommand RenameCommand
        {
            get
            {
                if(this.renameCommand == null)
                {
                    this.renameCommand = CreateCommandSequence();
                    DelegateCommand renameDC = DelegateCommand.FromAsyncHandler(ExecuteRenameAsync, CanExecuteRename);
                    renameDC.ObservesProperty(() => this.Ready);
                    renameDC.ObservesProperty(() => this.DeviceNameEntry);
                    this.renameCommand.RegisterCommand(renameDC);
                    this.renameCommand.RegisterCommand(this.RebootCommand);
                    this.renameCommand.RegisterCommand(this.RefreshDeviceNameCommand);
                }
                return this.renameCommand;
            }
        }

        private bool CanExecuteRename()
        {
            //return
            //    this.Ready;
            return
                this.Ready &&
                !string.IsNullOrWhiteSpace(this.deviceNameEntry);
        }

        private async Task ExecuteRenameAsync()
        {
            this.diagnostics.OutputDiagnosticString("[{0}] ExecuteRenameAsync\n", this.diagnosticMoniker);
            this.Ready = false;
            try
            {
                string newName = this.deviceNameEntry;
                this.DeviceNameEntry = "";
                this.diagnostics.OutputDiagnosticString("[{0}] Attempting to rename device to {1}\n", this.diagnosticMoniker, newName);
                await this.portal.SetDeviceName(newName);
            }
            catch (Exception exn)
            {
                ReportException("Rename", exn);
            }
            this.Ready = true;
        }
        #endregion // RenameCommand

        #region RefreshDeviceName
        private CommandSequence refreshDeviceNameCommand;
        public ICommand RefreshDeviceNameCommand
        {
            get
            {
                if(this.refreshDeviceNameCommand == null)
                {
                    DelegateCommand refreshDeviceNameDC = DelegateCommand.FromAsyncHandler(RefreshDeviceNameAsync, CanExecuteRefreshDeviceName);
                    refreshDeviceNameDC.ObservesProperty(() => this.Ready);
                    this.refreshDeviceNameCommand = CreateCommandSequence();
                    this.refreshDeviceNameCommand.RegisterCommand(refreshDeviceNameDC);
                }
                return this.refreshDeviceNameCommand;
            }
        }

        private bool CanExecuteRefreshDeviceName()
        {
            return this.Ready;
        }

        private async Task RefreshDeviceNameAsync()
        {
            this.diagnostics.OutputDiagnosticString("[{0}] RefreshDeviceNameAsync\n", this.diagnosticMoniker);
            this.Ready = false;
            try
            {
                this.DeviceName = await this.portal.GetDeviceName();
            }
            catch (Exception exn)
            {
                ReportException("RefreshDeviceName", exn);
            }
            this.Ready = true;
        }
        #endregion // RefreshDeviceName

        #region Reboot Command
        private CommandSequence rebootCommand;
        public ICommand RebootCommand
        {
            get
            {
                if(this.rebootCommand == null)
                {
                    this.rebootCommand = CreateCommandSequence();
                    DelegateCommand rebootDC = DelegateCommand.FromAsyncHandler(ExecuteRebootAsync, CanExecuteReboot);
                    rebootDC.ObservesProperty(() => this.Ready);
                    this.rebootCommand.RegisterCommand(StopListeningForSystemPerfCommand);
                    this.rebootCommand.RegisterCommand(rebootDC);
                    this.rebootCommand.RegisterCommand(this.ReestablishConnectionCommand);
                    this.rebootCommand.RegisterCommand(this.refreshDeviceNameCommand);
                    this.rebootCommand.RegisterCommand(this.StartListeningForSystemPerfCommand);
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
            this.diagnostics.OutputDiagnosticString("[{0}] ExecuteRebootAsync\n", this.diagnosticMoniker);
            this.Ready = false;
            try
            {
                this.diagnostics.OutputDiagnosticString("[{0}] Attempting to reboot device.\n", this.diagnosticMoniker);
                await this.portal.Reboot();

                // Sometimes able to reestablish the connection prematurely before the console has a chance to shut down
                // So adding a delay here before trying to reestablish the connection.
                await Task.Delay(1000 * 5);
            }
            catch (Exception exn)
            {
                ReportException("Reboot", exn);
            }
            this.Ready = true;
        }
        #endregion // Reboot Command

        #region ReestablishConnectionCommand
        private CommandSequence reestablishConnectionCommand;
        public ICommand ReestablishConnectionCommand
        {
            get
            {
                if(this.reestablishConnectionCommand == null)
                {
                    this.reestablishConnectionCommand = CreateCommandSequence();
                    DelegateCommand reestablishConnectionDC = DelegateCommand.FromAsyncHandler(ExecuteReestablishConnectionAsync, CanExecuteReestablishConnection);
                    reestablishConnectionDC.ObservesProperty(() => this.Ready);
                    this.reestablishConnectionCommand.RegisterCommand(reestablishConnectionDC);
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
            this.diagnostics.OutputDiagnosticString("[{0}] ExecuteReestablishConnectionAsync\n", this.diagnosticMoniker);
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
                } while (this.portal.ConnectionHttpStatusCode != HttpStatusCode.OK && numTries < this.ConnectionRetryAttempts);
            }
            catch (Exception exn)
            {
                ReportException("ReestablishConnection", exn);
            }
            this.portal.ConnectionStatus -= handler;
            this.Ready = true;
        }
        #endregion // ReestablishConnectionCommand

        #region StartListeningForSystemPerfCommand
        private CommandSequence startListeningForSystemPerfCommand;
        public ICommand StartListeningForSystemPerfCommand
        {
            get
            {
                if(this.startListeningForSystemPerfCommand == null)
                {
                    this.startListeningForSystemPerfCommand = CreateCommandSequence();
                    DelegateCommand startListeningForSystemPerfDC = DelegateCommand.FromAsyncHandler(ExecuteStartListeningForSystemPerfAsync, CanStartListeningForSystemPerf);
                    startListeningForSystemPerfDC.ObservesProperty(() => this.Ready);
                    this.startListeningForSystemPerfCommand.RegisterCommand(startListeningForSystemPerfDC);
                }
                return startListeningForSystemPerfCommand;
            }
        }

        private bool CanStartListeningForSystemPerf()
        {
            return this.Ready;
        }

        private async Task ExecuteStartListeningForSystemPerfAsync()
        {
            this.diagnostics.OutputDiagnosticString("[{0}] ExecuteStartListeningForSystemPerfAsync\n", this.diagnosticMoniker);
            this.Ready = false;
            try
            {
                this.portal.SystemPerfMessageReceived += OnSystemPerfReceived;
                await Task.Run(StartListeningHelper);
            }
            catch(Exception exn)
            {
                ReportException("StartListeningForSystemPerf", exn);
            }
            this.Ready = true;
        }

        private async Task StartListeningHelper()
        {
            await this.portal.StartListeningForSystemPerf().ConfigureAwait(false);
        }
        #endregion // StartListeningForSystemPerfCommand

        #region StopListeningForSystemPerfCommand
        private CommandSequence stopListeningForSystemPerfCommand;
        public ICommand StopListeningForSystemPerfCommand
        {
            get
            {
                if(this.stopListeningForSystemPerfCommand == null)
                {
                    this.stopListeningForSystemPerfCommand = CreateCommandSequence();
                    DelegateCommand stopListeningForSystemPerfDC = DelegateCommand.FromAsyncHandler(ExecuteStopListeningForSystemPerfAsync, CanStopListeningForSystemPerf);
                    stopListeningForSystemPerfDC.ObservesProperty(() => this.Ready);
                    this.stopListeningForSystemPerfCommand.RegisterCommand(stopListeningForSystemPerfDC);
                }
                return stopListeningForSystemPerfCommand;
            }
        }

        private bool CanStopListeningForSystemPerf()
        {
            return this.Ready;
        }

        private async Task ExecuteStopListeningForSystemPerfAsync()
        {
            this.diagnostics.OutputDiagnosticString("[{0}] ExecuteStopListeningForSystemPerfAsync\n", this.diagnosticMoniker);
            this.Ready = false;
            try
            {
                this.portal.SystemPerfMessageReceived -= OnSystemPerfReceived;
                await StopListeningHelper();
                await StopListeningHelper();
            }
            catch(Exception exn)
            {
                ReportException("StopListeningForSystemPerf", exn);
            }
            this.Ready = true;
        }

        private async Task StopListeningHelper()
        {
            await this.portal.StopListeningForSystemPerf();
        }
        #endregion // StopListeningForSystemPerfCommand
        #endregion // Commands

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

            // Clear the command queue to prevent executing any more commands
            this.diagnostics.OutputDiagnosticString("[{0}] Clearing any queued commands\n", this.diagnosticMoniker);
            ClearCommandQueue();
        }
    }
}
