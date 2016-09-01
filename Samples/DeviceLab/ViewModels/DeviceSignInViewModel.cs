//----------------------------------------------------------------------------------------------
// <copyright file="DeviceSignInViewModel.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------
using System.Collections.ObjectModel;
using System.Security;
using System.Windows.Input;
using Microsoft.Tools.WindowsDevicePortal;
using Prism.Commands;
using Prism.Mvvm;

namespace DeviceLab
{
    /// <summary>
    /// ViewModel for the device sign in flow
    /// </summary>
    public class DeviceSignInViewModel : BindableBase
    {
        //-------------------------------------------------------------------
        //  Private Members
        //-------------------------------------------------------------------
        #region Private Members
        /// <summary>
        /// Destination for reporting diagnostic messages
        /// </summary>
        private IDiagnosticSink diagnostics;
        #endregion // Private Members

        //-------------------------------------------------------------------
        //  Constructors
        //-------------------------------------------------------------------
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceSignInViewModel" /> class.
        /// Default constructor creates a null diagnostic sink
        /// </summary>
        /// <remarks>Diagnostic output will be lost</remarks>
        public DeviceSignInViewModel()
        {
            this.diagnostics = new DiagnosticSinks.NullDiagnosticSink();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceSignInViewModel" /> class.
        /// Use this constructor to specify a diagnostic sink for diagnostic output
        /// </summary>
        /// <param name="diags">Diagnostic sink that will receive all the diagnostic output</param>
        public DeviceSignInViewModel(IDiagnosticSink diags)
        {
            this.diagnostics = diags;
        }
        #endregion // Constructors

        //-------------------------------------------------------------------
        //  Properties
        //-------------------------------------------------------------------
        #region Properties
        #region DeviceIP
        /// <summary>
        /// The IP address for the device with which we want to authentication
        /// </summary>
        private string deviceIP;

        /// <summary>
        ///  Gets or sets the IP address for the device with which we want to authenticate
        /// </summary>
        public string DeviceIP
        {
            get { return this.deviceIP; }
            set { this.SetProperty(ref this.deviceIP, value); }
        }
        #endregion // DeviceIP

        #region UserName
        /// <summary>
        /// Username to use when authentication with the device
        /// </summary>
        private string userName;

        /// <summary>
        /// Gets or sets the username to use when authenticating with the device
        /// </summary>
        public string UserName
        {
            get { return this.userName; }
            set { this.SetProperty(ref this.userName, value); }
        }
        #endregion // UserName

        #region Password
        /// <summary>
        /// Password for authenticating with the device
        /// </summary>
        private SecureString password;

        /// <summary>
        ///  Gets or sets the password for authenticating with the device
        /// </summary>
        public SecureString Password
        {
            get { return this.password; }
            set { this.SetProperty(ref this.password, value); }
        }
        #endregion // Password

        #region Connection Types
        /// <summary>
        /// Collection of IDevicePortalConnectionFactory objects for use when authenticating with different device types
        /// </summary>
        private ObservableCollection<IDevicePortalConnectionFactory> connectionTypes;

        /// <summary>
        /// Gets the collection of IDevicePortalConnectionFactory objects used for connecting
        /// </summary>
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
        /// <summary>
        /// Tracks the IDevicePortalConnectionFactory selected by the user
        /// </summary>
        private IDevicePortalConnectionFactory connectionTypeSelection;

        /// <summary>
        /// Gets or sets the IDevicePortalConnectionFactory selected by the user
        /// </summary>
        public IDevicePortalConnectionFactory ConnectionTypeSelection
        {
            get { return this.connectionTypeSelection; }
            set { this.SetProperty(ref this.connectionTypeSelection, value); }
        }
        #endregion // ConnectionTypeSelection
        #endregion // Properties

        //-------------------------------------------------------------------
        //  Commands
        //-------------------------------------------------------------------
        #region Commands

        #region Connect Command
        /// <summary>
        /// Command to connect to the device
        /// </summary>
        private DelegateCommand connectCommand;

        /// <summary>
        /// Gets the command for connecting with the device
        /// </summary>
        public ICommand ConnectCommand
        {
            get
            {
                if (this.connectCommand == null)
                {
                    this.connectCommand = new DelegateCommand(this.ExecuteConnect, this.CanExecuteConnect);
                    this.connectCommand.ObservesProperty(() => this.DeviceIP);
                    this.connectCommand.ObservesProperty(() => this.UserName);
                    this.connectCommand.ObservesProperty(() => this.Password);
                    this.connectCommand.ObservesProperty(() => this.ConnectionTypeSelection);
                }

                return this.connectCommand;
            }
        }

        /// <summary>
        /// Performs the action of the ConnectCommand
        /// </summary>
        private void ExecuteConnect()
        {
            IDevicePortalConnection conn = this.ConnectionTypeSelection.CreateConnection(this.DeviceIP, this.UserName, this.Password);
            DevicePortal portal = new DevicePortal(conn);
            this.SignInAttempted?.Invoke(this, new SignInAttemptEventArgs(portal, conn));
        }
        
        /// <summary>
        /// Predicate for the ConnectCommand
        /// </summary>
        /// <returns>Result indicates whether the ConnectCommand can execute</returns>
        private bool CanExecuteConnect()
        {
            return
                !string.IsNullOrWhiteSpace(this.DeviceIP) &&
                !string.IsNullOrWhiteSpace(this.UserName) &&
                this.Password != null &&
                this.Password.Length > 0 &&
                this.ConnectionTypeSelection != null;
        }
        #endregion // Connect Command
        #endregion // Commands

        #region Public Methods
        /// <summary>
        /// Add an IDevicePortalConnnectionFactory to be presented to the user for selectoin
        /// </summary>
        /// <param name="factory">The factory to be added</param>
        public void AddDevicePortalConnectionFactory(IDevicePortalConnectionFactory factory)
        {
            this.ConnectionTypes.Add(factory);
        }
        #endregion // Public Methods

        #region Events
        /// <summary>
        /// Delegate describes a method for handling SignInAttempt events
        /// </summary>
        /// <param name="sender">DeviceSignInViewModel that originated the event</param>
        /// <param name="args">Event arguments</param>
        public delegate void SignInAttemptEventHandler(DeviceSignInViewModel sender, SignInAttemptEventArgs args);

        /// <summary>
        /// The arguments for a DeviceSignInAttempt event
        /// </summary>
        public class SignInAttemptEventArgs : System.EventArgs
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SignInAttemptEventArgs" /> class.
            /// </summary>
            /// <param name="portal">The DevicePortal associated with the sign in attempt</param>
            /// <param name="conn">The IDevicePortalConnection instance associated with the sign in attempt</param>
            internal SignInAttemptEventArgs(DevicePortal portal, IDevicePortalConnection conn)
            {
                this.Portal = portal;
                this.Connection = conn;
            }

            /// <summary>
            /// Gets the DevicePortal associated with the sign in attempt
            /// </summary>
            public DevicePortal Portal { get; private set; }

            /// <summary>
            ///  Gets the IDevicePortalConnection instance associated with the sign in attempt
            /// </summary>
            public IDevicePortalConnection Connection { get; private set; }
        }

        /// <summary>
        /// Event fires whenever the user tries to connect to another device.
        /// </summary>
        public event SignInAttemptEventHandler SignInAttempted;
        #endregion // Events
    }

    /// <summary>
    /// Interface describes a factory class for creating IDevicePortalConnection instances
    /// </summary>
    public interface IDevicePortalConnectionFactory
    {
        /// <summary>
        /// Create an instance of IDevicePortalConnection
        /// </summary>
        /// <param name="address">IP address to use for connectin</param>
        /// <param name="userName">User name to use for authenticating</param>
        /// <param name="password">Password to use for authenticating</param>
        /// <returns>The new IDevicePortalConnection instance</returns>
        IDevicePortalConnection CreateConnection(
            string address,
            string userName,
            SecureString password);

        /// <summary>
        /// Gets a friendly name for the IDevicePortalConnectionFactory that can be presented to the user
        /// </summary>
        string Name { get; }
    }
}
