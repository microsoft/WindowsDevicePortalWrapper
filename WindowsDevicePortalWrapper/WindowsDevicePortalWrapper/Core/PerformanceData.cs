// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE.TXT in the project root license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal
{
    public partial class DevicePortal
    {
        public static readonly String _runningProcessApi = "api/resourcemanager/processes";
        public static readonly String _systemPerfApi = "api/resourcemanager/systemperf";

        /// <summary>
        /// Gets the collection of processes running on the device.
        /// </summary>
        /// <returns>RunningProcesses object containing the list of running processes.</returns>
        public async Task<RunningProcesses> GetRunningProcesses()
        {
            return await Get<RunningProcesses>(_runningProcessApi);
        }

        /// <summary>
        /// Gets system performance information for the device.
        /// </summary>
        /// <returns>SystemPerformanceInformation object containing information such as memory usage.</returns>
        public async Task<SystemPerformanceInformation> GetSystemPerf()
        {
            return await Get<SystemPerformanceInformation>(_systemPerfApi);
        }
    }

#region Device contract

    [DataContract]
    public class DeviceProcessInfo
    {
        [DataMember(Name="AppName")]
        public String AppName { get; set; }

        [DataMember(Name="CPUUsage")]    
        public Single CpuUsage { get; set; }

        [DataMember(Name="ImageName")]
        public String Name { get; set; }

        [DataMember(Name="ProcessId")]
        public Int32 ProcessId { get; set; }

        [DataMember(Name="UserName")]
        public String UserName { get; set; }

        [DataMember(Name="PackageFullName")]
        public String PackageFullName { get; set; }

        [DataMember(Name="PageFileUsage")]
        public UInt32 PageFile { get; set; }

        [DataMember(Name="WorkingSetSize")]
        public UInt32 WorkingSet { get; set; }

        public override string ToString()
        {
            return String.Format("{0} ({1})", AppName, Name);
        }
    }

    [DataContract]
    public class GpuAdapters
    {
        [DataMember(Name="DedicatedMemory")]
        public UInt32 DedicatedMemory { get; set; }

        [DataMember(Name="DedicatedMemoryUsed")]
        public UInt32 DedicatedMemoryUsed { get; set; }

        [DataMember(Name="Description")]
        public String Description { get; set; }

        [DataMember(Name="SystemMemory")]
        public UInt32 SystemMemory { get; set; }

        [DataMember(Name="SystemMemoryUsed")]
        public UInt32 SystemMemoryUsed { get; set; }

        [DataMember(Name="EnginesUtilization")]
        public List<Single> EnginesUtilization { get; set; }
    }

    [DataContract]
    public class GpuPerformanceData
    {
        [DataMember(Name="AvailableAdapters")]
        public List<GpuAdapters> Adapters { get; set; }
    }

    [DataContract]
    public class NetworkPerformanceData
    {
        [DataMember(Name="NetworkInBytes")]
        public Int32 BytesIn { get; set; }

        [DataMember(Name="NetworkOutBytes")]
        public Int32 BytesOut { get; set; }
    }

    [DataContract]
    public class RunningProcesses
    {
        [DataMember(Name="Processes")]
        public DeviceProcessInfo[] Processes { get; set; }

        public Boolean Contains(Int32 processId)
        {
            Boolean found = false;

            if (Processes != null)
            {
                foreach (DeviceProcessInfo pi in Processes)
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

        public Boolean Contains(String packageName, Boolean caseSensitive=true)
        {
            Boolean found = false;

            if (Processes != null)
            {
                foreach (DeviceProcessInfo pi in Processes)
                {
                    if (0 == String.Compare(pi.PackageFullName, 
                                            packageName, 
                                            caseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase))
                    {
                        found = true;
                        break;
                    }
                }
            }

            return found;
        }
    }

    [DataContract]
    public class SystemPerformanceInformation
    {
        [DataMember(Name="AvailablePages")]
        public Int32 AvailablePages { get; set; }

        [DataMember(Name="CommitLimit")]
        public Int32 CommitLimit { get; set; }

        [DataMember(Name="CommittedPages")]
        public Int32 CommittedPages { get; set; }

        [DataMember(Name="CpuLoad")]
        public Int32 CpuLoad { get; set; }

        [DataMember(Name="IOOtherSpeed")]
        public Int32 IoOtherSpeed { get; set; }

        [DataMember(Name="IOReadSpeed")]
        public Int32 IoReadSpeed { get; set; }

        [DataMember(Name="IOWriteSpeed")]
        public Int32 IoWriteSpeed { get; set; }

        [DataMember(Name="NonPagedPoolPages")]
        public Int32 NonPagedPoolPages { get; set; }

        [DataMember(Name="PageSize")]
        public Int32 PageSize { get; set; }

        [DataMember(Name="PagedPoolPages")]
        public Int32 PagedPoolPages { get; set; }

        [DataMember(Name="TotalInstalledInKb")]
        public Int32 TotalInstalledKb { get; set; }

        [DataMember(Name="TotalPages")]
        public Int32 TotalPages { get; set; }

        [DataMember(Name="GPUData")]
        public GpuPerformanceData GpuData { get; set; }

        [DataMember(Name="NetworkingData")]
        public NetworkPerformanceData NetworkData { get; set; }
    }

#endregion // Device contract
}
