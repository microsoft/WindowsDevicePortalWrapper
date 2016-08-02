//----------------------------------------------------------------------------------------------
// <copyright file="PerformanceData.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <content>
    /// Wrappers for Performance methods
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// API for getting all running processes
        /// </summary>
        public static readonly string RunningProcessApi = "api/resourcemanager/processes";

        /// <summary>
        /// API for getting system performance
        /// </summary>
        public static readonly string SystemPerfApi = "api/resourcemanager/systemperf";

        /// <summary>
        /// Web socket to get running processes.
        /// </summary>
        private WebSocket<RunningProcesses> deviceProcessesWebSocket;

        /// <summary>
        /// Web socket to get the system perf of the device.
        /// </summary>
        private WebSocket<SystemPerformanceInformation> systemPerfWebSocket;

        /// <summary>
        /// Gets or sets the running processes message received handler.
        /// </summary>
        public event WebSocketMessageReceivedEventHandler<RunningProcesses> RunningProcessesMessageReceived;

        /// <summary>
        /// Gets or sets the system perf message received handler.
        /// </summary>
        public event WebSocketMessageReceivedEventHandler<SystemPerformanceInformation> SystemPerfMessageReceived;

        /// <summary>
        /// Gets the collection of processes running on the device.
        /// </summary>
        /// <returns>RunningProcesses object containing the list of running processes.</returns>
        public async Task<RunningProcesses> GetRunningProcesses()
        {
            return await this.Get<RunningProcesses>(RunningProcessApi);
        }

        /// <summary>
        /// Starts listening for the running processes on the device with them being returned via the RunningProcessesMessageReceived event handler.
        /// </summary>
        /// <returns>Task for connecting to the websocket but not for listening to it.</returns>
        public async Task StartListeningForRunningProcesses()
        {
            if (this.deviceProcessesWebSocket == null)
            {
#if WINDOWS_UWP
                this.deviceProcessesWebSocket = new WebSocket<RunningProcesses>(this.deviceConnection);
#else
                this.deviceProcessesWebSocket = new WebSocket<RunningProcesses>(this.deviceConnection, this.ServerCertificateValidation);
#endif

                this.deviceProcessesWebSocket.WebSocketMessageReceived += this.RunningProcessesReceivedHandler;
            }
            else
            {
                if (this.deviceProcessesWebSocket.IsListeningForMessages)
                {
                    return;
                }
            }

            await this.deviceProcessesWebSocket.StartListeningForMessages(RunningProcessApi);
        }

        /// <summary>
        /// Stop listening for the running processes on the device.
        /// </summary>
        /// <returns>Task for stop listening for processes and disconnecting from the websocket .</returns>
        public async Task StopListeningForRunningProcesses()
        {
            if (this.deviceProcessesWebSocket == null || !this.deviceProcessesWebSocket.IsListeningForMessages)
            {
                return;
            }

            await this.deviceProcessesWebSocket.StopListeningForMessages();
        }

        /// <summary>
        /// Gets system performance information for the device.
        /// </summary>
        /// <returns>SystemPerformanceInformation object containing information such as memory usage.</returns>
        public async Task<SystemPerformanceInformation> GetSystemPerf()
        {
            return await this.Get<SystemPerformanceInformation>(SystemPerfApi);
        }

        /// <summary>
        /// Starts listening for the system performance information for the device with it being returned via the SystemPerfMessageReceived event handler.
        /// </summary>
        /// <returns>Task for connecting to the websocket but not for listening to it.</returns>
        public async Task StartListeningForSystemPerf()
        {
            if (this.systemPerfWebSocket == null)
            {
#if WINDOWS_UWP
                this.systemPerfWebSocket = new WebSocket<SystemPerformanceInformation>(this.deviceConnection);
#else
                this.systemPerfWebSocket = new WebSocket<SystemPerformanceInformation>(this.deviceConnection, this.ServerCertificateValidation);
#endif

                this.systemPerfWebSocket.WebSocketMessageReceived += this.SystemPerfReceivedHandler;
            }
            else
            {
                if (this.systemPerfWebSocket.IsListeningForMessages)
                {
                    return;
                }
            }

            await this.systemPerfWebSocket.StartListeningForMessages(SystemPerfApi);
        }

        /// <summary>
        /// Stop listening for the system performance information for the device.
        /// </summary>
        /// <returns>Task for stop listening for system perf and disconnecting from the websocket .</returns>
        public async Task StopListeningForSystemPerf()
        {
            if (this.systemPerfWebSocket == null || !this.systemPerfWebSocket.IsListeningForMessages)
            {
                return;
            }

            await this.systemPerfWebSocket.StopListeningForMessages();
        }

        /// <summary>
        /// Handler for the processes received event that passes the event to the RunningProcessesMessageReceived handler.
        /// </summary>
        /// <param name="sender">The <see cref="WebSocket{RunningProcesses}"/> object sending the event.</param>
        /// <param name="args">The event data.</param>
        private void RunningProcessesReceivedHandler(
            WebSocket<RunningProcesses> sender,
            WebSocketMessageReceivedEventArgs<RunningProcesses> args)
        {
            if (args.Message != null)
            {
                this.RunningProcessesMessageReceived?.Invoke(
                            this,
                            args);
            }
        }

        /// <summary>
        /// Handler for the system performance information received event that passes the event to the SystemPerfMessageReceived handler.
        /// </summary>
        /// <param name="sender">The <see cref="WebSocket{SystemPerformanceInformation}"/> object sending the event.</param>
        /// <param name="args">The event data.</param>
        private void SystemPerfReceivedHandler(
            WebSocket<SystemPerformanceInformation> sender,
            WebSocketMessageReceivedEventArgs<SystemPerformanceInformation> args)
        {
            if (args.Message != null)
            {
                this.SystemPerfMessageReceived?.Invoke(
                            this,
                            args);
            }
        }

