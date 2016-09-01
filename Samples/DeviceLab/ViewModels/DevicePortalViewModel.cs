//----------------------------------------------------------------------------------------------
// <copyright file="DevicePortalViewModel.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------
using System;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Tools.WindowsDevicePortal;
using Prism.Commands;

namespace DeviceLab
{
    /// <summary>
    /// Viewmodel for a single connected device
    /// </summary>
    public class DevicePortalViewModel : DevicePortalCommandModel
    {
        //-------------------------------------------------------------------
        //  Constructors
        //-------------------------------------------------------------------
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="DevicePortalViewModel" /> class.
        /// </summary>
        /// <param name="portal">DevicePortal object enscapsulated by this</param>
        /// <param name="diags">Diagnostic sink for reporting</param>
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
        /// <summary>
        /// Gets the overriden DiagnosticMoniker for the device. The overriden
        /// version will use the friendly name for the device or otherwise
        /// default to the IP address.
        /// </summary>
        protected override string DiagnosticMoniker
        {
            get
            {
                return !string.IsNullOrWhiteSpace(this.deviceName) ? this.deviceName : base.DiagnosticMoniker;
            }
        }
        #endregion // Diagnostic Moniker

        #region DeviceName
        /// <summary>
        /// The friendly name for the device
        /// </summary>
        private string deviceName;

        /// <summary>
        /// Gets the freindly name for the device
        /// </summary>
        public string DeviceName
        {
            get
            {
                return string.IsNullOrWhiteSpace(this.deviceName) ? "<unknown>" : this.deviceName;
            }

            private set
            {
                this.SetProperty(ref this.deviceName, value);
            }
        }
        #endregion //DeviceName

        #region DeviceNameEntry
        /// <summary>
        /// User input from the user to be used as a new device name
        /// </summary>
        private string deviceNameEntry;

        /// <summary>
        /// Gets or sets a value representing input from the user to be used as a new device name
        /// </summary>
        public string DeviceNameEntry
        {
            get
            {
                return this.deviceNameEntry;
            }

            set
            {
                this.SetProperty(ref this.deviceNameEntry, value);
            }
        }
        #endregion // DeviceNameEntry

        #region CPULoad
        /// <summary>
        /// The CPU Load on the device
        /// </summary>
        private int cpuLoad;

        /// <summary>
        ///  Gets a value representing the CPU load on the device
        /// </summary>
        public string CPULoad
        {
            get
            {
                return this.cpuLoad.ToString();
            }
        }
        #endregion // CPULoad

        #region ConnectionRetryAttempts
        /// <summary>
        /// The number of times to attempt to connect to the device
        /// </summary>
        private int connectionRetryAttempts;

        /// <summary>
        /// Gets or sets the number of time to attempt to connect to the device
        /// </summary>
        public int ConnectionRetryAttempts
        {
            get
            {
                return this.connectionRetryAttempts;
            }

            set
            {
                this.SetProperty(ref this.connectionRetryAttempts, value);
            }
        }
        #endregion // ConnectionRetryAttempts
        #endregion // Properties

        //-------------------------------------------------------------------
        //  Commands
        //-------------------------------------------------------------------
        #region Commands
        #region RenameCommand
        /// <summary>
        /// Command to rename the device
        /// </summary>
        private CommandSequence renameCommand;

        /// <summary>
        /// Gets the command to rename the device
        /// </summary>
        public ICommand RenameCommand
        {
            get
            {
                if (this.renameCommand == null)
                {
                    this.renameCommand = this.CreateCommandSequence();
                    DelegateCommand renameDC = DelegateCommand.FromAsyncHandler(this.ExecuteRenameAsync, this.CanExecuteRename);
                    renameDC.ObservesProperty(() => this.Ready);
                    renameDC.ObservesProperty(() => this.DeviceNameEntry);
                    this.renameCommand.RegisterCommand(renameDC);
                    this.renameCommand.RegisterCommand(this.RebootCommand);
                    this.renameCommand.RegisterCommand(this.RefreshDeviceNameCommand);
                }

                return this.renameCommand;
            }
        }

