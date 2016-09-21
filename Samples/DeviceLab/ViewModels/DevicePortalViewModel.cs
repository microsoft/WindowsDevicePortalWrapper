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
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using System.Net.Security;
using static Microsoft.Tools.WindowsDevicePortal.DevicePortal;

namespace DeviceLab
{
    /// <summary>
    /// Viewmodel for a single connected device
    /// </summary>
    public class DevicePortalViewModel : DevicePortalCommandModel
    {
        MainViewModel mainViewModel;

        //-------------------------------------------------------------------
        //  Constructors
        //-------------------------------------------------------------------
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="DevicePortalViewModel" /> class.
        /// </summary>
        /// <param name="portal">DevicePortal object enscapsulated by this</param>
        /// <param name="diags">Diagnostic sink for reporting</param>
        public DevicePortalViewModel(MainViewModel mainViewModel, IDevicePortalConnection connection, IDiagnosticSink diags)
            : base(connection, diags)
        {
            this.mainViewModel = mainViewModel;

            // Add additional handling for untrusted certs.
            this.Portal.UnvalidatedCert += DoCertValidation;

            // Default number of retry attempts to make when reestablishing the connection
            this.ConnectionRetryAttempts = 3;
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
                if(string.IsNullOrWhiteSpace(this.deviceName))
                {
                    return !string.IsNullOrWhiteSpace(this.CannonicalIpAddress) ? this.CannonicalIpAddress : base.DiagnosticMoniker;
                }
                return this.deviceName;
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

        #region CannonicalIpAddress
        private string cannonicalIpAddress;
        public string CannonicalIpAddress
        {
            get
            {
                return cannonicalIpAddress;
            }

            private set
            {
                this.SetProperty(ref this.cannonicalIpAddress, value);
            }
        }
        #endregion // CannonicalIpAddress

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
        #region GetCannonicalIpAddress
        private CommandSequence getCannonicalIpAddressCommand;
        public ICommand GetCannonicalIpAddressCommand
        {
            get
            {
                if(this.getCannonicalIpAddressCommand == null)
                {
                    this.getCannonicalIpAddressCommand = this.CreateCommandSequence();
                    DelegateCommand getCannonicalIpAddressDC = DelegateCommand.FromAsyncHandler(this.ExecuteGetCannonicalIpAddressAsync, this.CanExecuteGetCannonicalIpAddress);
                    getCannonicalIpAddressDC.ObservesProperty(() => this.Ready);
                    this.getCannonicalIpAddressCommand.RegisterCommand(getCannonicalIpAddressDC);
                }
                return this.getCannonicalIpAddressCommand;
            }
        }

        private bool CanExecuteGetCannonicalIpAddress()
        {
            return this.Ready;
        }

        private async Task ExecuteGetCannonicalIpAddressAsync()
        {
            this.OutputDiagnosticString("ExecuteGetCannonicalIpAddress\n");
            this.Ready = false;
            try
            {
                IpConfiguration config = await this.Portal.GetIpConfig();
                this.CannonicalIpAddressFromIpConfig(config);
                this.CheckForDuplicateDevices();
            }
            catch (Exception exn)
            {
                this.ReportException("GetCannonicalIpAddress", exn);
            }
            this.Ready = true;
        }

        private void CheckForDuplicateDevices()
        {
            foreach(DevicePortalViewModel dpvm in this.mainViewModel.ConnectedDevices)
            {
                if (dpvm == this)
                    continue;

                if(this.cannonicalIpAddress == dpvm.cannonicalIpAddress)
                {
                    MessageBox.Show(string.Format("You already have a connection for this device address: {0}", this.cannonicalIpAddress),
                            "Duplicate",
                            MessageBoxButton.OK,
                            MessageBoxImage.Exclamation);
                    this.ClearCommandQueue();
                    this.mainViewModel.RemoveDeviceCommand.Execute(this);
                }
            }
        }

        private void CannonicalIpAddressFromIpConfig(IpConfiguration config)
        {
            string firstNonZeroIpAddress = "";
            string connectionIp = this.StripPort(this.Connection.Connection.Authority);
            bool ipMatchesConnection = false;
            foreach(NetworkAdapterInfo naio in config.Adapters)
            {
                foreach(IpAddressInfo iai in naio.IpAddresses)
                {
                    string addr = iai.Address;
                    if (string.IsNullOrWhiteSpace(addr))
                        continue;
                    if (addr == "0.0.0.0")
                        continue;
                    firstNonZeroIpAddress = addr;
                    if(addr == connectionIp)
                    {
                        ipMatchesConnection = true;
                        break;
                    }
                }
                
                if(ipMatchesConnection)
                {
                    this.CannonicalIpAddress = connectionIp;
                }
                else
                {
                    this.CannonicalIpAddress = firstNonZeroIpAddress;
                }
            }
        }

        private string StripPort(string authority)
        {
            return authority.Split(':')[0];
        }
        #endregion // GetCannonicalIpAddress

        #region DumpIpConfigCommand
        private CommandSequence dumpIpConfigCommand;
        public ICommand DumpIpConfigCommand
        {
            get
            {
                if(this.dumpIpConfigCommand == null)
                {
                    this.dumpIpConfigCommand = this.CreateCommandSequence();
                    DelegateCommand dumpIpConfigDC = DelegateCommand.FromAsyncHandler(this.ExecuteDumpIpConfigAsync, this.CanExecuteDumpIpConfig);
                    dumpIpConfigDC.ObservesProperty(() => this.Ready);
                    this.dumpIpConfigCommand.RegisterCommand(dumpIpConfigDC);
                }
                return this.dumpIpConfigCommand;
            }
        }

        private bool CanExecuteDumpIpConfig()
        {
            return this.Ready;
        }

        private async Task ExecuteDumpIpConfigAsync()
        {
            this.OutputDiagnosticString("ExecuteDumpIpConfigAsync\n");
            this.Ready = false;
            try
            {
                IpConfiguration config = await this.Portal.GetIpConfig();
                this.OutputIpConfiguration(config);
            }
            catch (Exception exn)
            {
                this.ReportException("DumpIPConfig", exn);
            }
            this.Ready = true;
        }

        private void OutputIpConfiguration(IpConfiguration config)
        {
            // For now, just dump out the adapters to the debug output to see what I got
            this.OutputDiagnosticString("Dumping network adapter information:\n");

            foreach (NetworkAdapterInfo nai in config.Adapters)
            {
                this.OutputDiagnosticString("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\n");
                this.OutputDiagnosticString("    Description: {0}\n", nai.Description);
                this.OutputDiagnosticString("    Mac Address: {0}\n", nai.MacAddress);
                this.OutputDiagnosticString("    Index: {0}\n", nai.Index);
                this.OutputDiagnosticString("    Id: {0}\n", nai.Id);
                this.OutputDiagnosticString("    Adapter Type: {0}\n", nai.AdapterType);
                this.OutputDiagnosticString("    DHCP:\n");
                this.OutputDHCPInfo(nai.Dhcp);
                this.OutputDiagnosticString("    Gateways:\n");
                foreach (IpAddressInfo iai in nai.Gateways)
                {
                    this.OutputIpAddressInfo(iai);
                }
                this.OutputDiagnosticString("    IP Addresses:\n");
                foreach (IpAddressInfo iai in nai.IpAddresses)
                {
                    this.OutputIpAddressInfo(iai);
                }
            }
        }

        private void OutputDHCPInfo(Dhcp dhcp)
        {
            this.OutputIpAddressInfo(dhcp.Address);
            this.OutputDiagnosticString("        Lease Obtained {0}\n", dhcp.LeaseObtained.ToLocalTime().ToString());
            this.OutputDiagnosticString("        Lease Expires {0}\n", dhcp.LeaseExpires.ToLocalTime().ToString());
        }

        private void OutputIpAddressInfo(IpAddressInfo ipAddr)
        {
            this.OutputDiagnosticString("        Address: {0}\n", ipAddr.Address);
            this.OutputDiagnosticString("        Subnet Mask: {0}\n", ipAddr.SubnetMask);            
        }
        #endregion // DumpIpConfigCommand

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
                    

                    DelegateCommand resetConnectionAndPortalDC = new DelegateCommand(this.ExecuteResetConnectionAndPortal, this.CanExecuteResetConnectionAndPortal);
                    resetConnectionAndPortalDC.ObservesProperty(() => this.Ready);

                    this.renameCommand.RegisterCommand(renameDC);
                    this.renameCommand.RegisterCommand(resetConnectionAndPortalDC);
                    this.renameCommand.RegisterCommand(this.ReestablishConnectionCommand);
                    this.renameCommand.RegisterCommand(this.RebootCommand);
                }

