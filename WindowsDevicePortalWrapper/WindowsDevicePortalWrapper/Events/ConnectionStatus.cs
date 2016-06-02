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
    public delegate void DeviceConnectionStatusEventHandler(Object sender, DeviceConnectionStatusEventArgs args);

    /// <summary>
    /// 
    /// </summary>
    public class DeviceConnectionStatusEventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        public DeviceConnectionStatus Status 
        { get; private set; }
        
        /// <summary>
        /// 
        /// </summary>
        public DeviceConnectionPhase Phase
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
        internal DeviceConnectionStatusEventArgs(DeviceConnectionStatus status,
                                        DeviceConnectionPhase phase,
                                        String message = "")
        {
            Status = status;
            Phase = phase;
            Message = message;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public enum DeviceConnectionStatus
    {
        None,
        Connecting,
        Connected,
        Failed,
    }

    /// <summary>
    /// 
    /// </summary>
    public enum DeviceConnectionPhase
    {
        Idle,
        AcquiringCertificate,
        DeterminingConnectionRequirements,
        RequestingOperatingSystemInformation,
        ConnectingToTargetNetwork,
        UpdatingDeviceAddress
    }
}