        /// <summary>
        /// Predicate for the rename command
        /// </summary>
        /// <returns>Result indicates whether the rename command can execute</returns>
        private bool CanExecuteRename()
        {
            //return
            //    this.Ready;
            return
                this.Ready &&
                !string.IsNullOrWhiteSpace(this.deviceNameEntry);
        }

        /// <summary>
        /// Performs the action for the rename command
        /// </summary>
        /// <returns>A task capturing the continuation of setting the device name</returns>
        private async Task ExecuteRenameAsync()
        {
            this.OutputDiagnosticString("ExecuteRenameAsync\n");
            this.Ready = false;
            try
            {
                string newName = this.deviceNameEntry;
                this.DeviceNameEntry = string.Empty;
                this.OutputDiagnosticString("Attempting to rename device to {0}\n", newName);
                await this.Portal.SetDeviceName(newName);
            }
            catch (Exception exn)
            {
                this.ReportException("Rename", exn);
            }

            this.Ready = true;
        }
        #endregion // RenameCommand

        #region RefreshDeviceName
        /// <summary>
        /// Retreive the friendly device name from the device
        /// </summary>
        private CommandSequence refreshDeviceNameCommand;

        /// <summary>
        /// Gets the command for retrieving the device name
        /// </summary>
        public ICommand RefreshDeviceNameCommand
        {
            get
            {
                if (this.refreshDeviceNameCommand == null)
                {
                    DelegateCommand refreshDeviceNameDC = DelegateCommand.FromAsyncHandler(this.RefreshDeviceNameAsync, this.CanExecuteRefreshDeviceName);
                    refreshDeviceNameDC.ObservesProperty(() => this.Ready);
                    this.refreshDeviceNameCommand = this.CreateCommandSequence();
                    this.refreshDeviceNameCommand.RegisterCommand(refreshDeviceNameDC);
                }

                return this.refreshDeviceNameCommand;
            }
        }

        /// <summary>
        /// Predicate indicates whether refresh device name command can execute
        /// </summary>
        /// <returns>Whether the command can execute</returns>
        private bool CanExecuteRefreshDeviceName()
        {
            return this.Ready;
        }

        /// <summary>
        /// Performs the action of the refresh device name command
        /// </summary>
        /// <returns>A task that captures the continuation of the refresh device name action</returns>
        private async Task RefreshDeviceNameAsync()
        {
            this.OutputDiagnosticString("RefreshDeviceNameAsync\n");
            this.Ready = false;
            try
            {
                this.DeviceName = await this.Portal.GetDeviceName();
            }
            catch (Exception exn)
            {
                this.ReportException("RefreshDeviceName", exn);
            }

            this.Ready = true;
        }
        #endregion // RefreshDeviceName

        #region Reboot Command
        /// <summary>
        /// Command to reboot the device
        /// </summary>
        private CommandSequence rebootCommand;

        /// <summary>
        ///  Gets the command for rebooting the device
        /// </summary>
        public ICommand RebootCommand
        {
            get
            {
                if (this.rebootCommand == null)
                {
                    this.rebootCommand = this.CreateCommandSequence();
                    DelegateCommand rebootDC = DelegateCommand.FromAsyncHandler(this.ExecuteRebootAsync, this.CanExecuteReboot);
                    rebootDC.ObservesProperty(() => this.Ready);
                    this.rebootCommand.RegisterCommand(this.StopListeningForSystemPerfCommand);
                    this.rebootCommand.RegisterCommand(rebootDC);
                    this.rebootCommand.RegisterCommand(this.ReestablishConnectionCommand);
                    this.rebootCommand.RegisterCommand(this.refreshDeviceNameCommand);
                    this.rebootCommand.RegisterCommand(this.StartListeningForSystemPerfCommand);
                }

                return this.rebootCommand;
            }
        }

        /// <summary>
        /// Predicate for determining whether the reboot command can execute
        /// </summary>
        /// <returns>Whether or not the reboot command can execute</returns>
        private bool CanExecuteReboot()
        {
            return this.Ready;
        }

