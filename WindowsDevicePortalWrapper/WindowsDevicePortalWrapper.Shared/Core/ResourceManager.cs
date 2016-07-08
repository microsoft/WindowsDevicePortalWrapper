//----------------------------------------------------------------------------------------------
// <copyright file="ResourceManager.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <content>
    /// Wrappers for Resource Manager methods.
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// API for getting processes currently running on the device.
        /// </summary>
        private static readonly string ProcessesApi = "/api/resourcemanager/processes";

#if !WINDOWS_UWP
        /// <summary>
        /// Web socket to get the processes currently running on the device.
        /// </summary>
        private WebSocket<DeviceProcesses> deviceProcessesWebSocket;

        /// <summary>
        /// Gets or sets the processes message received handler.
        /// </summary>
        public WebSocketMessageReceivedEventHandler<DeviceProcesses> ProcessesMessageReceived
        {
            get;
            set;
        }

        /// <summary>
        /// Starts listening for the running processes on the device with them being returned via the ProcessesMessageReceived event handler.
        /// </summary>
        /// <returns>Task for connecting to the websocket but not for listening to it.</returns>
        public async Task StartListeningForProcesses()
        {
            if (this.deviceProcessesWebSocket == null)
            {
                this.deviceProcessesWebSocket = new WebSocket<DeviceProcesses>(this.deviceConnection, this.ServerCertificateValidation);

                this.deviceProcessesWebSocket.WebSocketMessageReceived += this.ProcessesReceivedHandler;
            }
            else
            {
                if (this.deviceProcessesWebSocket.IsListeningForMessages)
                {
                    return;
                }
            }

            await this.deviceProcessesWebSocket.OpenConnection(ProcessesApi);

            // Do not await on the actual listening.
            Task listenTask = this.deviceProcessesWebSocket.StartListeningForMessages();
        }

        /// <summary>
        /// Stop listening for the running processes on the device.
        /// </summary>
        /// <returns>Task for stop listening for processes and disconnecting from the websocket .</returns>
        public async Task StopListeningForProcesses()
        {
            if (this.deviceProcessesWebSocket == null || !this.deviceProcessesWebSocket.IsListeningForMessages)
            {
                return;
            }

            await this.deviceProcessesWebSocket.CloseConnection();
        }

        /// <summary>
        /// Handler for the processes received event that passes the event to the ProcessesMessageReceived handler.
        /// </summary>
        /// <param name="sender">The object sending the event.</param>
        /// <param name="args">The event data.</param>
        private void ProcessesReceivedHandler(
            object sender,
            WebSocketMessageReceivedEventArgs<DeviceProcesses> args)
        {
            if (args.Message != null)
            {
                this.ProcessesMessageReceived?.Invoke(
                            this,
                            args);
            }
        }
#endif // !WINDOWS_UWP

        #region Data contract

        /// <summary>
        /// Object representing a list of processes
        /// </summary>
        [DataContract]
        public class DeviceProcesses
        {
            /// <summary>
            /// Gets or sets a list of the packages
            /// </summary>
            [DataMember(Name = "Processes")]
            public List<ProcessInfo> Processes { get; set; }
        }

        /// <summary>
        /// Object representing the process information
        /// </summary>
        [DataContract]
        public class ProcessInfo
        {
            /// <summary>
            /// Gets or sets CPU usage
            /// </summary>
            [DataMember(Name = "CPUUsage")]
            public double CPUUsage { get; set; }

            /// <summary>
            /// Gets or sets image name
            /// </summary>
            [DataMember(Name = "ImageName")]
            public string ImageName { get; set; }

            /// <summary>
            /// Gets or sets page file usage
            /// </summary>
            [DataMember(Name = "PageFileUsage")]
            public double PageFileUsage { get; set; }

            /// <summary>
            /// Gets or sets package working set
            /// </summary>
            [DataMember(Name = "PrivateWorkingSet")]
            public double PrivateWorkingSet { get; set; }

            /// <summary>
            /// Gets or sets process id
            /// </summary>
            [DataMember(Name = "ProcessId")]
            public uint ProcessId { get; set; }

            /// <summary>
            /// Gets or sets session id
            /// </summary>
            [DataMember(Name = "SessionId")]
            public uint SessionId { get; set; }

            /// <summary>
            /// Gets or sets total commit
            /// </summary>
            [DataMember(Name = "TotalCommit")]
            public double TotalCommit { get; set; }

            /// <summary>
            /// Gets or sets user name
            /// </summary>
            [DataMember(Name = "UserName")]
            public string UserName { get; set; }

            /// <summary>
            /// Gets or sets virtual size
            /// </summary>
            [DataMember(Name = "VirtualSize")]
            public double VirtualSize { get; set; }

            /// <summary>
            /// Gets or sets working set size
            /// </summary>
            [DataMember(Name = "WorkingSetSize")]
            public double WorkingSetSize { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether or not the process is running
            /// </summary>
            [DataMember(Name = "IsRunning")]
            public bool IsRunning { get; set; }

            /// <summary>
            /// Gets or sets package full name
            /// </summary>
            [DataMember(Name = "PackageFullName")]
            public string PackageFullName { get; set; }

            /// <summary>
            /// Gets or sets app name
            /// </summary>
            [DataMember(Name = "AppName")]
            public string AppName { get; set; }

            /// <summary>
            /// Gets or sets publisher
            /// </summary>
            [DataMember(Name = "Publisher")]
            public string Publisher { get; set; }

            /// <summary>
            /// Gets or sets version
            /// </summary>
            [DataMember(Name = "Version")]
            public ProcessVersion Version { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether or not the package is a XAP package
            /// </summary>
            [DataMember(Name = "IsXAP")]
            public bool IsXAP { get; set; }
        }

        /// <summary>
        /// Object representing the process version.
        /// </summary>
        [DataContract]
        public class ProcessVersion
        {
            /// <summary>
            /// Gets or sets the major version number
            /// </summary>
            [DataMember(Name = "Major")]
            public uint Major { get; set; }

            /// <summary>
            /// Gets or sets the minor version number
            /// </summary>
            [DataMember(Name = "Minor")]
            public uint Minor { get; set; }

            /// <summary>
            /// Gets or sets the build number
            /// </summary>
            [DataMember(Name = "Build")]
            public uint Build { get; set; }

            /// <summary>
            /// Gets or sets the revision number
            /// </summary>
            [DataMember(Name = "Revision")]
            public uint Revision { get; set; }
        }

        #endregion
    }
}
