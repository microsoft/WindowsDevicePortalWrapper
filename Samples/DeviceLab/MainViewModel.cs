using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Collections.Specialized;
using System.Collections;
using System.ComponentModel;

namespace DeviceLab
{
    public static class CommandExtensions
    {
        public class ChainedCommand
        {
            
            private ICommand executeMe;
            private object arg;

            private ChainedCommand nextChainedCommand;

            public ChainedCommand(ICommand executeMe, object arg, bool start = true)
            {
                this.executeMe = executeMe;
                this.arg = arg;
                this.nextChainedCommand = null;
                this.Status = State.NotStarted;
                if(start)
                {
                    Start();
                }
            }

            public enum State
            {
                NotStarted,
                Waiting,
                Canceled,
                Completed
            };

            public State Status { get; private set; }

            public void Start()
            {
                if(this.Status != State.NotStarted)
                {
                    throw new InvalidOperationException("Must be in the NotStarted state");
                }
                this.Status = State.Waiting;
                this.executeMe.CanExecuteChanged += ExecuteMe_CanExecuteChanged;
                TryExecute();
            }

            public State Cancel()
            {
                this.executeMe.CanExecuteChanged -= ExecuteMe_CanExecuteChanged;
                State oldState = this.Status;
                this.Status = State.Canceled;
                return oldState;
            }

            public ChainedCommand ExecuteNext(ICommand next, object arg)
            {
                this.nextChainedCommand = new ChainedCommand(next, arg, false);
                TryExecuteNext();
                return this.nextChainedCommand;
            }

            private void ExecuteMe_CanExecuteChanged(object sender, EventArgs e)
            {
                TryExecute();
            }

            private void TryExecute()
            {
                if (this.executeMe.CanExecute(arg))
                {
                    this.executeMe.Execute(arg);
                    this.executeMe.CanExecuteChanged -= ExecuteMe_CanExecuteChanged;
                    this.Status = State.Completed;
                    TryExecuteNext();
                }
            }

            private void TryExecuteNext()
            {
                if(this.Status == State.Completed && this.nextChainedCommand != null)
                {
                    this.nextChainedCommand.Start();
                }
            }
        }

        public static ChainedCommand ExecuteWhenReady(this ICommand executeMe, object arg)
        {
            return new ChainedCommand(executeMe, arg);
        }
    }

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

            this.ConnectedDevices.CollectionChanged += OnConnectedDevicesChanged;
        }


        private void OnSignInAttemptCompleted(DeviceSignInViewModel sender, DeviceSignInEventArgs args)
        {
            if(args.StatusCode == System.Net.HttpStatusCode.OK)
            {
                this.ConnectedDevices.Add(new DevicePortalViewModel(args.Portal, Diagnostics));
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

        #region SelectedDevices
        private IList selectedDevices;
        public IList SelectedDevices
        {
            get
            {
                if(selectedDevices == null)
                {
                    selectedDevices = new List<DevicePortalViewModel>();
                }
                return selectedDevices;
            }

            set
            {
                SetProperty(ref selectedDevices, value);
                OnPropertyChanged("SomeSelectedDevicesReady");
                OnPropertyChanged("AllSelectedDevicesReady");
            }
        }
        #endregion // SelectedDevices

        #region SomeDevicesReady
        public bool SomeDevicesReady
        {
            get
            {
                foreach(DevicePortalViewModel dpvm in ConnectedDevices)
                {
                    if(dpvm.Ready)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        #endregion // SomeDevicesReady

        #region AllDevicesReady
        public bool AllDevicesReady
        {
            get
            {
                foreach(DevicePortalViewModel dpvm in ConnectedDevices)
                {
                    if(!dpvm.Ready)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        #endregion // AllDevicesReady

        #region SomeSelectedDevicesReady
        public bool SomeSelectedDevicesReady
        {
            get
            {
                foreach (DevicePortalViewModel dpvm in SelectedDevices)
                {
                    if (dpvm.Ready)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        #endregion // SomeDevicesReady

        #region AllSelectedDevicesReady
        public bool AllSelectedDevicesReady
        {
            get
            {
                foreach (DevicePortalViewModel dpvm in ConnectedDevices)
                {
                    if (!dpvm.Ready)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        #endregion // AllDevicesReady
        #endregion // Properties

        private void OnConnectedDevicesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                if (e.NewItems != null)
                {
                    foreach (DevicePortalViewModel dpvm in e.NewItems)
                    {
                        OnDeviceAdded(dpvm);
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                if(e.OldItems != null)
                {
                    foreach(DevicePortalViewModel dpvm in e.OldItems)
                    {
                        OnDeviceRemoved(dpvm);
                    }
                }
            }
        }

        private void OnDeviceAdded(DevicePortalViewModel dpvm)
        {
            dpvm.PropertyChanged += DevicePropertyChanged;
            dpvm.RefreshDeviceNameCommand.ExecuteWhenReady(this).ExecuteNext(dpvm.StartListeningForSystemPerfCommand, this);
        }

        private void OnDeviceRemoved(DevicePortalViewModel dpvm)
        {
            dpvm.PropertyChanged -= DevicePropertyChanged;
            if(dpvm.StopListeningForSystemPerfCommand.CanExecute(this))
            {
                dpvm.StopListeningForSystemPerfCommand.Execute(this);
            }
        }

        private void DevicePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "Ready")
            {
                OnPropertyChanged("SomeDevicesReady");
                OnPropertyChanged("AllDevicesReady");
                OnPropertyChanged("SomeSelectedDevicesReady");
                OnPropertyChanged("AllSelectedDevicesReady");
            }
        }
    }
}
