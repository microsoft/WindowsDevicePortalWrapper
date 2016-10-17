//----------------------------------------------------------------------------------------------
// <copyright file="DeviceSignInViewModel.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Security;
using System.Windows.Input;
using Microsoft.Tools.WindowsDevicePortal;
using Prism.Commands;
using Prism.Mvvm;

namespace SampleDeviceCollection
{
    /// <summary>
    /// Enumerates the device families that may be selected by the user
    /// </summary>
    public enum DeviceFamilySelections
    {
        XboxOne,
        HoloLens,
        // TODO: Phone is not yet supported
        // Phone,
        IoT,
        Desktop,
        Other
    }

    /// <summary>
    /// ViewModel for the device sign in flow
    /// </summary>
    public class DeviceSignInViewModel : BindableBase
    {
        //-------------------------------------------------------------------
        //  Private Members
        //-------------------------------------------------------------------
        #region Private Members
        #endregion // Private Members

        //-------------------------------------------------------------------
        //  Constructors
        //-------------------------------------------------------------------
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceSignInViewModel" /> class.
        /// </summary>
        public DeviceSignInViewModel()
        {
            this.DeviceFamily = DeviceFamilySelections.XboxOne;
            this.ProtocolIsHttps = true;
        }
        #endregion // Constructors

        //-------------------------------------------------------------------
        //  Properties
        //-------------------------------------------------------------------
        #region Properties
        #region DeviceFamily
        /// <summary>
        /// The device family selected by the user
        /// </summary>
        private DeviceFamilySelections deviceFamily;

        /// <summary>
        /// Gets or sets the device family selected by the user
        /// </summary>
        public DeviceFamilySelections DeviceFamily
        {
            get
            {
                return this.deviceFamily;
            }

            set
            {
                this.SetProperty(ref this.deviceFamily, value);

                // Prepopulate some "best guess" values for Desktop
                if (this.DeviceFamily == DeviceFamilySelections.Desktop)
                {
                    this.PrepopulateDesktopPort();
                }

                // All bets are off for "Other" so clear everything and leave it up to the user
                if (this.DeviceFamily == DeviceFamilySelections.Other)
                {
                    this.portUserEntry = string.Empty;
                }
                
                this.OnPropertyChanged("UsbAvailable");
                this.OnPropertyChanged("UsbSelected");
                this.OnPropertyChanged("ProtocolSelectionEnabled");
                this.OnPropertyChanged("Port");
                this.OnPropertyChanged("IsPortEntryEnabled");
                this.OnPropertyChanged("AddressEntryEnabled");
                this.OnPropertyChanged("Address");
                if (this.DeviceFamily == DeviceFamilySelections.XboxOne)
                {
                    this.ProtocolIsHttps = true;
                }
            }
        }
        #endregion // DeviceFamily

        #region ProtocolIsHttps
        /// <summary>
        /// Bidning for a radio selection to disambiguate the URL protocol scheme for the user. Protocol can be either http or https.
        /// </summary>
        private bool protocolIsHttps;

        /// <summary>
        /// Gets or sets a value indicating whether the URL protocol scheme is https or http
        /// </summary>
        public bool ProtocolIsHttps
        {
            get
            {
                return this.protocolIsHttps;
            }

            set
            {
                this.SetProperty(ref this.protocolIsHttps, value);
                if (this.DeviceFamily == DeviceFamilySelections.Desktop)
                {
                    this.PrepopulateDesktopPort();
                }

                this.OnPropertyChanged("Port");
                this.OnPropertyChanged("IsPortEntryEnabled");
                this.OnPropertyChanged("Address");
            }
        }
        #endregion // ProtocolIsHttps
                
        #region ProtocolSelectionEnabled
        /// <summary>
        /// Gets a value indicating whether the user may select the URL protocol scheme
        /// </summary>
        public bool ProtocolSelectionEnabled
        {
            get
            {
                return this.DeviceFamily != DeviceFamilySelections.XboxOne;
            }
        }
        #endregion // ProtocolSelectionEnabled

        #region UsbAvailable
        /// <summary>
        /// Gets a value indicating whether USB selection is available
        /// </summary>
        public bool UsbAvailable
        {
            get
            {
                return this.DeviceFamily == DeviceFamilySelections.HoloLens;
                    // TODO: Phone is not yet supported
                    // || this.DeviceFamily == DeviceFamilySelections.Phone
            }
        }
        #endregion // UsbAvailable

        #region UsbSelected
        /// <summary>
        /// Binding property for a checkbox that indictates whether the user is attemptint to connect over USB
        /// </summary>
        private bool usbSelected;

        /// <summary>
        /// Gets or sets a value indicating whether the user is attempting to connect over USB
        /// </summary>
        public bool UsbSelected
        {
            get
            {
                return this.UsbAvailable && this.usbSelected;
            }

            set
            {
                this.SetProperty(ref this.usbSelected, value);
                this.OnPropertyChanged("Port");
                this.OnPropertyChanged("IsPortEntryEnabled");
                this.OnPropertyChanged("Address");
                this.OnPropertyChanged("AddressEntryEnabled");
            }
        }
        #endregion // UsbSelected

