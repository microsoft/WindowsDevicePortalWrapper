//----------------------------------------------------------------------------------------------
// <copyright file="MainViewModel.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Net;
using System.Windows;
using System.Windows.Input;
using Microsoft.Tools.WindowsDevicePortal;
using Prism.Commands;
using Prism.Mvvm;

namespace SampleDeviceCollection
{
    /// <summary>
    /// View model for the main view.
    /// </summary>
    public class MainViewModel : BindableBase
    {
        //-------------------------------------------------------------------
        //  Constructors
        //-------------------------------------------------------------------
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel" /> class.
        /// </summary>
        public MainViewModel()
        {
            this.Diagnostics = new DiagnosticOutputViewModel();

            DiagnosticSinks.DebugDiagnosticSink debugDiags = new DiagnosticSinks.DebugDiagnosticSink();
            DiagnosticSinks.AggregateDiagnosticSink aggDiags = new DiagnosticSinks.AggregateDiagnosticSink(this.Diagnostics, debugDiags);

            this.SignIn = new DeviceSignInViewModel();

            this.SignIn.SignInAttempted += this.OnSignInAttemptCompleted;

            this.ConnectedDevices.CollectionChanged += this.OnConnectedDevicesChanged;
        }
        #endregion // Constructors
        
        /// <summary>
        /// Handler for SignInAttempt events
        /// </summary>
        /// <param name="sender">DeviceSignInViewModel originating the event</param>
        /// <param name="args">Arguments for the event</param>
        private void OnSignInAttemptCompleted(DeviceSignInViewModel sender, DeviceSignInViewModel.SignInAttemptEventArgs args)
        {
            this.ConnectedDevices.Add(new DevicePortalViewModel(args.Connection, this.Diagnostics));
        }

        //-------------------------------------------------------------------
        //  Properties
        //-------------------------------------------------------------------
        #region Properties
        /// <summary>
        /// Gets the diagnostic sink for this view model
        /// </summary>
        public DiagnosticOutputViewModel Diagnostics { get; private set; }

        /// <summary>
        /// Gets the ViewModel for the sign-in portion of the user experience
        /// </summary>
        public DeviceSignInViewModel SignIn { get; private set; }

        #region Connected Devices
        /// <summary>
        /// Collection of view models for devices that have been connected to
        /// </summary>
        private ObservableCollection<DevicePortalViewModel> connectedDevices;

        /// <summary>
        /// Gets the collection of view models for the connected devices
        /// </summary>
        public ObservableCollection<DevicePortalViewModel> ConnectedDevices
        {
            get
            {
                if (this.connectedDevices == null)
                {
                    this.connectedDevices = new ObservableCollection<DevicePortalViewModel>();
                    this.OnPropertyChanged("ConnectedDevices");
                }

                return this.connectedDevices;
            }
        }
        #endregion // Connected Devices

        #region SelectedDevices
        /// <summary>
        /// Subset of the collection of connected devices that have been selected by the user
        /// </summary>
        private IList selectedDevices;

        /// <summary>
        /// Gets or sets the subset of connected devices that have been selected by the user.
        /// </summary>
        public IList SelectedDevices
        {
            get
            {
                if (this.selectedDevices == null)
                {
                    this.selectedDevices = new List<DevicePortalViewModel>();
                }

                return this.selectedDevices;
            }

            set
            {
                // This will be called when the contents of the list change without changing
                // the actual list itself (i.e. same list, but different contents)
                if (this.selectedDevices != value)
                {
                    this.selectedDevices = value;
                }

                // We always want to fire the event to handle the case when contents change
                this.OnPropertyChanged("SelectedDevices");
            }
        }
        #endregion // SelectedDevices

