//----------------------------------------------------------------------------------------------
// <copyright file="DevicePortalCommandModel.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------
using System;
using Microsoft.Tools.WindowsDevicePortal;
using Prism.Mvvm;

namespace SampleDeviceCollection
{
    /// <summary>
    /// Base class for ViewModel classes that wrap a DevicePortal object.
    /// Provides status for pending DevicePortal operations, diagnostic
    /// output support, an ObservableCommandQueue to enable composing
    /// commands using CommandSequences, etc.
    /// </summary>
    public class DevicePortalCommandModel : BindableBase
    {
        //-------------------------------------------------------------------
        // Constructors
        //-------------------------------------------------------------------
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="DevicePortalCommandModel" /> class.
        /// </summary>
        /// <param name="connection">IDevicePortalConnection object used for connecting</param>
        /// <param name="diags">Diagnostic sink for reporting</param>
        public DevicePortalCommandModel(IDevicePortalConnection connection, IDiagnosticSink diags)
        {
            if (connection == null)
            {
                throw new ArgumentException("Must provide a valid IDevicePortalConnection object");
            }

            this.Connection = connection;
            this.Portal = new DevicePortal(connection);

            this.diagnostics = diags;
            this.commandQueue = new ObservableCommandQueue();
            this.Ready = true;
        }
        #endregion // Constructors

        //-------------------------------------------------------------------
        // Class Members
        //-------------------------------------------------------------------
        #region Class Members
        /// <summary>
        ///  Gets or sets the DevicePortal object encapsulated by this class
        /// </summary>
        public DevicePortal Portal { get; protected set; }

        /// <summary>
        /// Gets or sets the IDevicePortalConnection object encapsulated by this class
        /// </summary>
        public IDevicePortalConnection Connection { get; protected set; }
        #endregion // Class Members

        //-------------------------------------------------------------------
        //  Properties
        //-------------------------------------------------------------------
        #region Properties
        #region Ready
        /// <summary>
        /// The Ready property indicates that there are no RESTful calls in flight via
        /// the underlying DevicePortal object
        /// </summary>
        private bool ready;

        /// <summary>
        /// Gets or sets a value indicating whether the DevicePortal is ready
        /// </summary>
        public bool Ready
        {
            get
            {
                return this.ready;
            }

            protected set
            {
                this.SetProperty(ref this.ready, value);
            }
        }
        #endregion // Ready

        #region Address
        /// <summary>
        /// Gets the IP Address for the device
        /// </summary>
        public string Address
        {
            get
            {
                return this.Portal == null ? "<unknown>" : this.Portal.Address.Split(':')[0];
            }
        }
        #endregion // Address

        #region DeviceFamily
        /// <summary>
        /// Gets the device's DeviceFamily
        /// </summary>
        public string DeviceFamily
        {
            get
            {
                return this.Portal == null ? "<unknown>" : this.Portal.DeviceFamily;
            }
        }
        #endregion // DeviceFamily

        #region OperatingSystemVersion
        /// <summary>
        /// Gets the operating system version for the device
        /// </summary>
        public string OperatingSystemVersion
        {
            get
            {
                return this.Portal == null ? "<unknown>" : this.Portal.OperatingSystemVersion;
            }
        }
        #endregion // OperatingSystemVersion

        #region Platform
        /// <summary>
        /// Gets the platform for the device
        /// </summary>
        public string Platform
        {
            get
            {
                return this.Portal == null ? "<unknown>" : this.Portal.Platform.ToString();
            }
        }
        #endregion // Platform

        #region PlatformName
        /// <summary>
        /// Gets the platform name of the device
        /// </summary>
        public string PlatformName
        {
            get
            {
                return this.Portal == null ? "<unknown>" : this.Portal.PlatformName;
            }
        }
        #endregion // PlatformName
        #endregion // Properties

        //-------------------------------------------------------------------
        // Command Sequences
        //-------------------------------------------------------------------
        #region Command Sequences
        /// <summary>
        /// CommandQueue shared by all command sequences created via a call to CreateCommandSequence.
        /// CommandSequences will not be able to execute in the sense of CanExecute if there is already
        /// another command sequence usign the shared queue
        /// </summary>
        private ObservableCommandQueue commandQueue;

        /// <summary>
        /// Clears the shared command queue
        /// </summary>
        public void ClearCommandQueue()
        {
            this.commandQueue.Clear();
        }

        /// <summary>
        /// Creates a new CommandSequence using the shared command queue
        /// </summary>
        /// <returns>The new CommandSequence</returns>
        public CommandSequence CreateCommandSequence()
        {
            return new CommandSequence(this.commandQueue);
        }
        #endregion // Command Sequences

        //-------------------------------------------------------------------
        // Diagnostics
        //-------------------------------------------------------------------
        #region Diagnostics
        /// <summary>
        /// Destination for reporting diagnostic messages
        /// </summary>
        private IDiagnosticSink diagnostics;

        /// <summary>
        /// Gets protected access to the diagnostic sink
        /// </summary>
        protected IDiagnosticSink Diagnostics
        {
            get
            {
                return this.diagnostics;
            }
        }
            
        /// <summary>
        /// Gets a short string, identifying the device, that is prepended to diagnostic output
        /// </summary>
        protected virtual string DiagnosticMoniker
        {
            get
            {
                return this.Address;
            }
        }

        /// <summary>
        /// Output diagnostic messages with the moniker already prepended to the output
        /// </summary>
        /// <param name="fmt">Format string</param>
        /// <param name="args">Format arguments</param>
        protected virtual void OutputDiagnosticString(string fmt, params object[] args)
        {
            this.diagnostics.OutputDiagnosticString("[{0}] ", this.DiagnosticMoniker);
            this.diagnostics.OutputDiagnosticString(fmt, args);
        }

        /// <summary>
        /// Report an exception to the diagnostic output and clear the command queue
        /// </summary>
        /// <param name="commandName">The command that generated the exception</param>
        /// <param name="exn">The exception caught when attempting to execute the command</param>
        protected virtual void ReportException(string commandName, Exception exn)
        {
            this.OutputDiagnosticString("Exception during {0} command:\n", commandName);
            this.OutputDiagnosticString(exn.Message + "\n");
            this.OutputDiagnosticString(exn.StackTrace + "\n");

            // Clear the command queue to prevent executing any more commands
            this.OutputDiagnosticString("Clearing any queued commands\n");
            this.ClearCommandQueue();
        }
        #endregion // Diagnostics
    }
}
