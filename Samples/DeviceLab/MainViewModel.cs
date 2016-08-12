using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceLab
{
    public class MainViewModel : BindableBase
    {
        //-------------------------------------------------------------------
        //  Constructors
        //-------------------------------------------------------------------
        #region Constructors
        public MainViewModel()
        {


            Diagnostics = new DiagnosticOutputViewModel();

            DebugDiagnosticSink debugDiags = new DebugDiagnosticSink();
            AggregateDiagnosticSink aggDiags = new AggregateDiagnosticSink(Diagnostics, debugDiags);

            this.SignIn = new DeviceSignInViewModel(aggDiags);
            this.SignIn.AddDevicePortalConnectionFactory(new XboxDevicePortalConnectionFactory());
            this.SignIn.AddDevicePortalConnectionFactory(new GenericDevicePortalConnectionFactory());

            this.SignIn.SignInAttempts += OnSignInAttemptCompleted;
        }

        private void OnSignInAttemptCompleted(DeviceSignInViewModel sender, DeviceSignInEventArgs args)
        {
            if(args.StatusCode == System.Net.HttpStatusCode.OK)
            {
                connectedDevices.Add(new DevicePortalViewModel(args.Portal, Diagnostics));
            }
        }
        #endregion // Constructors

        //-------------------------------------------------------------------
        //  Properties
        //-------------------------------------------------------------------
        #region Properties
        public DiagnosticOutputViewModel Diagnostics { get; private set; }
        public DeviceSignInViewModel SignIn { get; private set; }

        #region Connected Devices
        private ObservableCollection<DevicePortalViewModel> connectedDevices;
        public ObservableCollection<DevicePortalViewModel> ConnectedDevices
        {
            get
            {
                if(this.connectedDevices == null)
                {
                    this.connectedDevices = new ObservableCollection<DevicePortalViewModel>();
                    OnPropertyChanged("ConnectedDevices");
                }
                return this.connectedDevices;
            }
        }
        #endregion // Connected Devices

        #endregion // Properties
    }
}
