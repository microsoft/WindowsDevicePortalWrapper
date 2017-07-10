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
        /// The running processes message received handler.
        /// </summary>
        public event WebSocketMessageReceivedEventHandler<RunningProcesses> RunningProcessesMessageReceived;

        /// <summary>
        /// The system perf message received handler.
        /// </summary>
        public event WebSocketMessageReceivedEventHandler<SystemPerformanceInformation> SystemPerfMessageReceived;

        /// <summary>
        /// Gets the collection of processes running on the device.
        /// </summary>
        /// <returns>RunningProcesses object containing the list of running processes.</returns>
        public async Task<RunningProcesses> GetRunningProcessesAsync()
        {
            return await this.GetAsync<RunningProcesses>(RunningProcessApi);
        }

        /// <summary>
        /// Starts listening for the running processes on the device with them being returned via the RunningProcessesMessageReceived event handler.
        /// </summary>
        /// <returns>Task for connecting to the websocket but not for listening to it.</returns>
        public async Task StartListeningForRunningProcessesAsync()
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

            await this.deviceProcessesWebSocket.ConnectAsync(RunningProcessApi);
            await this.deviceProcessesWebSocket.ReceiveMessagesAsync();
        }

        /// <summary>
        /// Stop listening for the running processes on the device.
        /// </summary>
        /// <returns>Task for stop listening for processes and disconnecting from the websocket .</returns>
        public async Task StopListeningForRunningProcessesAsync()
        {
            await this.deviceProcessesWebSocket.CloseAsync();
        }

        /// <summary>
        /// Gets system performance information for the device.
        /// </summary>
        /// <returns>SystemPerformanceInformation object containing information such as memory usage.</returns>
        public async Task<SystemPerformanceInformation> GetSystemPerfAsync()
        {
            return await this.GetAsync<SystemPerformanceInformation>(SystemPerfApi);
        }

        /// <summary>
        /// Starts listening for the system performance information for the device with it being returned via the SystemPerfMessageReceived event handler.
        /// </summary>
        /// <returns>Task for connecting to the websocket but not for listening to it.</returns>
        public async Task StartListeningForSystemPerfAsync()
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

            await this.systemPerfWebSocket.ConnectAsync(SystemPerfApi);
            await this.systemPerfWebSocket.ReceiveMessagesAsync();
        }

        /// <summary>
        /// Stop listening for the system performance information for the device.
        /// </summary>
        /// <returns>Task for stop listening for system perf and disconnecting from the websocket .</returns>
        public async Task StopListeningForSystemPerfAsync()
        {
            await this.systemPerfWebSocket.CloseAsync();
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
        /// Object representing the app version.  Only present if the process is an app. 
        /// </summary>
        [DataContract]
        public class AppVersion
        {
            /// <summary>
            /// Gets the major version number
            /// </summary>
            [DataMember(Name = "Major")]
            public uint Major { get; private set; }

            /// <summary>
            /// Gets the minor version number
            /// </summary>
            [DataMember(Name = "Minor")]
            public uint Minor { get; private set; }

            /// <summary>
            /// Gets the build number
            /// </summary>
            [DataMember(Name = "Build")]
            public uint Build { get; private set; }

            /// <summary>
            /// Gets the revision number
            /// </summary>
            [DataMember(Name = "Revision")]
            public uint Revision { get; private set; }
        }

        /// <summary>
        /// Process Info.  Contains app information if the process is an app. 
        /// </summary>
        [DataContract]
        public class DeviceProcessInfo
        {
            /// <summary>
            /// Gets the app name. Only present if the process is an app. 
            /// </summary>
            [DataMember(Name = "AppName")]
            public string AppName { get; private set; }

            /// <summary>
            /// Gets CPU Usage as a percentage of available CPU resources (0-100)
            /// </summary>
            [DataMember(Name = "CPUUsage")]
            public float CpuUsage { get; private set; }

            /// <summary>
            /// Gets the image name
            /// </summary>
            [DataMember(Name = "ImageName")]
            public string Name { get; private set; }

            /// <summary>
            /// Gets the process id (pid)
            /// </summary>
            [DataMember(Name = "ProcessId")]
            public uint ProcessId { get; private set; }

            /// <summary>
            /// Gets the user the process is running as. 
            /// </summary>
            [DataMember(Name = "UserName")]
            public string UserName { get; private set; }

            /// <summary>
            /// Gets the package full name.  Only present if the process is an app. 
            /// </summary>
            [DataMember(Name = "PackageFullName")]
            public string PackageFullName { get; private set; }

            /// <summary>
            /// Gets the Page file usage info, in bytes
            /// </summary>
            [DataMember(Name = "PageFileUsage")]
            public ulong PageFile { get; private set; }

            /// <summary>
            /// Gets the working set size, in bytes
            /// </summary>
            [DataMember(Name = "WorkingSetSize")]
            public ulong WorkingSet { get; private set; }

            /// <summary>
            /// Gets package working set, in bytes
            /// </summary>
            [DataMember(Name = "PrivateWorkingSet")]
            public ulong PrivateWorkingSet { get; private set; }

            /// <summary>
            /// Gets session id
            /// </summary>
            [DataMember(Name = "SessionId")]
            public uint SessionId { get; private set; }

            /// <summary>
            /// Gets total commit, in bytes
            /// </summary>
            [DataMember(Name = "TotalCommit")]
            public ulong TotalCommit { get; private set; }

            /// <summary>
            /// Gets virtual size, in bytes
            /// </summary>
            [DataMember(Name = "VirtualSize")]
            public ulong VirtualSize { get; private set; }

            /// <summary>
            /// Gets a value indicating whether or not the app is running 
            /// (versus suspended). Only present if the process is an app.
            /// </summary>
            [DataMember(Name = "IsRunning")]
            public bool IsRunning { get; private set; }

            /// <summary>
            /// Gets publisher. Only present if the process is an app.
            /// </summary>
            [DataMember(Name = "Publisher")]
            public string Publisher { get; private set; }

            /// <summary>
            /// Gets version. Only present if the process is an app.
            /// </summary>
            [DataMember(Name = "Version")]
            public AppVersion Version { get; private set; }

            /// <summary>
            /// Gets a value indicating whether or not the package is a XAP 
            /// package. Only present if the process is an app.
            /// </summary>
            [DataMember(Name = "IsXAP")]
            public bool IsXAP { get; private set; }

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
            /// Gets total Dedicated memory in bytes
            /// </summary>
            [DataMember(Name = "DedicatedMemory")]
            public ulong DedicatedMemory { get; private set; }

            /// <summary>
            /// Gets used Dedicated memory in bytes
            /// </summary>
            [DataMember(Name = "DedicatedMemoryUsed")]
            public ulong DedicatedMemoryUsed { get; private set; }

            /// <summary>
            /// Gets description
            /// </summary>
            [DataMember(Name = "Description")]
            public string Description { get; private set; }

            /// <summary>
            /// Gets system memory in bytes
            /// </summary>
            [DataMember(Name = "SystemMemory")]
            public ulong SystemMemory { get; private set; }

            /// <summary>
            /// Gets memory used in bytes
            /// </summary>
            [DataMember(Name = "SystemMemoryUsed")]
            public ulong SystemMemoryUsed { get; private set; }

            /// <summary>
            /// Gets engines utilization as percent of maximum. 
            /// </summary>
            [DataMember(Name = "EnginesUtilization")]
            public List<double> EnginesUtilization { get; private set; }
        }

        /// <summary>
        /// GPU performance data
        /// </summary>
        [DataContract]
        public class GpuPerformanceData
        {
            /// <summary>
            /// Gets list of available adapters
            /// </summary>
            [DataMember(Name = "AvailableAdapters")]
            public List<GpuAdapter> Adapters { get; private set; }
        }

        /// <summary>
        /// Network performance data
        /// </summary>
        [DataContract]
        public class NetworkPerformanceData
        {
            /// <summary>
            /// Gets current download speed in bytes per second
            /// </summary>
            [DataMember(Name = "NetworkInBytes")]
            public ulong BytesIn { get; private set; }

            /// <summary>
            ///  Gets current upload speed in bytes per second
            /// </summary>
            [DataMember(Name = "NetworkOutBytes")]
            public ulong BytesOut { get; private set; }
        }

        /// <summary>
        /// Running processes
        /// </summary>
        [DataContract]
        public class RunningProcesses
        {
            /// <summary>
            /// Gets list of running processes.
            /// </summary>
            [DataMember(Name = "Processes")]
            public List<DeviceProcessInfo> Processes { get; private set; }

            /// <summary>
            /// Checks to see if this process Id is in the list of processes
            /// </summary>
            /// <param name="processId">Process to look for</param>
            /// <returns>whether the process id was found</returns>
            public bool Contains(int processId)
            {
                bool found = false;

                foreach (DeviceProcessInfo pi in this.Processes)
                {
                    if (pi.ProcessId == processId)
                    {
                        found = true;
                        break;
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
            /// Gets available pages
            /// </summary>
            [DataMember(Name = "AvailablePages")]
            public ulong AvailablePages { get; private set; }

            /// <summary>
            /// Gets commit limit in bytes
            /// </summary>
            [DataMember(Name = "CommitLimit")]
            public uint CommitLimit { get; private set; }

            /// <summary>
            /// Gets committed pages
            /// </summary>
            [DataMember(Name = "CommittedPages")]
            public uint CommittedPages { get; private set; }

            /// <summary>
            /// Gets CPU load as percent of maximum (0 - 100)
            /// </summary>
            [DataMember(Name = "CpuLoad")]
            public uint CpuLoad { get; private set; }

            /// <summary>
            /// Gets IO Other Speed in bytes per second
            /// </summary>
            [DataMember(Name = "IOOtherSpeed")]
            public ulong IoOtherSpeed { get; private set; }

            /// <summary>
            /// Gets IO Read speed in bytes per second. 
            /// </summary>
            [DataMember(Name = "IOReadSpeed")]
            public ulong IoReadSpeed { get; private set; }

            /// <summary>
            /// Gets IO write speed in bytes per second
            /// </summary>
            [DataMember(Name = "IOWriteSpeed")]
            public ulong IoWriteSpeed { get; private set; }

            /// <summary>
            /// Gets Non paged pool pages
            /// </summary>
            [DataMember(Name = "NonPagedPoolPages")]
            public uint NonPagedPoolPages { get; private set; }

            /// <summary>
            /// Gets page size
            /// </summary>
            [DataMember(Name = "PageSize")]
            public uint PageSize { get; private set; }

            /// <summary>
            /// Gets paged pool pages
            /// </summary>
            [DataMember(Name = "PagedPoolPages")]
            public uint PagedPoolPages { get; private set; }

            /// <summary>
            /// Gets total installed in KB
            /// </summary>
            [DataMember(Name = "TotalInstalledInKb")]
            public ulong TotalInstalledKb { get; private set; }

            /// <summary>
            /// Gets total pages
            /// </summary>
            [DataMember(Name = "TotalPages")]
            public uint TotalPages { get; private set; }

            /// <summary>
            /// Gets GPU data
            /// </summary>
            [DataMember(Name = "GPUData")]
            public GpuPerformanceData GpuData { get; private set; }

            /// <summary>
            /// Gets Networking data
            /// </summary>
            [DataMember(Name = "NetworkingData")]
            public NetworkPerformanceData NetworkData { get; private set; }
        }

#endregion // Device contract
    }
}
