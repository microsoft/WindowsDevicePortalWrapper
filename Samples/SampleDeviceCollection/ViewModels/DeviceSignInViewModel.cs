//----------------------------------------------------------------------------------------------
// <copyright file="DeviceSignInViewModel.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Windows.Data;
using System.Windows;

using System.Security;
using Microsoft.Tools.WindowsDevicePortal;
using Prism.Commands;
using Prism.Mvvm;
using System.Windows.Input;

namespace SampleDeviceCollection
{
    //-------------------------------------------------------------------
    //  Value Converters
    //-------------------------------------------------------------------
    #region Value Converters
    /// <summary>
    ///     Template allows for easy creation of a value converter for bools
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    ///     See BooleanToVisibilityConverter and BooleanToBrushConverter (below) and usage in Generic.xaml
    /// </remarks>
    public class BooleanConverter<T> : IValueConverter
    {
        public BooleanConverter(T trueValue, T falseValue)
        {
            True = trueValue;
            False = falseValue;
        }

        public T True { get; set; }
        public T False { get; set; }

        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool && ((bool)value) ? True : False;
        }

        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is T && EqualityComparer<T>.Default.Equals((T)value, True);
        }
    }

    /// <summary>
    /// Converts a boolean to a Visibility value
    /// </summary>
    public sealed class BooleanToVisibilityConverter : BooleanConverter<Visibility>
    {
        public BooleanToVisibilityConverter() :
            base(Visibility.Visible, Visibility.Hidden)
        { }
    }

    public sealed class BooleanToHttpsConverter : BooleanConverter<string>
    {
        public BooleanToHttpsConverter() :
            base("https", "http")
        { }
    }
    #endregion // Value Converters

    public enum DeviceFamilySelections
    {
        XboxOne,
        HoloLens,
        Phone,
        IoT,
        Desktop
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
        private DeviceFamilySelections deviceFamily;
        public DeviceFamilySelections DeviceFamily
        {
            get
            {
                return this.deviceFamily;
            }

            set
            {
                this.SetProperty(ref this.deviceFamily, value);
                OnPropertyChanged("UsbAvailable");
                OnPropertyChanged("UsbSelected");
                OnPropertyChanged("ProtocolSelectionEnabled");
                OnPropertyChanged("Port");
                OnPropertyChanged("IsPortEntryEnabled");
                OnPropertyChanged("AddressEntryEnabled");
                OnPropertyChanged("Address");
                if (this.DeviceFamily == DeviceFamilySelections.XboxOne)
                {
                    this.ProtocolIsHttps = true;
                }
            }
        }
        #endregion // DeviceFamily

        #region ProtocolIsHttps
        private bool protocolIsHttps;
        public bool ProtocolIsHttps
        {
            get
            {
                return this.protocolIsHttps;
            }

            set
            {
                this.SetProperty(ref this.protocolIsHttps, value);
                OnPropertyChanged("Port");
                OnPropertyChanged("IsPortEntryEnabled");
                OnPropertyChanged("Address");
            }
        }
        #endregion // ProtocolIsHttps

        #region ProtocolSelectionEnabled
        public bool ProtocolSelectionEnabled
        {
            get
            {
                return this.DeviceFamily != DeviceFamilySelections.XboxOne;
            }
        }
        #endregion // ProtocolSelectionEnabled

        #region UsbAvailable
        public bool UsbAvailable
        {
            get
            {
                return this.DeviceFamily == DeviceFamilySelections.HoloLens
                    || this.DeviceFamily == DeviceFamilySelections.Phone;
            }
        }
        #endregion // UsbAvailable

        #region UsbSelected
        private bool usbSelected;
        public bool UsbSelected
        {
            get
            {
                return this.UsbAvailable && this.usbSelected;
            }

            set
            {
                this.SetProperty(ref this.usbSelected, value);
                OnPropertyChanged("Port");
                OnPropertyChanged("IsPortEntryEnabled");
                OnPropertyChanged("Address");
                OnPropertyChanged("AddressEntryEnabled");
            }
        }
        #endregion // UsbSelected

        #region IsPortEntryEnabled
        public bool IsPortEntryEnabled
        {
            get
            {
                switch (this.DeviceFamily)
                {
                    case DeviceFamilySelections.HoloLens:
                    case DeviceFamilySelections.Phone:
                    case DeviceFamilySelections.XboxOne:
                        return false;

                    case DeviceFamilySelections.IoT:
                        return this.ProtocolIsHttps;

                    case DeviceFamilySelections.Desktop:
                    default:
                        return true;
                }
            }
        }
        #endregion // IsPortEntryEnabled

        #region Port
        private string portUserEntry;
        public string Port
        {
            get
            {
                string portForFamily = GetPortForDeviceFamily();
                if (string.IsNullOrEmpty(portForFamily))
                {
                    return portUserEntry;
                }
                else
                {
                    return portForFamily;
                }
            }
            set
            {
                SetProperty(ref portUserEntry, value);
            }
        }
        #endregion // Port

        #region GetPortForDeviceFamily
        private string GetPortForDeviceFamily()
        {
            switch (this.DeviceFamily)
            {
                case DeviceFamilySelections.HoloLens:
                case DeviceFamilySelections.Phone:
                    {
                        if (this.UsbSelected)
                            return "10080";
                        return this.ProtocolIsHttps ? "443" : "80";
                    }
                case DeviceFamilySelections.IoT:
                    return this.ProtocolIsHttps ? "" : "8080";
                case DeviceFamilySelections.XboxOne:
                    return "11443";
                case DeviceFamilySelections.Desktop:
                    return "";
                default:
                    return "";
            }
        }
        #endregion // GetPortForDeviceFamily

        #region Address
        private string addressUserEntry;
        public string Address
        {
            get
            {
                if (this.UsbAvailable && this.UsbSelected)
                {
                    return "localhost";
                }
                return addressUserEntry;
            }

            set
            {
                SetProperty(ref this.addressUserEntry, value);
            }
        }
        #endregion // Address

        #region AddressEntryEnabled
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
                this.Port );

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
            /// <param name="portal">The DevicePortal associated with the sign in attempt</param>
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