        #region SomeDevicesReady
        /// <summary>
        /// Gets a value indicating whether some of the devices in the ConnectedDevices collection are ready
        /// </summary>
        public bool SomeDevicesReady
        {
            get
            {
                foreach (DevicePortalViewModel dpvm in this.ConnectedDevices)
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

        #region AllDevicesReady
        /// <summary>
        /// Gets a value indicating whether all of the devices in the ConnectedDevices collection are ready
        /// </summary>
        public bool AllDevicesReady
        {
            get
            {
                foreach (DevicePortalViewModel dpvm in this.ConnectedDevices)
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

        //-------------------------------------------------------------------
        // Commands
        //-------------------------------------------------------------------
        #region Commands
        #region SelectAllDevicesCommand
        /// <summary>
        /// Command to select all connected devices
        /// </summary>
        private DelegateCommand selectAllDevicesCommand;

        /// <summary>
        /// Gets the command to select all connected devices
        /// </summary>
        public ICommand SelectAllDevicesCommand
        {
            get
            {
                if (this.selectAllDevicesCommand == null)
                {
                    this.selectAllDevicesCommand = new DelegateCommand(this.ExecuteSelectAllDevices, this.CanExecuteSelectAllDevices);
                    this.selectAllDevicesCommand.ObservesProperty(() => this.SomeDevicesReady);
                    this.selectAllDevicesCommand.ObservesProperty(() => this.SelectedDevices);
                }

                return this.selectAllDevicesCommand;
            }
        }

        /// <summary>
        /// Predicate for the SeletAllDevices command
        /// </summary>
        /// <returns>Result indicates whether the command can execute</returns>
        private bool CanExecuteSelectAllDevices()
        {
            return this.SomeDevicesReady && (this.SelectedDevices.Count < this.ConnectedDevices.Count);
        }

        /// <summary>
        /// Performs the action for the SelectAllDevices command
        /// </summary>
        private void ExecuteSelectAllDevices()
        {
            List<DevicePortalViewModel> newSelection = new List<DevicePortalViewModel>();
            foreach (DevicePortalViewModel dpvm in this.ConnectedDevices)
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
        /// <summary>
        /// Command to un-select all the devices
        /// </summary>
        private DelegateCommand unSelectAllDevicesCommand;

        /// <summary>
        /// Gets the command to un-select all the devices
        /// </summary>
        public ICommand UnSelectAllDevicesCommand
        {
            get
            {
                if (this.unSelectAllDevicesCommand == null)
                {
                    this.unSelectAllDevicesCommand = new DelegateCommand(this.ExecuteUnSelectAllDevices, this.CanExecuteUnSelectAllDevices);
                    this.unSelectAllDevicesCommand.ObservesProperty(() => this.SelectedDevices);
                }

                return this.unSelectAllDevicesCommand;
            }
        }

        /// <summary>
        /// Predicate for the UnSelectAllDevices command
        /// </summary>
        /// <returns>Result indicates whether the command can excecute</returns>
        private bool CanExecuteUnSelectAllDevices()
        {
            return this.SelectedDevices.Count > 0;
        }

        /// <summary>
        /// Performs the action of the UnSelectAllDevices command
        /// </summary>
        private void ExecuteUnSelectAllDevices()
        {
            this.SelectedDevices = new List<DevicePortalViewModel>();
        }
        #endregion // UnSelectAllDevicesCommand

        #region RebootSelectedDevicesCommand
        /// <summary>
        /// Command to reboot all the selected devices
        /// </summary>
        private DelegateCommand rebootSelectedDevicesCommand;

        /// <summary>
        /// Gets the command to reboot all the selected devices
        /// </summary>
        public ICommand RebootSelectedDevicesCommand
        {
            get
            {
                if (this.rebootSelectedDevicesCommand == null)
                {
                    this.rebootSelectedDevicesCommand = new DelegateCommand(this.ExecuteRebootSelectedDevices, this.CanExecuteRebootSelectedDevices);
                    this.rebootSelectedDevicesCommand.ObservesProperty(() => this.SelectedDevices);
                }

                return this.rebootSelectedDevicesCommand;
            }
        }

        /// <summary>
        /// Predicate for the command to reboot all the devices
        /// </summary>
        /// <returns>Result indicates whether the command can execute.</returns>
        private bool CanExecuteRebootSelectedDevices()
        {
            return this.SelectedDevices.Count > 0;
        }

        /// <summary>
        /// Performs the action of the RebootSelectedDevices command
        /// </summary>
        private void ExecuteRebootSelectedDevices()
        {
            // When devices become busy, they will be reomoved from the selected devices collection.
            // So, make a local copy of the devices we want to reboot:
            List<DevicePortalViewModel> rebootList = new List<DevicePortalViewModel>();
            foreach (DevicePortalViewModel dpvm in this.SelectedDevices)
            {
                if (dpvm.RebootCommand.CanExecute(this))
                {
                    rebootList.Add(dpvm);
                }
            }

            foreach (DevicePortalViewModel dpvm in rebootList)
            {
                dpvm.RebootCommand.Execute(this);
            }
        }
        #endregion // RebootSelectedDevicesCommand

        #region RemoveDeviceCommand
        /// <summary>
        /// Command to remove a device from the collection of connected devices
        /// </summary>
        private DelegateCommand<object> removeDeviceCommand;
        // NOTE: Ideally, the template parameter would be DevicePortalViewModel, rather than object.
        // Unfortunately, this would sporadically throw an invalid cast exception from within the
        // DelegateCommand<> constructor in the PRISM library. Apparently, the object passed to the
        // constructor cannot always be cast to something that would rightfully belong to the 
        // collection. 

        /// <summary>
        /// Gets the command that removes a device from the collection of connected devices
        /// </summary>
        public ICommand RemoveDeviceCommand
        {
            get
            {
                if (this.removeDeviceCommand == null)
                {
                    this.removeDeviceCommand = new DelegateCommand<object>(this.ExecuteRemoveDeviceCommand, this.CanExecuteRemoveDeviceCommand);
                    this.removeDeviceCommand.ObservesProperty(() => this.SomeDevicesReady);
                }

                return this.removeDeviceCommand;
            }
        }

        /// <summary>
        /// Predicate for the RemoveDevice command
        /// </summary>
        /// <param name="arg">The device to be removed from the collection</param>
        /// <returns>Result indicates whether the command can execute</returns>
        private bool CanExecuteRemoveDeviceCommand(object arg)
        {
            DevicePortalViewModel dpvm = arg as DevicePortalViewModel;
            if (dpvm != null)
            {
                return dpvm.Ready;
            }

            return false;
        }

        /// <summary>
        /// Peform the action for the RemoveDevice command
        /// </summary>
        /// <param name="obj">The device to be removed from the collection of connected devices</param>
        private void ExecuteRemoveDeviceCommand(object obj)
        {
            DevicePortalViewModel dpvm = obj as DevicePortalViewModel;
            if (dpvm != null)
            {
                this.RemoveDevice(dpvm);
            }
        }
        #endregion // RemoveDeviceCommand
        #endregion // Commands

        /// <summary>
        /// Handle changes to the collection of connected devices
        /// </summary>
        /// <param name="sender">Object that originated the event</param>
        /// <param name="e">Parameters for the event</param>
        private void OnConnectedDevicesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                if (e.NewItems != null)
                {
                    foreach (DevicePortalViewModel dpvm in e.NewItems)
                    {
                        this.OnDeviceAdded(dpvm);
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                if (e.OldItems != null)
                {
                    foreach (DevicePortalViewModel dpvm in e.OldItems)
                    {
                        this.OnDeviceRemoved(dpvm);
                    }
                }
            }
        }

        /// <summary>
        /// Perform additional initialization on a device that has been newly added to the collection of connected devices
        /// </summary>
        /// <param name="dpvm">The device that has been added</param>
        private void OnDeviceAdded(DevicePortalViewModel dpvm)
        {
            dpvm.PropertyChanged += this.DevicePropertyChanged;

            CommandSequence cmdSeq = dpvm.CreateCommandSequence();
            cmdSeq.RegisterCommand(dpvm.ReestablishConnectionCommand);
            cmdSeq.RegisterCommand(dpvm.RefreshDeviceNameCommand);
            cmdSeq.RegisterCommand(dpvm.StartListeningForSystemPerfCommand);
            cmdSeq.Execute(null);
        }

        /// <summary>
        /// Tidy up a device that has just been removed from the collection of connected devices
        /// </summary>
        /// <param name="dpvm">The device just removed</param>
        private void OnDeviceRemoved(DevicePortalViewModel dpvm)
        {
            dpvm.PropertyChanged -= this.DevicePropertyChanged;
            if (dpvm.StopListeningForSystemPerfCommand.CanExecute(this))
            {
                dpvm.StopListeningForSystemPerfCommand.Execute(this);
            }
        }

        /// <summary>
        /// Removes a device from the collection and clears any pending commands
        /// </summary>
        /// <param name="removeMe">The device to remove</param>
        private void RemoveDevice(DevicePortalViewModel removeMe)
        {
            removeMe.ClearCommandQueue();
            this.ConnectedDevices.Remove(removeMe);
        }

        /// <summary>
        /// Event handler to monitor property changes on each of the elements in the collection of connected devices
        /// </summary>
        /// <param name="sender">Device that originated the event</param>
        /// <param name="e">The arguments for the event</param>
        private void DevicePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Ready")
            {
                DevicePortalViewModel dpvmSender = sender as DevicePortalViewModel;
                if (dpvmSender != null && !dpvmSender.Ready)
                {
                    // A device is no-longer ready, so remove it from the current selection
                    List<DevicePortalViewModel> updatedSelection = new List<DevicePortalViewModel>();
                    foreach (DevicePortalViewModel dpvm in this.SelectedDevices)
                    {
                        if (dpvm.Ready)
                        {
                            updatedSelection.Add(dpvm);
                        }
                    }

                    this.SelectedDevices = updatedSelection;
                }

                this.OnPropertyChanged("SomeDevicesReady");
                this.OnPropertyChanged("AllDevicesReady");
            }

            if (e.PropertyName == "ConnectionStatus")
            {
                DevicePortalViewModel dpvmSender = sender as DevicePortalViewModel;
                if (dpvmSender.ConnectionStatus == DeviceConnectionStatus.Failed)
                {
                    // Check for authentication failure and warn the user about bad credentials
                    if (dpvmSender.Portal.ConnectionHttpStatusCode == HttpStatusCode.Unauthorized)
                    {
                        MessageBox.Show(
                            "Connection Unauthorized. Please double check your authentication credentials",
                            "Unauthorized",
                            MessageBoxButton.OK,
                            MessageBoxImage.Exclamation);
                        this.RemoveDevice(dpvmSender);
                    }
                }
            }

            if (e.PropertyName == "Address")
            {
                DevicePortalViewModel dpvmSender = sender as DevicePortalViewModel;
                if (this.DeviceIsDuplicate(dpvmSender))
                {
                    MessageBox.Show(
                        string.Format("You already have a connection for this device address: {0}", dpvmSender.Address),
                        "Duplicate",
                        MessageBoxButton.OK,
                        MessageBoxImage.Exclamation);
                    this.RemoveDevice(dpvmSender);
                }
            }
        }

        /// <summary>
        /// Determine whether the provided DevicePortalViewModel is already represented in the collection
        /// </summary>
        /// <param name="dpvmToCheck">The DevicePortalViewModel for which you want to check for duplicates</param>
        /// <returns>True if the device is already represented in the collection</returns>
        private bool DeviceIsDuplicate(DevicePortalViewModel dpvmToCheck)
        {
            foreach (DevicePortalViewModel dpvm in this.ConnectedDevices)
            {
                if (dpvmToCheck == dpvm)
                {
                    continue;
                }

                if (dpvmToCheck.Address == dpvm.Address)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