        /// <summary>
        /// Performs the action of the reboot command
        /// </summary>
        /// <returns>A task that captures the continuation of the asynchronous reboot action</returns>
        private async Task ExecuteRebootAsync()
        {
            this.OutputDiagnosticString("ExecuteRebootAsync\n");
            this.Ready = false;
            try
            {
                this.OutputDiagnosticString("Attempting to reboot device.\n");
                await this.Portal.Reboot();

                // Sometimes able to reestablish the connection prematurely before the console has a chance to shut down
                // So adding a delay here before trying to reestablish the connection.
                await Task.Delay(1000 * 5);
            }
            catch (Exception exn)
            {
                this.ReportException("Reboot", exn);
            }

            this.Ready = true;
        }
        #endregion // Reboot Command

        #region ReestablishConnectionCommand
        /// <summary>
        /// Command to reestablish the connection with the device
        /// </summary>
        private CommandSequence reestablishConnectionCommand;

        /// <summary>
        /// Gets the command for reestablishing the connection with the device
        /// </summary>
        public ICommand ReestablishConnectionCommand
        {
            get
            {
                if (this.reestablishConnectionCommand == null)
                {
                    this.reestablishConnectionCommand = this.CreateCommandSequence();
                    DelegateCommand reestablishConnectionDC = DelegateCommand.FromAsyncHandler(this.ExecuteReestablishConnectionAsync, this.CanExecuteReestablishConnection);
                    reestablishConnectionDC.ObservesProperty(() => this.Ready);
                    this.reestablishConnectionCommand.RegisterCommand(reestablishConnectionDC);
                }

                return this.reestablishConnectionCommand;
            }
        }

        /// <summary>
        /// Predicate for the reestablish connection command
        /// </summary>
        /// <returns>Indicates whether the reestablish connection command can execute.</returns>
        private bool CanExecuteReestablishConnection()
        {
            return this.Ready;
        }

        /// <summary>
        /// Performs the action of the reestablish connection command
        /// </summary>
        /// <returns>A task capturing the continuation of the asynchronous action to reestablish the connection</returns>
        private async Task ExecuteReestablishConnectionAsync()
        {
            this.OutputDiagnosticString("ExecuteReestablishConnectionAsync\n");
            int numTries = 1;

            DeviceConnectionStatusEventHandler handler = (DevicePortal sender, DeviceConnectionStatusEventArgs args) =>
            {
                this.OutputDiagnosticString("Connection status update: Status: {0}, Phase: {1}\n", args.Status, args.Phase);
                if (args.Status == DeviceConnectionStatus.Connected)
                {
                    this.OutputDiagnosticString("Connection succeeded after {0} tries.\n", numTries);
                }
                else if (args.Status == DeviceConnectionStatus.Failed)
                {
                    this.OutputDiagnosticString("Connection failed after {0} tries.\n", numTries);
                }
            };
            this.Portal.ConnectionStatus += handler;

            this.Ready = false;
            try
            {
                do
                {
                    await this.Portal.Connect();
                    ++numTries;
                }
                while (this.Portal.ConnectionHttpStatusCode != HttpStatusCode.OK && numTries < this.ConnectionRetryAttempts);
            }
            catch (Exception exn)
            {
                this.ReportException("ReestablishConnection", exn);
            }

            this.Portal.ConnectionStatus -= handler;
            this.Ready = true;
        }
        #endregion // ReestablishConnectionCommand

        #region StartListeningForSystemPerfCommand
        /// <summary>
        /// Command to start listing for asynchronous system performance updates
        /// </summary>
        private CommandSequence startListeningForSystemPerfCommand;

        /// <summary>
        /// Gets the command to start listing for system performance updates
        /// </summary>
        public ICommand StartListeningForSystemPerfCommand
        {
            get
            {
                if (this.startListeningForSystemPerfCommand == null)
                {
                    this.startListeningForSystemPerfCommand = this.CreateCommandSequence();
                    DelegateCommand startListeningForSystemPerfDC = DelegateCommand.FromAsyncHandler(this.ExecuteStartListeningForSystemPerfAsync, this.CanStartListeningForSystemPerf);
                    startListeningForSystemPerfDC.ObservesProperty(() => this.Ready);
                    this.startListeningForSystemPerfCommand.RegisterCommand(startListeningForSystemPerfDC);
                }

                return this.startListeningForSystemPerfCommand;
            }
        }

