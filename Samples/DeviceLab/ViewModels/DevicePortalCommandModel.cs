using Microsoft.Tools.WindowsDevicePortal;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DeviceLab
{
    public class DevicePortalCommandModel : BindableBase
    {
        //-------------------------------------------------------------------
        //  Protected Members
        //-------------------------------------------------------------------
        #region Protected Members
        protected IDiagnosticSink diagnostics;
        protected DevicePortal portal;

        #endregion // Protected Members

        //-------------------------------------------------------------------
        // Constructors
        //-------------------------------------------------------------------
        #region Constructors
        public DevicePortalCommandModel(DevicePortal portal, IDiagnosticSink diags)
        {
            this.portal = portal;
            this.diagnostics = diags;
            this.commandQueue = new ObservableCommandQueue();
            this.Ready = true;
        }
        #endregion // Constructors

        //-------------------------------------------------------------------
        //  Properties
        //-------------------------------------------------------------------
        #region Properties

        /// <summary>
        /// The Ready property indicates that there are no REST calls in flight via
        /// the underlying DevicePortal object
        /// </summary>
        #region Ready
        private bool ready;
        public bool Ready
        {
            get
            {
                return this.ready;
            }
            protected set
            {
                SetProperty(ref this.ready, value);
            }
        }
        #endregion // Ready
        #endregion // Properties

        //-------------------------------------------------------------------
        // Command Sequences
        //-------------------------------------------------------------------
        #region Command Sequences
        private ObservableCommandQueue commandQueue;

        protected void ClearCommandQueue()
        {
            this.commandQueue.Clear();
        }

        public CommandSequence CreateCommandSequence()
        {
            return new CommandSequence(this.commandQueue);
        }

        #endregion // Command Sequences

    }
}