        #region IsPortEntryEnabled
        /// <summary>
        /// Gets a value indicating whether the user may enter the port value
        /// </summary>
        public bool IsPortEntryEnabled
        {
            get
            {
                switch (this.DeviceFamily)
                {
                    case DeviceFamilySelections.HoloLens:
                    // TODO: Phone is not yet supported
                    // case DeviceFamilySelections.Phone:
                    case DeviceFamilySelections.XboxOne:
                        return false;

                    case DeviceFamilySelections.IoT:
                        return this.ProtocolIsHttps;

                    case DeviceFamilySelections.Other:
                    default:
                        return true;
                }
            }
        }
        #endregion // IsPortEntryEnabled

        #region Port
        /// <summary>
        /// Binding property for a field where the user may enter a port value
        /// </summary>
        private string portUserEntry;

        /// <summary>
        /// Gets or sets the port value to use when connecting to the device.
        /// </summary>
        public string Port
        {
            get
            {
                string portForFamily = this.GetPortForDeviceFamily();
                if (string.IsNullOrEmpty(portForFamily))
                {
                    return this.portUserEntry;
                }
                else
                {
                    return portForFamily;
                }
            }

            set
            {
                this.SetProperty(ref this.portUserEntry, value);
            }
        }
        #endregion // Port

        #region GetPortForDeviceFamily
        /// <summary>
        /// Gets the port number to use based on the device family, protocol, and USB selections
        /// </summary>
        /// <returns>the port number as a string</returns>
        private string GetPortForDeviceFamily()
        {
            switch (this.DeviceFamily)
            {
                case DeviceFamilySelections.HoloLens:
                //case DeviceFamilySelections.Phone:
                    {
                        if (this.UsbSelected)
                        {
                            return "10080";
                        }

                        return this.ProtocolIsHttps ? "443" : "80";
                    }

                case DeviceFamilySelections.IoT:
                    return this.ProtocolIsHttps ? string.Empty : "8080";
                case DeviceFamilySelections.XboxOne:
                    return "11443";
                case DeviceFamilySelections.Other:
                default:
                    return string.Empty;
            }
        }
        #endregion // GetPortForDeviceFamily

        #region Address
        /// <summary>
        /// Binding property for a field where the user enters the device address
        /// </summary>
        private string addressUserEntry;

        /// <summary>
        /// Gets or sets a value containing the device address
        /// </summary>
        public string Address
        {
            get
            {
                if (this.UsbAvailable && this.UsbSelected)
                {
                    return "localhost";
                }

                return this.addressUserEntry;
            }

            set
            {
                this.SetProperty(ref this.addressUserEntry, value);
            }
        }
        #endregion // Address

        #region AddressEntryEnabled
        /// <summary>
        /// Gets a value indicating whether the user may enter an address for the device.
        /// (In case it is a USB connection then the address is always localhost.)
        /// </summary>
        public bool AddressEntryEnabled
        {
            get
            {
                return !this.UsbAvailable || !this.UsbSelected;
            }
        }
        #endregion // AddressEntryEnabled

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

        /// <summary>
        /// Prepopulate the port entry with a best guess value whenever the user selects Desktop
        /// or when Desktop is already selected and the user changes the protocol
        /// </summary>
        private void PrepopulateDesktopPort()
        {
            if(this.ProtocolIsHttps)
            {
                // Some logic to prevent clobbering the user's entry
                if(string.IsNullOrWhiteSpace(this.portUserEntry) || this.portUserEntry == "50080")
                {
                    this.portUserEntry = "50443";
                }
            }
            else
            {
                // Some logic to prevent clobbering the user's entry
                if (string.IsNullOrWhiteSpace(this.portUserEntry) || this.portUserEntry == "50443")
                {
                    this.portUserEntry = "50080";
                }
            }
        }
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
                    this.connectCommand.ObservesProperty(() => this.Address);
                    this.connectCommand.ObservesProperty(() => this.Port);
                    this.connectCommand.ObservesProperty(() => this.UserName);
                    this.connectCommand.ObservesProperty(() => this.Password);
                }

                return this.connectCommand;
            }
        }

        /// <summary>
        /// Performs the action of the ConnectCommand
        /// </summary>
        private void ExecuteConnect()
        {
            string protocolAddressPort = string.Format(
                @"{0}://{1}:{2}",
                this.ProtocolIsHttps ? @"https" : @"http",
                this.Address,
                this.Port);

            IDevicePortalConnection conn = new DefaultDevicePortalConnection(protocolAddressPort, this.UserName, this.Password);
            this.SignInAttempted?.Invoke(this, new SignInAttemptEventArgs(conn));
        }
        
        /// <summary>
        /// Predicate for the ConnectCommand
        /// </summary>
        /// <returns>Result indicates whether the ConnectCommand can execute</returns>
        private bool CanExecuteConnect()
        {
            return
                !string.IsNullOrWhiteSpace(this.Address) &&
                !string.IsNullOrWhiteSpace(this.Port) &&
                !string.IsNullOrWhiteSpace(this.UserName) &&
                this.Password != null &&
                this.Password.Length > 0;
        }
        #endregion // Connect Command
        #endregion // Commands
        
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
            /// <param name="conn">The IDevicePortalConnection instance associated with the sign in attempt</param>
            internal SignInAttemptEventArgs(IDevicePortalConnection conn)
            {
                this.Connection = conn;
            }
                        
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
}