        /// <summary>
        /// Predicate for the command to start listening for system performance updates
        /// </summary>
        /// <returns>Result indicates whether the command can execute</returns>
        private bool CanStartListeningForSystemPerf()
        {
            return this.Ready;
        }

        /// <summary>
        /// Performs the action of the command to start listening for system performance updates
        /// </summary>
        /// <returns>A task that captures the continuation of the action to start listening for system performance updates</returns>
        private async Task ExecuteStartListeningForSystemPerfAsync()
        {
            this.OutputDiagnosticString("ExecuteStartListeningForSystemPerfAsync\n");
            this.Ready = false;
            try
            {
                this.Portal.SystemPerfMessageReceived += this.OnSystemPerfReceived;
                await Task.Run(this.StartListeningHelper);
            }
            catch (Exception exn)
            {
                this.ReportException("StartListeningForSystemPerf", exn);
            }

            this.Ready = true;
        }

        /// <summary>
        /// StartListeningForSystemPerf would deadlock when called from the main thread.
        /// This helper exists so that the asynchronous operation can be run on the
        /// thread pool and avoid the deadlock
        /// </summary>
        /// <returns>A task the captures the continuation of the start listening call</returns>
        private async Task StartListeningHelper()
        {
            await this.Portal.StartListeningForSystemPerf().ConfigureAwait(false);
        }
        #endregion // StartListeningForSystemPerfCommand

        #region StopListeningForSystemPerfCommand
        /// <summary>
        /// Command to stop listing to asynchronous system performance updates
        /// </summary>
        private CommandSequence stopListeningForSystemPerfCommand;

        /// <summary>
        /// Gets the command to stop listening to asynchronous system performance updates
        /// </summary>
        public ICommand StopListeningForSystemPerfCommand
        {
            get
            {
                if (this.stopListeningForSystemPerfCommand == null)
                {
                    this.stopListeningForSystemPerfCommand = this.CreateCommandSequence();
                    DelegateCommand stopListeningForSystemPerfDC = DelegateCommand.FromAsyncHandler(this.ExecuteStopListeningForSystemPerfAsync, this.CanStopListeningForSystemPerf);
                    stopListeningForSystemPerfDC.ObservesProperty(() => this.Ready);
                    this.stopListeningForSystemPerfCommand.RegisterCommand(stopListeningForSystemPerfDC);
                }

                return this.stopListeningForSystemPerfCommand;
            }
        }

        /// <summary>
        /// Predicate for the stop listening for system performance command
        /// </summary>
        /// <returns>Result indicates whether the command can execute</returns>
        private bool CanStopListeningForSystemPerf()
        {
            return this.Ready;
        }

        /// <summary>
        /// Performs the action of the command to stop listening to system performance updates
        /// </summary>
        /// <returns>Task the captures the continuation of the asynchronous action</returns>
        private async Task ExecuteStopListeningForSystemPerfAsync()
        {
            this.OutputDiagnosticString("ExecuteStopListeningForSystemPerfAsync\n");
            this.Ready = false;
            try
            {
                this.Portal.SystemPerfMessageReceived -= this.OnSystemPerfReceived;
                await this.StopListeningHelper();
                await this.StopListeningHelper();
            }
            catch (Exception exn)
            {
                this.ReportException("StopListeningForSystemPerf", exn);
            }

            this.Ready = true;
        }

        /// <summary>
        /// StopListeningForSystemPerf would deadlock when called from the main thread.
        /// This helper exists so that the asynchronous operation can be run on the
        /// thread pool and avoid the deadlock
        /// </summary>
        /// <returns>Task the captures the continuation of the asynchronous action</returns>
        private async Task StopListeningHelper()
        {
            await this.Portal.StopListeningForSystemPerf();
        }
        #endregion // StopListeningForSystemPerfCommand
        #endregion // Commands

        /// <summary>
        /// Event handler receives the system performance updates and sets the CPULoad property
        /// </summary>
        /// <param name="sender">The DevicePortal that originated the event</param>
        /// <param name="args">The event data</param>
        private void OnSystemPerfReceived(DevicePortal sender, WebSocketMessageReceivedEventArgs<DevicePortal.SystemPerformanceInformation> args)
        {
            this.cpuLoad = args.Message.CpuLoad;
            this.OnPropertyChanged("CPULoad");
        }
    }
}
