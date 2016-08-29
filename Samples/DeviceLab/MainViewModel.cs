using Prism.Commands;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Collections.Specialized;
using System.Collections;
using System.ComponentModel;
using System;
using Microsoft.Tools.WindowsDevicePortal;

namespace DeviceLab
{
    public class NewDeviceHelper : ICommand
    {
        // TODO: Don't close over a DevicePortalViewModel
        // Instead, should just hook up the CanExecuteChanged events on
        // the commands
        private DevicePortalViewModel dpvm;
        private Queue<ICommand> commandQueue;
        private bool executionStarted;

        // TODO:
        public event EventHandler CanExecuteChanged;

        public NewDeviceHelper(DevicePortalViewModel dpvm)
        {
            this.dpvm = dpvm;
            
            this.commandQueue = new Queue<ICommand>();
            this.executionStarted = false;
        }

        public void Enqueue(ICommand command)
        {
            if (this.executionStarted)
            {
                throw new InvalidOperationException("Execution already started");
            }

            this.commandQueue.Enqueue(command);
        }

        private void Dpvm_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "Ready")
            {
                if(this.dpvm.Ready)
                {
                    DoNextCommand();
                }
            }
        }

        private void DoNextCommand()
        {
            if(this.commandQueue == null)
            {
                throw new InvalidOperationException("Attempting to execute commands without a command queue");
            }

            if(this.commandQueue.Count == 0)
            {
                throw new InvalidOperationException("Attempting to execute command on an empty command queue");
            }

            if(this.commandQueue.Peek().CanExecute(null))
            {
                ICommand nextCommand = this.commandQueue.Dequeue();

                if (this.commandQueue.Count == 0)
                {
                    // Executing the next command will probably trigger property changed
                    // events and since this is the last command we want to disconnect from 
                    // those events
                    this.dpvm.PropertyChanged -= Dpvm_PropertyChanged;
                    this.executionStarted = false;
                }
                nextCommand.Execute(null);
            }
        }

        public bool CanExecute(object parameter)
        {
            if(this.executionStarted)
            {
                return false;
            }

            if(this.commandQueue.Count > 0)
            {
                return this.commandQueue.Peek().CanExecute(null);
            }

            return false;
        }

        public void Execute(object parameter)
        {
            if(this.executionStarted)
            {
                throw new InvalidOperationException("Execution already started");
            }

            dpvm.PropertyChanged += Dpvm_PropertyChanged;
            DoNextCommand();
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
            DevicePortal portal = args.Portal;
            // See if the device is already in the list
            foreach (DevicePortalViewModel dpvm in this.ConnectedDevices)
            {
                if(dpvm.Address == portal.Address)
                {
                    this.Diagnostics.OutputDiagnosticString("[!!] The device with address {0} is already in the list\n", portal.Address);
                    return;
                }
            }

            this.ConnectedDevices.Add(new DevicePortalViewModel(args.Portal, Diagnostics));
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
                // This will be called when the contents of the list change without changing
                // the actual list itself (i.e. same list, but different contents)
                if(this.selectedDevices != value)
                {
                    this.selectedDevices = value;
                }
                // We always want to fire the event to handle the case when contents change
                OnPropertyChanged("SelectedDevices");
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
        #endregion // Properties

        //-------------------------------------------------------------------
        // Commands
        //-------------------------------------------------------------------
        #region Commands
        #region SelectAllDevicesCommand
        private DelegateCommand selectAllDevicesCommand;
        public ICommand SelectAllDevicesCommand
        {
            get
            {
                if(this.selectAllDevicesCommand == null)
                {
                    this.selectAllDevicesCommand = new DelegateCommand(ExecuteSelectAllDevices, CanExecuteSelectAllDevices);
                    this.selectAllDevicesCommand.ObservesProperty(() => SomeDevicesReady);
                    this.selectAllDevicesCommand.ObservesProperty(() => SelectedDevices);
                }
                return this.selectAllDevicesCommand;
            }
        }

        private bool CanExecuteSelectAllDevices()
        {
            return this.SomeDevicesReady && (this.SelectedDevices.Count < this.ConnectedDevices.Count);
        }

        private void ExecuteSelectAllDevices()
        {
            List<DevicePortalViewModel> newSelection = new List<DevicePortalViewModel>();
            foreach(DevicePortalViewModel dpvm in this.ConnectedDevices)
            {
                if (dpvm.Ready)
                {
                    newSelection.Add(dpvm);
                }
            }
            this.SelectedDevices = newSelection;
        }
        #endregion // SelectAllDevicesCommand

        #region UnSelectAllDevicesCommand
        private DelegateCommand unSelectAllDevicesCommand;
        public ICommand UnSelectAllDevicesCommand
        {
            get
            {
                if(this.unSelectAllDevicesCommand == null)
                {
                    this.unSelectAllDevicesCommand = new DelegateCommand(ExecuteUnSelectAllDevices, CanExecuteUnSelectAllDevices);
                    this.unSelectAllDevicesCommand.ObservesProperty(() => SelectedDevices);
                }
                return this.unSelectAllDevicesCommand;
            }
        }

        private bool CanExecuteUnSelectAllDevices()
        {
            return this.SelectedDevices.Count > 0;
        }

        private void ExecuteUnSelectAllDevices()
        {
            this.SelectedDevices = new List<DevicePortalViewModel>();
        }
        #endregion // UnSelectAllDevicesCommand

        #region RebootSelectedDevicesCommand
        private DelegateCommand rebootSelectedDevicesCommand;
        public ICommand RebootSelectedDevicesCommand
        {
            get
            {
                if(this.rebootSelectedDevicesCommand == null)
                {
                    this.rebootSelectedDevicesCommand = new DelegateCommand(ExecuteRebootSelectedDevices, CanExecuteRebootSelectedDevices);
                    this.rebootSelectedDevicesCommand.ObservesProperty(() => this.SelectedDevices);
                }
                return rebootSelectedDevicesCommand;
            }
        }

        private bool CanExecuteRebootSelectedDevices()
        {
            return this.SelectedDevices.Count > 0;
        }

        private void ExecuteRebootSelectedDevices()
        {
            // When devices become busy, they will be reomoved from the selected devices collection.
            // So, make a local copy of the devices we want to reboot:
            List<DevicePortalViewModel> rebootList = new List<DevicePortalViewModel>();
            foreach(DevicePortalViewModel dpvm in this.SelectedDevices)
            {
                if(dpvm.RebootCommand.CanExecute(this))
                {
                    rebootList.Add(dpvm);
                }
            }

            foreach(DevicePortalViewModel dpvm in rebootList)
            {
                dpvm.RebootCommand.Execute(this);
            }
        }
        #endregion // RebootSelectedDevicesCommand

        #region RemoveDeviceCommand
        // NOTE: Ideally, the template parameter would be DevicePortalViewMOdel, rather than object.
        // Unfortunately, this would sporadically throw an invalid cast exception from within the
        // DelegateCommand<> constructor in the PRISM library. Apparently, the object passed to the
        // constructor cannot always be cast to something that would rightfully belong to the 
        // collection. 
        private DelegateCommand<object> removeDeviceCommand;
        public ICommand RemoveDeviceCommand
        {
            get
            {
                if(this.removeDeviceCommand == null)
                {
                    this.removeDeviceCommand = new DelegateCommand<object>(ExecuteRemoveDeviceCommand, CanExecuteRemoveDeviceCommand);
                    this.removeDeviceCommand.ObservesProperty(() => SomeDevicesReady);
                }
                return this.removeDeviceCommand;
            }
        }

        private bool CanExecuteRemoveDeviceCommand(object arg)
        {
            DevicePortalViewModel dpvm = arg as DevicePortalViewModel;
            if (dpvm != null)
            {
                return dpvm.Ready;
            }
            return false;
        }

        private void ExecuteRemoveDeviceCommand(object obj)
        {
            DevicePortalViewModel dpvm = obj as DevicePortalViewModel;
            if (dpvm != null)
            {
                this.ConnectedDevices.Remove(dpvm);
            }
        }
        #endregion // RemoveDeviceCommand
        #endregion // Commands

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
            // TODO:
            //CompositeCommand command = new CompositeCommand();
            //command.RegisterCommand(dpvm.RefreshDeviceNameCommand);
            //command.RegisterCommand(dpvm.StartListeningForSystemPerfCommand);
            //command.Execute(null);

            NewDeviceHelper ndh = new NewDeviceHelper(dpvm);
            ndh.Enqueue(dpvm.ReestablishConnectionCommand);
            ndh.Enqueue(dpvm.RefreshDeviceNameCommand);
            ndh.Enqueue(dpvm.StartListeningForSystemPerfCommand);
            ndh.Execute(null);
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
                DevicePortalViewModel dpvmSender = sender as DevicePortalViewModel;
                if (dpvmSender != null && !dpvmSender.Ready)
                { 
                    // A device is no-longer ready, so remove it from the current selection
                    List<DevicePortalViewModel> updatedSelection = new List<DevicePortalViewModel>();
                    foreach(DevicePortalViewModel dpvm in this.SelectedDevices)
                    {
                        if(dpvm.Ready)
                        {
                            updatedSelection.Add(dpvm);
                        }
                    }
                    this.SelectedDevices = updatedSelection;
                }

                OnPropertyChanged("SomeDevicesReady");
                OnPropertyChanged("AllDevicesReady");
            }
        }
    }
}
