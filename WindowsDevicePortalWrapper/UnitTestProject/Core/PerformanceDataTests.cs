//----------------------------------------------------------------------------------------------
// <copyright file="PerformanceDataTests.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Microsoft.Tools.WindowsDevicePortal.DevicePortal;

namespace Microsoft.Tools.WindowsDevicePortal.Tests.Core
{
    /// <summary>
    /// Test class for PerformanceData APIs.
    /// </summary>
    [TestClass]
    public class PerformanceDataTests : BaseTests
    {
        /// <summary>
        /// Basic test of GET method for getting a list of running processes.
        /// </summary>
        [TestMethod]
        public void GetRunningProcessesTest()
        {
            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.RunningProcessApi, HttpMethods.Get);

            Task<RunningProcesses> getRunningProcessesTask = TestHelpers.Portal.GetRunningProcesses();
            getRunningProcessesTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, getRunningProcessesTask.Status);

            ValidateRunningProcesses(getRunningProcessesTask.Result);
        }

        /// <summary>
        /// Basic test of web socket connection for getting a list of running processes.
        /// </summary>
        [TestMethod]
        public void GetRunningProcessesWebSocketTest()
        {
            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.RunningProcessApi, HttpMethods.WebSocket);

            ManualResetEvent runningProcessesReceived = new ManualResetEvent(false);
            RunningProcesses runningProcesses = null;

            WindowsDevicePortal.WebSocketMessageReceivedEventHandler<RunningProcesses> runningProcessesReceivedHandler = delegate(DevicePortal sender,
            WebSocketMessageReceivedEventArgs<RunningProcesses> args)
            {
                if (args.Message != null)
                {
                    runningProcesses = args.Message;
                    runningProcessesReceived.Set();
                }
            };

            TestHelpers.Portal.RunningProcessesMessageReceived += runningProcessesReceivedHandler;

            Task startListeningForProcessesTask = TestHelpers.Portal.StartListeningForRunningProcesses();
            startListeningForProcessesTask.Wait();
            Assert.AreEqual(TaskStatus.RanToCompletion, startListeningForProcessesTask.Status);

            runningProcessesReceived.WaitOne();

            Task stopListeningForProcessesTask = TestHelpers.Portal.StopListeningForRunningProcesses();
            stopListeningForProcessesTask.Wait();
            Assert.AreEqual(TaskStatus.RanToCompletion, stopListeningForProcessesTask.Status);

            TestHelpers.Portal.RunningProcessesMessageReceived -= runningProcessesReceivedHandler;

            ValidateRunningProcesses(runningProcesses);
        }

        /// <summary>
        /// Basic test of GET method for getting system perf without the known JSON error.
        /// </summary>
        [TestMethod]
        public void GetSystemPerfTest()
        {
            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.SystemPerfApi, HttpMethods.Get);

            Task<SystemPerformanceInformation> getSystemPerfTask = TestHelpers.Portal.GetSystemPerf();
            getSystemPerfTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, getSystemPerfTask.Status);

            ValidateSystemPerm(getSystemPerfTask.Result);
        }

        /// <summary>
        /// Basic test of web socket for getting system perf.
        /// </summary>
        [TestMethod]
        public void GetSystemPerfWebSocketTest()
        {
            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.SystemPerfApi, HttpMethods.WebSocket);

            ManualResetEvent systemPerfReceived = new ManualResetEvent(false);
            SystemPerformanceInformation systemPerfInfo = null;

            WebSocketMessageReceivedEventHandler<SystemPerformanceInformation> systemPerfReceivedHandler = delegate(DevicePortal sender,
            WebSocketMessageReceivedEventArgs<SystemPerformanceInformation> args)
            {
                if (args.Message != null)
                {
                    systemPerfInfo = args.Message;
                    systemPerfReceived.Set();
                }
            };

            TestHelpers.Portal.SystemPerfMessageReceived += systemPerfReceivedHandler;

            Task startListeningForSystemPerfTask = TestHelpers.Portal.StartListeningForSystemPerf();
            startListeningForSystemPerfTask.Wait();
            Assert.AreEqual(TaskStatus.RanToCompletion, startListeningForSystemPerfTask.Status);

            systemPerfReceived.WaitOne();

            Task stopListeningForSystemPerf = TestHelpers.Portal.StopListeningForSystemPerf();
            stopListeningForSystemPerf.Wait();
            Assert.AreEqual(TaskStatus.RanToCompletion, stopListeningForSystemPerf.Status);

            TestHelpers.Portal.SystemPerfMessageReceived -= systemPerfReceivedHandler;

            ValidateSystemPerm(systemPerfInfo);
        }

        /// <summary>
        /// Validate the <see cref="RunningProcesses" /> returned from the tests.
        /// </summary>
        /// <param name="runningProcesses">The <see cref="RunningProcesses" /> to validate.</param>
        private static void ValidateRunningProcesses(RunningProcesses runningProcesses)
        {
            List<DeviceProcessInfo> processes = new List<DeviceProcessInfo>(runningProcesses.Processes);

            // Check some known things about this response.
            Assert.AreEqual(2, processes.Count);

            DeviceProcessInfo systemIdleprocess = processes[0];
            Assert.IsNull(systemIdleprocess.AppName);
            Assert.AreEqual(systemIdleprocess.CpuUsage, 0);
            Assert.IsFalse(systemIdleprocess.IsRunning);
            Assert.IsFalse(systemIdleprocess.IsXAP);
            Assert.AreEqual(systemIdleprocess.Name, "System Idle Process");
            Assert.IsNull(systemIdleprocess.PackageFullName);
            Assert.AreEqual(systemIdleprocess.PageFile, 0U);
            Assert.AreEqual(systemIdleprocess.PrivateWorkingSet, 4096);
            Assert.AreEqual(systemIdleprocess.ProcessId, 0);
            Assert.IsNull(systemIdleprocess.Publisher);
            Assert.AreEqual(systemIdleprocess.SessionId, 0U);
            Assert.AreEqual(systemIdleprocess.TotalCommit, 0);
            Assert.AreEqual(systemIdleprocess.UserName, "NT AUTHORITY\\SYSTEM");
            Assert.IsNull(systemIdleprocess.Version);
            Assert.AreEqual(systemIdleprocess.VirtualSize, 65536);
            Assert.AreEqual(systemIdleprocess.WorkingSet, 4096U);

            DeviceProcessInfo devHomeProcess = processes[1];
            Assert.IsNull(devHomeProcess.AppName);
            Assert.AreEqual(devHomeProcess.CpuUsage, 0);
            Assert.IsFalse(devHomeProcess.IsRunning);
            Assert.IsFalse(devHomeProcess.IsXAP);
            Assert.AreEqual(devHomeProcess.Name, "svchost.exe");
            Assert.IsNull(devHomeProcess.PackageFullName);
            Assert.AreEqual(devHomeProcess.PageFile, 5472256U);
            Assert.AreEqual(devHomeProcess.PrivateWorkingSet, 4755456);
            Assert.AreEqual(devHomeProcess.ProcessId, 892);
            Assert.IsNull(devHomeProcess.Publisher);
            Assert.AreEqual(devHomeProcess.SessionId, 0U);
            Assert.AreEqual(devHomeProcess.TotalCommit, 5914624);
            Assert.AreEqual(devHomeProcess.UserName, "NT AUTHORITY\\SYSTEM");
            Assert.IsNull(devHomeProcess.Version);
            Assert.AreEqual(devHomeProcess.VirtualSize, 2203387539456);
            Assert.AreEqual(devHomeProcess.WorkingSet, 17285120U);
        }

        /// <summary>
        /// Validate the <see cref="SystemPerformanceInformation" /> returned from the tests.
        /// </summary>
        /// <param name="systemPerfInfo">The <see cref="SystemPerformanceInformation" /> to validate.</param>
        private static void ValidateSystemPerm(SystemPerformanceInformation systemPerfInfo)
        {
            // Check some known things about this response.
            Assert.AreEqual(systemPerfInfo.AvailablePages, 369054);
            Assert.AreEqual(systemPerfInfo.CommitLimit, 784851);
            Assert.AreEqual(systemPerfInfo.CommittedPages, 322627);
            Assert.AreEqual(systemPerfInfo.CpuLoad, 1);
            Assert.AreEqual(systemPerfInfo.IoOtherSpeed, 3692);
            Assert.AreEqual(systemPerfInfo.IoReadSpeed, 36);
            Assert.AreEqual(systemPerfInfo.IoWriteSpeed, 6480);
            Assert.AreEqual(systemPerfInfo.NonPagedPoolPages, 42504);
            Assert.AreEqual(systemPerfInfo.PageSize, 4096);
            Assert.AreEqual(systemPerfInfo.PagedPoolPages, 30697);
            Assert.AreEqual(systemPerfInfo.TotalInstalledKb, 1048592);
            Assert.AreEqual(systemPerfInfo.TotalPages, 655360);

            Assert.AreEqual(systemPerfInfo.GpuData.Adapters.Count, 1);
            GpuAdapter gpuAdapter = systemPerfInfo.GpuData.Adapters[0];
            Assert.AreEqual(gpuAdapter.DedicatedMemory, 268435456U);
            Assert.AreEqual(gpuAdapter.DedicatedMemoryUsed, 79282176U);
            Assert.AreEqual(gpuAdapter.Description, "ROOT\\SraKmd\\0000");
            Assert.AreEqual(gpuAdapter.SystemMemory, 1342177280U);
            Assert.AreEqual(gpuAdapter.SystemMemoryUsed, 10203136U);

            Assert.AreEqual(gpuAdapter.EnginesUtilization.Count, 7);
            double enguineUtilization = gpuAdapter.EnginesUtilization[0];
            Assert.AreEqual(enguineUtilization, 0.0011459999950602651);

            NetworkPerformanceData networkPerformanceData = systemPerfInfo.NetworkData;
            Assert.AreEqual(networkPerformanceData.BytesIn, 15000);
            Assert.AreEqual(networkPerformanceData.BytesOut, 0);
        }
    }
}