                return this.renameCommand;
            }
        }

        private bool CanExecuteResetConnectionAndPortal()
        {
            return this.Ready;
        }

        private void ExecuteResetConnectionAndPortal()
        {
            this.OutputDiagnosticString("ExecuteResetConnectionAndPortalAsync\n");
            this.Ready = false;
            try
            {
                NetworkCredential cred = this.Connection.Credentials;
                string scheme = this.Connection.Connection.Scheme;
                string address = this.CannonicalIpAddress;
                string port = this.Connection.Connection.Port.ToString();
                string schemeAddressPort = string.Format(@"{0}://{1}:{2}", scheme, address, port);
                string username = this.StripAutoPrefix(cred.UserName);

                this.Portal.UnvalidatedCert -= DoCertValidation;

                this.Connection = new DefaultDevicePortalConnection(schemeAddressPort, username, cred.SecurePassword);
                this.Portal = new DevicePortal(this.Connection);

                // Add additional handling for untrusted certs.
                this.Portal.UnvalidatedCert += DoCertValidation;
            }
            catch (Exception exn)
            {
                this.ReportException("ResetConnectionAndPortal", exn);
            }

            this.Ready = true;
        }

        private string StripAutoPrefix(string userName)
        {
            if(userName.Length > 5)
            {
                return userName.Substring(5);
            }
            return userName;
        }

        /// <summary>
        /// Predicate for the rename command
        /// </summary>
        /// <returns>Result indicates whether the rename command can execute</returns>
        private bool CanExecuteRename()
        {
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
                this.OutputDiagnosticString("Done refreshing device name\n");
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
                    this.rebootCommand.RegisterCommand(this.RefreshDeviceNameCommand);
                    this.renameCommand.RegisterCommand(this.GetCannonicalIpAddressCommand);
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

        #region RefreshConnectionCommand
        private CommandSequence refreshConnectionCommand;
        public ICommand RefreshConnectionCommand
        {
            get
            {
                if(this.refreshConnectionCommand == null)
                {
                    this.refreshConnectionCommand = this.CreateCommandSequence();
                    this.refreshConnectionCommand.RegisterCommand(this.StopListeningForSystemPerfCommand);
                    this.refreshConnectionCommand.RegisterCommand(this.ReestablishConnectionCommand);
                    this.refreshConnectionCommand.RegisterCommand(this.RefreshDeviceNameCommand);
                    this.refreshConnectionCommand.RegisterCommand(this.GetCannonicalIpAddressCommand);
                    this.refreshConnectionCommand.RegisterCommand(this.StartListeningForSystemPerfCommand);
                }
                return this.refreshConnectionCommand;
            }
        }
        #endregion // RefreshConnectionCommand

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
                    this.OutputDiagnosticString("HTTP Status: {0}\n", this.Portal.ConnectionHttpStatusCode);
                    this.OutputDiagnosticString("Failure description: {0}\n", args.Message);
                }
            };

            this.Portal.ConnectionStatus += handler;

            this.Ready = false;
            try
            {

                do
                {
                    await this.Portal.Connect();

                    if(this.Portal.ConnectionHttpStatusCode == HttpStatusCode.Unauthorized)
                    {
                        MessageBox.Show("Connection Unauthorized. Please double check your authentication credentials",
                            "Unauthorized",
                            MessageBoxButton.OK,
                            MessageBoxImage.Exclamation);
                        this.ClearCommandQueue();
                        this.mainViewModel.RemoveDeviceCommand.Execute(this);
                        break;
                    }
                    else if(this.Portal.ConnectionHttpStatusCode != HttpStatusCode.OK && numTries <= this.ConnectionRetryAttempts)
                    {
                        await Task.Delay(1000 * 5);
                    }
                    
                    ++numTries;
                }
                while (this.Portal.ConnectionHttpStatusCode != HttpStatusCode.OK && numTries <= this.ConnectionRetryAttempts);

                if (this.Portal.ConnectionHttpStatusCode != HttpStatusCode.OK)
                {
                    throw new Exception(string.Format("Unable to connect after {0} tries.", numTries - 1));
                }

                OnPropertyChanged("Address");
                OnPropertyChanged("DeviceFamily");
                OnPropertyChanged("OperatingSystemVersion");
                OnPropertyChanged("Platform");
                OnPropertyChanged("PlatformName");
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
                await this.Portal.StartListeningForSystemPerf();
            }
            catch (Exception exn)
            {
                this.ReportException("StartListeningForSystemPerf", exn);
            }

            this.Ready = true;
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
                await this.Portal.StopListeningForSystemPerf();
            }
            catch (Exception exn)
            {
                this.ReportException("StopListeningForSystemPerf", exn);
            }

            this.Ready = true;
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

        /// <summary>
        /// An SSL thumbprint that we'll accept.
        /// </summary>
        private string thumbprint;

        /// <summary>
        /// Validate the server certificate
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="certificate">The server's certificate</param>
        /// <param name="chain">The cert chain</param>
        /// <param name="sslPolicyErrors">Policy Errors</param>
        /// <returns>whether the cert passes validation</returns>
        private bool DoCertValidation(DevicePortal sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            X509Certificate2 cert = new X509Certificate2(certificate);

            // If we have previously said to accept this cert, don't prompt again for this session.
            if (!string.IsNullOrEmpty(this.thumbprint) && this.thumbprint.Equals(cert.Thumbprint, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // We could alternatively ask the user if they wanted to always trust
            // this device and we could persist the thumbprint in some way (registry, database, filesystem, etc).
            MessageBoxResult result = MessageBox.Show(string.Format(
                                "Do you want to accept the following certificate?\n\nThumbprint:\n  {0}\nIssuer:\n  {1}",
                                cert.Thumbprint,
                                cert.Issuer),
                            "Untrusted Certificate Detected",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question,
                            MessageBoxResult.No);

            if (result == MessageBoxResult.Yes)
            {
                this.thumbprint = cert.Thumbprint;
                return true;
            }
            return false;
        }
    }
}