#region Device contract

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

        /// <summary>
        /// Process Info
        /// </summary>
        [DataContract]
        public class DeviceProcessInfo
        {
            /// <summary>
            /// Gets or sets the app name
            /// </summary>
            [DataMember(Name = "AppName")]
            public string AppName { get; set; }

            /// <summary>
            /// Gets or sets CPU usage
            /// </summary>
            [DataMember(Name = "CPUUsage")]
            public float CpuUsage { get; set; }

            /// <summary>
            /// Gets or sets the image name
            /// </summary>
            [DataMember(Name = "ImageName")]
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the process id
            /// </summary>
            [DataMember(Name = "ProcessId")]
            public int ProcessId { get; set; }

            /// <summary>
            /// Gets or sets the owner name
            /// </summary>
            [DataMember(Name = "UserName")]
            public string UserName { get; set; }

            /// <summary>
            /// Gets or sets the package full name
            /// </summary>
            [DataMember(Name = "PackageFullName")]
            public string PackageFullName { get; set; }

            /// <summary>
            /// Gets or sets the Page file usage info
            /// </summary>
            [DataMember(Name = "PageFileUsage")]
            public uint PageFile { get; set; }

            /// <summary>
            /// Gets or sets the working set size
            /// </summary>
            [DataMember(Name = "WorkingSetSize")]
            public uint WorkingSet { get; set; }

            /// <summary>
            /// Gets or sets package working set
            /// </summary>
            [DataMember(Name = "PrivateWorkingSet")]
            public double PrivateWorkingSet { get; set; }

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
            /// Gets or sets virtual size
            /// </summary>
            [DataMember(Name = "VirtualSize")]
            public double VirtualSize { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether or not the process is running
            /// </summary>
            [DataMember(Name = "IsRunning")]
            public bool IsRunning { get; set; }

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

            /// <summary>
            /// String representation of a process
            /// </summary>
            /// <returns>String representation</returns>
            public override string ToString()
            {
                return string.Format("{0} ({1})", this.AppName, this.Name);
            }
        }

        /// <summary>
        /// GPU Adaptors
        /// </summary>
        [DataContract]
        public class GpuAdapter
        {
            /// <summary>
            /// Gets or sets total Dedicated memory
            /// </summary>
            [DataMember(Name = "DedicatedMemory")]
            public uint DedicatedMemory { get; set; }

            /// <summary>
            /// Gets or sets used Dedicated memory
            /// </summary>
            [DataMember(Name = "DedicatedMemoryUsed")]
            public uint DedicatedMemoryUsed { get; set; }

            /// <summary>
            /// Gets or sets description
            /// </summary>
            [DataMember(Name = "Description")]
            public string Description { get; set; }

            /// <summary>
            /// Gets or sets system memory
            /// </summary>
            [DataMember(Name = "SystemMemory")]
            public uint SystemMemory { get; set; }

            /// <summary>
            /// Gets or sets memory used
            /// </summary>
            [DataMember(Name = "SystemMemoryUsed")]
            public uint SystemMemoryUsed { get; set; }

            /// <summary>
            /// Gets or sets engines utilization
            /// </summary>
            [DataMember(Name = "EnginesUtilization")]
            public List<float> EnginesUtilization { get; set; }
        }

        /// <summary>
        /// GPU performance data
        /// </summary>
        [DataContract]
        public class GpuPerformanceData
        {
            /// <summary>
            /// Gets or sets list of available adapters
            /// </summary>
            [DataMember(Name = "AvailableAdapters")]
            public List<GpuAdapter> Adapters { get; set; }
        }

        /// <summary>
        /// Network performance data
        /// </summary>
        [DataContract]
        public class NetworkPerformanceData
        {
            /// <summary>
            /// Gets or sets bytes in
            /// </summary>
            [DataMember(Name = "NetworkInBytes")]
            public int BytesIn { get; set; }

            /// <summary>
            ///  Gets or sets bytes out
            /// </summary>
            [DataMember(Name = "NetworkOutBytes")]
            public int BytesOut { get; set; }
        }

        /// <summary>
        /// Running processes
        /// </summary>
        [DataContract]
        public class RunningProcesses
        {
            /// <summary>
            /// Gets or sets processes info
            /// </summary>
            [DataMember(Name = "Processes")]
            public DeviceProcessInfo[] Processes { get; set; }

            /// <summary>
            /// Checks to see if this process Id is in the list of processes
            /// </summary>
            /// <param name="processId">Process to look for</param>
            /// <returns>whether the process id was found</returns>
            public bool Contains(int processId)
            {
                bool found = false;

                if (this.Processes != null)
                {
                    foreach (DeviceProcessInfo pi in this.Processes)
                    {
                        if (pi.ProcessId == processId)
                        {
                            found = true;
                            break;
                        }
                    }
                }

                return found;
            }

            /// <summary>
            /// Checks for a given package name
            /// </summary>
            /// <param name="packageName">Name of the package to look for</param>
            /// <param name="caseSensitive">Whether we should be case sensitive in our search</param>
            /// <returns>Whether the package was found</returns>
            public bool Contains(string packageName, bool caseSensitive = true)
            {
                bool found = false;

                if (this.Processes != null)
                {
                    foreach (DeviceProcessInfo pi in this.Processes)
                    {
                        if (string.Compare(
                                pi.PackageFullName,
                                packageName,
                                caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            found = true;
                            break;
                        }
                    }
                }

                return found;
            }
        }

        /// <summary>
        /// System performance information
        /// </summary>
        [DataContract]
        public class SystemPerformanceInformation
        {
            /// <summary>
            /// Gets or sets available pages
            /// </summary>
            [DataMember(Name = "AvailablePages")]
            public int AvailablePages { get; set; }

            /// <summary>
            /// Gets or sets commit limit
            /// </summary>
            [DataMember(Name = "CommitLimit")]
            public int CommitLimit { get; set; }

            /// <summary>
            /// Gets or sets committed pages
            /// </summary>
            [DataMember(Name = "CommittedPages")]
            public int CommittedPages { get; set; }

            /// <summary>
            /// Gets or sets CPU load
            /// </summary>
            [DataMember(Name = "CpuLoad")]
            public int CpuLoad { get; set; }

            /// <summary>
            /// Gets or sets IO Other Speed
            /// </summary>
            [DataMember(Name = "IOOtherSpeed")]
            public int IoOtherSpeed { get; set; }

            /// <summary>
            /// Gets or sets IO Read speed
            /// </summary>
            [DataMember(Name = "IOReadSpeed")]
            public int IoReadSpeed { get; set; }

            /// <summary>
            /// Gets or sets IO write speed
            /// </summary>
            [DataMember(Name = "IOWriteSpeed")]
            public int IoWriteSpeed { get; set; }

            /// <summary>
            /// Gets or sets Non paged pool pages
            /// </summary>
            [DataMember(Name = "NonPagedPoolPages")]
            public int NonPagedPoolPages { get; set; }

            /// <summary>
            /// Gets or sets page size
            /// </summary>
            [DataMember(Name = "PageSize")]
            public int PageSize { get; set; }

            /// <summary>
            /// Gets or sets paged pool pages
            /// </summary>
            [DataMember(Name = "PagedPoolPages")]
            public int PagedPoolPages { get; set; }

            /// <summary>
            /// Gets or sets total installed in KB
            /// </summary>
            [DataMember(Name = "TotalInstalledInKb")]
            public int TotalInstalledKb { get; set; }

            /// <summary>
            /// Gets or sets total pages
            /// </summary>
            [DataMember(Name = "TotalPages")]
            public int TotalPages { get; set; }

            /// <summary>
            /// Gets or sets GPU data
            /// </summary>
            [DataMember(Name = "GPUData")]
            public GpuPerformanceData GpuData { get; set; }

            /// <summary>
            /// Gets or sets Networking data
            /// </summary>
            [DataMember(Name = "NetworkingData")]
            public NetworkPerformanceData NetworkData { get; set; }
        }

#endregion // Device contract
    }
}
