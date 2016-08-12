using Microsoft.Tools.WindowsDevicePortal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace DeviceLab
{

    public interface IDevicePortalConnectionFactory
    {
        IDevicePortalConnection CreateConnection(
            string address,
            string userName,
            SecureString password);

        string Name { get; }
    }

    class XboxDevicePortalConnectionFactory : IDevicePortalConnectionFactory
    {
        public string Name { get { return "Xbox One"; } }

        public IDevicePortalConnection CreateConnection(
            string address,
            string userName,
            SecureString password)
        {
            return new XboxDevicePortalConnection(address, userName, password);
        }
    }

    class GenericDevicePortalConnectionFactory : IDevicePortalConnectionFactory
    {
        public string Name { get { return "Generic Device"; } }

        public IDevicePortalConnection CreateConnection(string address, string userName, SecureString password)
        {
            return new DefaultDevicePortalConnection(address, userName, password);
        }
    }

    public delegate void DeviceSignInEventHandler(DeviceSignInViewModel sender, DeviceSignInEventArgs args);

    public class DeviceSignInEventArgs : System.EventArgs
    {
        internal DeviceSignInEventArgs(HttpStatusCode statusCode, DevicePortal portal)
        {
            this.StatusCode = statusCode;
            this.Portal = portal;
        }

        public HttpStatusCode StatusCode { get; private set; }

        public DevicePortal Portal { get; private set; }
    }

    public class DeviceSignInViewModel : BindableBase
    {
        //-------------------------------------------------------------------
        //  Private Members
        //-------------------------------------------------------------------
        #region Private Members
        private IDiagnosticSink diagnostics;
        #endregion // Private Members

        //-------------------------------------------------------------------
        //  Constructors
        //-------------------------------------------------------------------
        #region Constructors
        /// <summary>
        ///  Default constructor creates a null diagnostic sink
        /// </summary>
        /// <remarks>Diagnostic output will be lost</remarks>
        public DeviceSignInViewModel()
        {
            this.ready = true;
            this.diagnostics = new NullDiagnosticSink();

        }

        /// <summary>
        /// Use this constructor to specify a diagnostic sink for diagnostic output
        /// </summary>
        /// <param name="diags">Diagnostic sink that will receive all the diagnostic output</param>
        public DeviceSignInViewModel(IDiagnosticSink diags)
        {
            this.ready = true;
            this.diagnostics = diags;
        }
        #endregion // Constructors

        //-------------------------------------------------------------------
        //  Properties
        //-------------------------------------------------------------------
        #region Properties
        #region DeviceIP
        private string deviceIP;
        public string DeviceIP
        {
            get { return this.deviceIP; }
            set { SetProperty(ref this.deviceIP, value); }
        }
        #endregion // DeviceIP

        #region UserName
        private string userName;
        public string UserName
        {
            get { return this.userName; }
            set { SetProperty(ref this.userName, value); }
        }
        #endregion // UserName

        #region Password
        private SecureString password;
        public SecureString Password
        {
            get { return this.password; }
            set { SetProperty(ref this.password, value); }
        }
        #endregion // Password

        #region Ready
        private bool ready;
        public bool Ready
        {
            get { return this.ready; }
            private set { SetProperty(ref this.ready, value); }
        }
        #endregion // Ready

        #region Connection Types
        private ObservableCollection<IDevicePortalConnectionFactory> connectionTypes;
        public ObservableCollection<IDevicePortalConnectionFactory> ConnectionTypes
        {
            get
            {
                if (this.connectionTypes == null)
                {
                    this.connectionTypes = new ObservableCollection<IDevicePortalConnectionFactory>();
                }
                return this.connectionTypes;
            }
        }
        #endregion // Connection Types

        #region ConnectionTypeSelection
        private IDevicePortalConnectionFactory connectionTypeSelection;
        public IDevicePortalConnectionFactory ConnectionTypeSelection
        {
            get { return this.connectionTypeSelection; }
            set { SetProperty(ref this.connectionTypeSelection, value); }
        }
        #endregion // ConnectionTypeSelection
        #endregion // Properties

        //-------------------------------------------------------------------
        //  Commands
        //-------------------------------------------------------------------
        #region Commands

        #region Connect Command
        private DelegateCommand connectCommand;
        public ICommand ConnectCommand
        {
            get
            {
                if (connectCommand == null)
                {
                    connectCommand = DelegateCommand.FromAsyncHandler(ExecuteConnectAsync, CanExecuteConnect);
                    connectCommand.ObservesProperty(() => this.DeviceIP);
                    connectCommand.ObservesProperty(() => this.UserName);
                    connectCommand.ObservesProperty(() => this.Password);
                    connectCommand.ObservesProperty(() => this.Ready);
                    connectCommand.ObservesProperty(() => this.ConnectionTypeSelection);
                }
                return connectCommand;
            }
        }

        private async Task ExecuteConnectAsync()
        {
            this.diagnostics.OutputDiagnosticString("[{0}] Attempting to connect.\n", this.deviceIP);

            IDevicePortalConnection conn = this.ConnectionTypeSelection.CreateConnection(this.DeviceIP, this.UserName, this.Password);

            DevicePortal portal = new DevicePortal(conn);
            DeviceConnectionStatusEventHandler handler = (DevicePortal sender, DeviceConnectionStatusEventArgs args) =>
            {
                this.diagnostics.OutputDiagnosticString("[{0}] Connection status update: Status: {1}, Phase: {2}\n", portal.Address, args.Status, args.Phase);
                if (args.Status == DeviceConnectionStatus.Connected)
                {
                    this.diagnostics.OutputDiagnosticString("[{0}] Language: {1}\n", portal.Address, conn.OsInfo.Language);
                    this.diagnostics.OutputDiagnosticString("[{0}] Name: {1}\n", portal.Address, conn.OsInfo.Name);
                    this.diagnostics.OutputDiagnosticString("[{0}] OsEdition: {1}\n", portal.Address, conn.OsInfo.OsEdition);
                    this.diagnostics.OutputDiagnosticString("[{0}] OsEditionId: {1}\n", portal.Address, conn.OsInfo.OsEditionId);
                    this.diagnostics.OutputDiagnosticString("[{0}] OsVersionString: {1}\n", portal.Address, conn.OsInfo.OsVersionString);
                    this.diagnostics.OutputDiagnosticString("[{0}] Platform: {1}\n", portal.Address, conn.OsInfo.Platform);
                    this.diagnostics.OutputDiagnosticString("[{0}] PlatformName: {1}\n", portal.Address, conn.OsInfo.PlatformName);
                }
                else if (args.Status == DeviceConnectionStatus.Failed)
                {
                    this.diagnostics.OutputDiagnosticString("[{0}] Bummer.\n", portal.Address);
                }
            };
            portal.ConnectionStatus += handler;

            try
            {
                this.Ready = false;
                await portal.Connect();
            }
            catch (Exception exn)
            {
                this.diagnostics.OutputDiagnosticString("[{0}] Exception when trying to connect:\n[{0}] {1}\nStackTrace: \n[{0}] {2}\n", portal.Address, exn.Message, exn.StackTrace);
            }
            this.SignInAttempts?.Invoke(this, new DeviceSignInEventArgs(portal.ConnectionHttpStatusCode, portal));
            portal.ConnectionStatus -= handler;
            this.Ready = true;
        }

        private bool CanExecuteConnect()
        {
            return
                Ready &&
                !string.IsNullOrWhiteSpace(this.DeviceIP) &&
                !string.IsNullOrWhiteSpace(this.UserName) &&
                this.Password != null &&
                this.Password.Length > 0 &&
                this.ConnectionTypeSelection != null;
        }
        #endregion // Connect Command
        #endregion // Commands

        #region Public Methods
        public void AddDevicePortalConnectionFactory(IDevicePortalConnectionFactory factory)
        {
            this.ConnectionTypes.Add(factory);
        }
        #endregion // Public Methods

        #region Events
        public event DeviceSignInEventHandler SignInAttempts;
        #endregion // Events

    }
}
