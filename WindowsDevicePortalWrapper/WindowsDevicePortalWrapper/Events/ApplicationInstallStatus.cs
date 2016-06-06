using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public delegate void ApplicationInstallStatusEventHandler(Object sender, ApplicationInstallStatusEventArgs args);

    /// <summary>
    /// 
    /// </summary>
    public class ApplicationInstallStatusEventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        public ApplicationInstallStatus Status
        { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public ApplicationInstallPhase Phase
        { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public String Message
        { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="status"></param>
        /// <param name="phase"></param>
        /// <param name="message"></param>
        internal ApplicationInstallStatusEventArgs(ApplicationInstallStatus status,
                                                ApplicationInstallPhase phase,
                                                String message = "")
        {
            Status = status;
            Phase = phase;
            Message = message;
        }
    }

    public enum ApplicationInstallStatus
    {
        None,
        InProgress,
        Completed,
        Failed
    }

    public enum ApplicationInstallPhase
    {
        Idle,
        UninstallingPreviousVersion,
        CopyingFile,
        Installing
    }
}
