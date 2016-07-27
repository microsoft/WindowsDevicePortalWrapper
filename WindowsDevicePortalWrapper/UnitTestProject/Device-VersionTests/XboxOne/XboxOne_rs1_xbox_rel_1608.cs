//----------------------------------------------------------------------------------------------
// <copyright file="XboxOne_rs1_xbox_rel_1608.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Microsoft.Tools.WindowsDevicePortal.DevicePortal;

namespace Microsoft.Tools.WindowsDevicePortal.Tests
{
    /// <summary>
    /// Test class for XboxOne_rs1_xbox_rel_1608 version
    /// </summary>
    [TestClass]
    public class XboxOne_rs1_xbox_rel_1608 : BaseTests
    {
        /// <summary>
        /// Gets the Platform type these tests are targeting.
        /// </summary>
        protected override DevicePortalPlatforms PlatformType
        {
            get
            {
                return DevicePortalPlatforms.XboxOne;
            }
        }

        /// <summary>
        /// Gets the friendly OS Version these tests are targeting.
        /// </summary>
        protected override string FriendlyOperatingSystemVersion
        {
            get
            {
                return "rs1_xbox_rel_1608";
            }
        }

        /// <summary>
        /// Gets the OS Version these tests are targeting.
        /// </summary>
        protected override string OperatingSystemVersion
        {
            get
            {
                return "14385_1002_amd64fre_rs1_xbox_rel_1608_160709_1700";
            }
        }

        /// <summary>
        /// Gets a mock list of users and verifies it comes back as 
        /// expected from the raw response content using a mock generated
        /// on an Xbox One running the 1608 OS recovery.
        /// </summary>
        [TestMethod]
        public void GetXboxLiveUserListTest_XboxOne_1608()
        {
            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.XboxLiveUserApi, this.PlatformType, this.FriendlyOperatingSystemVersion, HttpMethods.Get);

            Task<UserList> getUserTask = TestHelpers.Portal.GetXboxLiveUsers();
            getUserTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, getUserTask.Status);

            List<UserInfo> users = getUserTask.Result.Users;

            // Check some known things about this response.
            Assert.AreEqual(1, users.Count);

            Assert.AreEqual("TestUser@XboxTestDomain.com", users[0].EmailAddress);
            Assert.AreEqual("TestGamertag", users[0].Gamertag);
            Assert.AreEqual(20u, users[0].UserId);
            Assert.AreEqual("1234567890123456", users[0].XboxUserId);
            Assert.AreEqual(false, users[0].SignedIn);
            Assert.AreEqual(false, users[0].AutoSignIn);
        }

        /// <summary>
        /// Basic test of GET for operating system info for 1608 OS.
        /// </summary>
        [TestMethod]
        public void GetOsInfo_XboxOne_1608()
        {
            XboxHelpers.VerifyOsInformation(this.FriendlyOperatingSystemVersion, this.OperatingSystemVersion);
        }

        /// <summary>
        /// Basic test of the GET method. Gets a mock list of settings
        /// and verifies it comes back as expected from the raw response
        /// content for this 1608 OS.
        /// </summary>
        [TestMethod]
        public void GetXboxSettingsTest_XboxOne_1608()
        {
            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.XboxSettingsApi, this.PlatformType, this.FriendlyOperatingSystemVersion, HttpMethods.Get);

            Task<XboxSettingList> getSettingsTask = TestHelpers.Portal.GetXboxSettings();
            getSettingsTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, getSettingsTask.Status);

            List<XboxSetting> settings = getSettingsTask.Result.Settings;

            // Check some known things about this response.
            Assert.AreEqual(8, settings.Count);

            Assert.AreEqual("AudioBitstreamFormat", settings[0].Name);
            Assert.AreEqual("DTS", settings[0].Value);
            Assert.AreEqual("Audio", settings[0].Category);
            Assert.AreEqual("No", settings[0].RequiresReboot);

            Assert.AreEqual("ColorDepth", settings[1].Name);
            Assert.AreEqual("24 bit", settings[1].Value);
            Assert.AreEqual("Video", settings[1].Category);
            Assert.AreEqual("Yes", settings[1].RequiresReboot);

            Assert.AreEqual("TVResolution", settings[7].Name);
            Assert.AreEqual("720p", settings[7].Value);
            Assert.AreEqual("Video", settings[7].Category);
            Assert.AreEqual("No", settings[7].RequiresReboot);
        }

        /// <summary>
        /// Simple test which gets a response with a couple of known folders
        /// and verifies they are returned correctly.
        /// </summary>
        [TestMethod]
        public void AppFileExplorerGetKnownFolderTest_XboxOne_1608()
        {
            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.KnownFoldersApi, this.PlatformType, this.FriendlyOperatingSystemVersion, HttpMethods.Get);

            Task<KnownFolders> getKnownFoldersTask = TestHelpers.Portal.GetKnownFolders();
            getKnownFoldersTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, getKnownFoldersTask.Status);

            List<string> knownFolders = getKnownFoldersTask.Result.Folders;

            // Check some known things about this response.
            Assert.AreEqual(2, knownFolders.Count);
            Assert.AreEqual("DevelopmentFiles", knownFolders[0]);
            Assert.AreEqual("LocalAppData", knownFolders[1]);
        }

        /// <summary>
        /// Tests getting the contents of a folder that is not for
        /// an application (eg developer folder, documents folder).
        /// </summary>
        [TestMethod]
        public void AppFileExplorerGetFolderContentsTest_XboxOne_1608()
        {
            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.GetFilesApi, this.PlatformType, this.FriendlyOperatingSystemVersion, HttpMethods.Get);

            Task<FolderContents> getFolderContentsTask = TestHelpers.Portal.GetFolderContents("DevelopmentFiles");
            getFolderContentsTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, getFolderContentsTask.Status);

            List<FileOrFolderInformation> directoryContents = getFolderContentsTask.Result.Contents;

            // Check some known things about this response.
            Assert.AreEqual(9, directoryContents.Count);

            int currentFileIndex = 0;

            Assert.AreEqual(@"LooseApps\LooseFolder", directoryContents[currentFileIndex].CurrentDir);
            Assert.AreEqual(131117780895076062, directoryContents[currentFileIndex].DateCreated);
            Assert.AreEqual(2985, directoryContents[currentFileIndex].SizeInBytes);
            Assert.AreEqual("AppxManifest.xml", directoryContents[currentFileIndex].Id);
            Assert.AreEqual("AppxManifest.xml", directoryContents[currentFileIndex].Name);
            Assert.AreEqual(@"LooseApps\LooseFolder", directoryContents[currentFileIndex].SubPath);
            Assert.AreEqual(32, directoryContents[currentFileIndex].Type);
            currentFileIndex++;

            Assert.AreEqual(@"LooseApps\LooseFolder", directoryContents[currentFileIndex].CurrentDir);
            Assert.AreEqual(131117780895226350, directoryContents[currentFileIndex].DateCreated);
            Assert.AreEqual(1952, directoryContents[currentFileIndex].SizeInBytes);
            Assert.AreEqual("resources.pri", directoryContents[currentFileIndex].Id);
            Assert.AreEqual("resources.pri", directoryContents[currentFileIndex].Name);
            Assert.AreEqual(@"LooseApps\LooseFolder", directoryContents[currentFileIndex].SubPath);
            Assert.AreEqual(32, directoryContents[currentFileIndex].Type);
            currentFileIndex++;

            Assert.AreEqual(@"LooseApps\LooseFolder", directoryContents[currentFileIndex].CurrentDir);
            Assert.AreEqual(131117780895466081, directoryContents[currentFileIndex].DateCreated);
            Assert.AreEqual(14900, directoryContents[currentFileIndex].SizeInBytes);
            Assert.AreEqual("SamplePixelShader.cso", directoryContents[currentFileIndex].Id);
            Assert.AreEqual("SamplePixelShader.cso", directoryContents[currentFileIndex].Name);
            Assert.AreEqual(@"LooseApps\LooseFolder", directoryContents[currentFileIndex].SubPath);
            Assert.AreEqual(32, directoryContents[currentFileIndex].Type);
            currentFileIndex++;

            Assert.AreEqual(@"LooseApps\LooseFolder", directoryContents[currentFileIndex].CurrentDir);
            Assert.AreEqual(131117780895599579, directoryContents[currentFileIndex].DateCreated);
            Assert.AreEqual(18444, directoryContents[currentFileIndex].SizeInBytes);
            Assert.AreEqual("SampleVertexShader.cso", directoryContents[currentFileIndex].Id);
            Assert.AreEqual("SampleVertexShader.cso", directoryContents[currentFileIndex].Name);
            Assert.AreEqual(@"LooseApps\LooseFolder", directoryContents[currentFileIndex].SubPath);
            Assert.AreEqual(32, directoryContents[currentFileIndex].Type);
            currentFileIndex++;

            Assert.AreEqual(@"LooseApps\LooseFolder", directoryContents[currentFileIndex].CurrentDir);
            Assert.AreEqual(131117780895735331, directoryContents[currentFileIndex].DateCreated);
            Assert.AreEqual(1234944, directoryContents[currentFileIndex].SizeInBytes);
            Assert.AreEqual("UWP_StorageTest.exe", directoryContents[currentFileIndex].Id);
            Assert.AreEqual("UWP_StorageTest.exe", directoryContents[currentFileIndex].Name);
            Assert.AreEqual(@"LooseApps\LooseFolder", directoryContents[currentFileIndex].SubPath);
            Assert.AreEqual(32, directoryContents[currentFileIndex].Type);
            currentFileIndex++;

            Assert.AreEqual(@"LooseApps\LooseFolder", directoryContents[currentFileIndex].CurrentDir);
            Assert.AreEqual(131117780896423174, directoryContents[currentFileIndex].DateCreated);
            Assert.AreEqual(4149884, directoryContents[currentFileIndex].SizeInBytes);
            Assert.AreEqual("UWP_StorageTest.ilk", directoryContents[currentFileIndex].Id);
            Assert.AreEqual("UWP_StorageTest.ilk", directoryContents[currentFileIndex].Name);
            Assert.AreEqual(@"LooseApps\LooseFolder", directoryContents[currentFileIndex].SubPath);
            Assert.AreEqual(32, directoryContents[currentFileIndex].Type);
            currentFileIndex++;

            Assert.AreEqual(@"LooseApps\LooseFolder", directoryContents[currentFileIndex].CurrentDir);
            Assert.AreEqual(131117780897192161, directoryContents[currentFileIndex].DateCreated);
            Assert.AreEqual(4919296, directoryContents[currentFileIndex].SizeInBytes);
            Assert.AreEqual("UWP_StorageTest.pdb", directoryContents[currentFileIndex].Id);
            Assert.AreEqual("UWP_StorageTest.pdb", directoryContents[currentFileIndex].Name);
            Assert.AreEqual(@"LooseApps\LooseFolder", directoryContents[currentFileIndex].SubPath);
            Assert.AreEqual(32, directoryContents[currentFileIndex].Type);
            currentFileIndex++;

            Assert.AreEqual(@"LooseApps\LooseFolder", directoryContents[currentFileIndex].CurrentDir);
            Assert.AreEqual(131117780898018399, directoryContents[currentFileIndex].DateCreated);
            Assert.AreEqual(1536, directoryContents[currentFileIndex].SizeInBytes);
            Assert.AreEqual("UWP_StorageTest.winmd", directoryContents[currentFileIndex].Id);
            Assert.AreEqual("UWP_StorageTest.winmd", directoryContents[currentFileIndex].Name);
            Assert.AreEqual(@"LooseApps\LooseFolder", directoryContents[currentFileIndex].SubPath);
            Assert.AreEqual(32, directoryContents[currentFileIndex].Type);
            currentFileIndex++;

            Assert.AreEqual(@"LooseApps\LooseFolder", directoryContents[currentFileIndex].CurrentDir);
            Assert.AreEqual(131117780894117950, directoryContents[currentFileIndex].DateCreated);
            Assert.AreEqual(0, directoryContents[currentFileIndex].SizeInBytes);
            Assert.AreEqual("SubFolder", directoryContents[currentFileIndex].Id);
            Assert.AreEqual("SubFolder", directoryContents[currentFileIndex].Name);
            Assert.AreEqual(@"LooseApps\LooseFolder\SubFolder", directoryContents[currentFileIndex].SubPath);
            Assert.AreEqual(16, directoryContents[currentFileIndex].Type);
            currentFileIndex++;
        }

        /// <summary>
        /// Basic test of GET method. Gets a mock list of processes and verifies it comes 
        /// back as expected from the raw response content for this 1608 OS.
        /// </summary>
        [TestMethod]
        public void GetRunningProcessesTest_XboxOne_1608()
        {
            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.RunningProcessApi, this.PlatformType, this.FriendlyOperatingSystemVersion, HttpMethods.Get);

            Task<RunningProcesses> getRunningProcessesTask = TestHelpers.Portal.GetRunningProcesses();
            getRunningProcessesTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, getRunningProcessesTask.Status);

            ValidateRunningProcesses(getRunningProcessesTask.Result);
        }

        /// <summary>
        /// Basic test of web socket. Gets a mock list of processes and verifies it comes back as expected 
        /// from the raw response content for this 1608 OS.
        /// </summary>
        [TestMethod]
        public void GetRunningProcessesWebSocketTest_XboxOne_1608()
        {
            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.RunningProcessApi, this.PlatformType, this.FriendlyOperatingSystemVersion, HttpMethods.WebSocket);

            ManualResetEvent runningProcessesReceived = new ManualResetEvent(false);
            RunningProcesses runningProcesses = null;

            WindowsDevicePortal.WebSocketMessageReceivedEventHandler<RunningProcesses> runningProcessesReceivedHandler = delegate(object sender,
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
        /// Basic test of GET method. Gets a mock list of processes and verifies it comes back as expected from
        /// the raw response content for this 1608 OS.
        /// This response contains the known system perf JSON error.
        /// </summary>
        [TestMethod]
        public void GetSystemPerfTest_XboxOne_1608()
        {
            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.SystemPerfApi, this.PlatformType, this.FriendlyOperatingSystemVersion, HttpMethods.Get);

            Task<SystemPerformanceInformation> getSystemPerfTask = TestHelpers.Portal.GetSystemPerf();
            getSystemPerfTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, getSystemPerfTask.Status);

            ValidateSystemPerm(getSystemPerfTask.Result);
        }

        /// <summary>
        /// Basic test of web socket. Gets a mock list of processes and verifies it comes back as expected from
        /// the raw response content for this 1608 OS.
        /// </summary>
        [TestMethod]
        public void GetSystemPerfWebSocketTest_XboxOne_1608()
        {
            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.SystemPerfApi, this.PlatformType, this.FriendlyOperatingSystemVersion, HttpMethods.WebSocket);

            ManualResetEvent systemPerfReceived = new ManualResetEvent(false);
            SystemPerformanceInformation systemPerfInfo = null;

            WindowsDevicePortal.WebSocketMessageReceivedEventHandler<SystemPerformanceInformation> systemPerfReceivedHandler = delegate(object sender,
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
            Assert.AreEqual(75, processes.Count);

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

            DeviceProcessInfo devHomeProcess = processes[56];
            Assert.AreEqual(devHomeProcess.AppName, "Dev Home");
            Assert.AreEqual(devHomeProcess.CpuUsage, 0);
            Assert.IsFalse(devHomeProcess.IsRunning);
            Assert.IsFalse(devHomeProcess.IsXAP);
            Assert.AreEqual(devHomeProcess.Name, "WWAHost.exe");
            Assert.AreEqual(devHomeProcess.PackageFullName, "Microsoft.Xbox.DevHome_100.1607.9000.0_x64__8wekyb3d8bbwe");
            Assert.AreEqual(devHomeProcess.PageFile, 47067136U);
            Assert.AreEqual(devHomeProcess.PrivateWorkingSet, 32796672);
            Assert.AreEqual(devHomeProcess.ProcessId, 3424);
            Assert.AreEqual(devHomeProcess.Publisher, "CN=Microsoft Corporation, O=Microsoft Corporation, L=Redmond, S=Washington, C=US");
            Assert.AreEqual(devHomeProcess.SessionId, 0U);
            Assert.AreEqual(devHomeProcess.TotalCommit, 49213440);

            Assert.AreEqual(devHomeProcess.UserName, "TESTXBOX\\DefaultAccount");
            Assert.AreEqual(devHomeProcess.Version.Build, 9000U);
            Assert.AreEqual(devHomeProcess.Version.Major, 100U);
            Assert.AreEqual(devHomeProcess.Version.Minor, 1607U);
            Assert.AreEqual(devHomeProcess.Version.Revision, 0U);
            Assert.AreEqual(devHomeProcess.VirtualSize, 2234032066560);
            Assert.AreEqual(devHomeProcess.WorkingSet, 79466496U);
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