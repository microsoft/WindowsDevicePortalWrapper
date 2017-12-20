//----------------------------------------------------------------------------------------------
// <copyright file="XboxOne_rs1_xbox_rel_1608.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
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
                return "14385.1002.amd64fre.rs1_xbox_rel_1608.160709-1700";
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

            Task<UserList> getUserTask = TestHelpers.Portal.GetXboxLiveUsersAsync();
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

            Task<XboxSettingList> getSettingsTask = TestHelpers.Portal.GetXboxSettingsAsync();
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

            Task<KnownFolders> getKnownFoldersTask = TestHelpers.Portal.GetKnownFoldersAsync();
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

            Task<FolderContents> getFolderContentsTask = TestHelpers.Portal.GetFolderContentsAsync("DevelopmentFiles");
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

            Task<RunningProcesses> getRunningProcessesTask = TestHelpers.Portal.GetRunningProcessesAsync();
            getRunningProcessesTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, getRunningProcessesTask.Status);

            ValidateRunningProcessesAsync(getRunningProcessesTask.Result);
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

            Task startListeningForProcessesTask = TestHelpers.Portal.StartListeningForRunningProcessesAsync();
            startListeningForProcessesTask.Wait();
            Assert.AreEqual(TaskStatus.RanToCompletion, startListeningForProcessesTask.Status);

            runningProcessesReceived.WaitOne();

            Task stopListeningForProcessesTask = TestHelpers.Portal.StopListeningForRunningProcessesAsync();
            stopListeningForProcessesTask.Wait();
            Assert.AreEqual(TaskStatus.RanToCompletion, stopListeningForProcessesTask.Status);

            TestHelpers.Portal.RunningProcessesMessageReceived -= runningProcessesReceivedHandler;

            ValidateRunningProcessesAsync(runningProcesses);
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

            Task<SystemPerformanceInformation> getSystemPerfTask = TestHelpers.Portal.GetSystemPerfAsync();
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

            WindowsDevicePortal.WebSocketMessageReceivedEventHandler<SystemPerformanceInformation> systemPerfReceivedHandler = delegate(DevicePortal sender,
            WebSocketMessageReceivedEventArgs<SystemPerformanceInformation> args)
            {
                if (args.Message != null)
                {
                    systemPerfInfo = args.Message;
                    systemPerfReceived.Set();
                }
            };

            TestHelpers.Portal.SystemPerfMessageReceived += systemPerfReceivedHandler;

            Task startListeningForSystemPerfTask = TestHelpers.Portal.StartListeningForSystemPerfAsync();
            startListeningForSystemPerfTask.Wait();
            Assert.AreEqual(TaskStatus.RanToCompletion, startListeningForSystemPerfTask.Status);

            systemPerfReceived.WaitOne();

            Task stopListeningForSystemPerf = TestHelpers.Portal.StopListeningForSystemPerfAsync();
            stopListeningForSystemPerf.Wait();
            Assert.AreEqual(TaskStatus.RanToCompletion, stopListeningForSystemPerf.Status);

            TestHelpers.Portal.SystemPerfMessageReceived -= systemPerfReceivedHandler;

            ValidateSystemPerm(systemPerfInfo);
        }

        /// <summary>
        /// Basic test of the register API.
        /// </summary>
        [TestMethod]
        public void XboxAppRegisterTest()
        {
            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.RegisterPackageApi, HttpMethods.Post);
            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.InstallStateApi, HttpMethods.Post);

            Task registerTask = TestHelpers.Portal.RegisterApplicationAsync("SomeLooseFolder");
            registerTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, registerTask.Status);
        }

        /// <summary>
        /// Basic test of the folder upload API.
        /// </summary>
        [TestMethod]
        public void XboxAppUploadFolderTest()
        {
            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.UploadPackageFolderApi, HttpMethods.Post);

            Task uploadTask = TestHelpers.Portal.UploadPackageFolderAsync("MockData\\XboxOne", "DestinationLooseFolder");
            uploadTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, uploadTask.Status);
        }

        /// <summary>
        /// Basic test of the GET method. Gets a mock list of users
        /// and verifies it comes back as expected from the raw response
        /// content.
        /// </summary>
        [TestMethod]
        public void GetXboxLiveUserListTest()
        {
            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.XboxLiveUserApi, HttpMethods.Get);

            Task<UserList> getUserTask = TestHelpers.Portal.GetXboxLiveUsersAsync();
            getUserTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, getUserTask.Status);

            List<UserInfo> users = getUserTask.Result.Users;

            // Check some known things about this response.
            Assert.AreEqual(2, users.Count);

            Assert.AreEqual("fakeMsa@fakedomain.com", users[0].EmailAddress);
            Assert.AreEqual("fakeGamertag", users[0].Gamertag);
            Assert.AreEqual(16u, users[0].UserId);
            Assert.AreEqual("12345667890123456", users[0].XboxUserId);
            Assert.AreEqual(true, users[0].SignedIn);
            Assert.AreEqual(false, users[0].AutoSignIn);

            Assert.IsNull(users[1].EmailAddress);
            Assert.AreEqual("fakeGamertag(1)", users[1].Gamertag);
            Assert.AreEqual(1u, users[1].UserId);
            Assert.AreEqual("7036874539097560", users[1].XboxUserId);
            Assert.AreEqual(true, users[1].SponsoredUser);
            Assert.AreEqual(true, users[1].SignedIn);
        }

        /// <summary>
        /// Basic test of the PUT method. Creates a UserList
        /// object and passes that to the server.
        /// </summary>
        [TestMethod]
        public void UpdateXboxLiveUsersTest()
        {
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.NoContent);
            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.XboxLiveUserApi, response, HttpMethods.Put);

            UserList users = new UserList();
            UserInfo user = new UserInfo();
            user.EmailAddress = "fakeMsa@fakeDomain.com";
            user.Password = "someFakePassword!";
            user.SignedIn = true;
            users.Add(user);

            Task updateUsersTask = TestHelpers.Portal.UpdateXboxLiveUsersAsync(users);
            updateUsersTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, updateUsersTask.Status);
        }

        /// <summary>
        /// Tests the failure case of trying to add a sponsored user
        /// when the maximum number is already on the console.
        /// </summary>
        [TestMethod]
        public void AddSponsoredUserTest_Failure()
        {
            HttpResponseMessage response = new HttpResponseMessage((HttpStatusCode)422);
            HttpContent content = new StringContent(
                "{\"ErrorCode\":-2136866553,\"ErrorMessage\":\"The maximum number of sponsored users is already signed in.\"}",
                System.Text.Encoding.UTF8,
                "application/json");

            response.Content = content;

            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.XboxLiveUserApi, response, HttpMethods.Put);

            UserList users = new UserList();
            UserInfo user = new UserInfo();
            user.SponsoredUser = true;
            users.Add(user);

            try
            {
                Task updateUsersTask = TestHelpers.Portal.UpdateXboxLiveUsersAsync(users);
                updateUsersTask.Wait();

                Assert.Fail("Expected an exception due to mock responder returning failure HRESULT.");
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is AggregateException);
                Assert.IsNotNull(e.InnerException);
                Assert.IsTrue(e.InnerException is DevicePortalException);

                DevicePortalException exception = e.InnerException as DevicePortalException;

                Assert.AreEqual(-2136866553, exception.HResult);
                Assert.AreEqual("The maximum number of sponsored users is already signed in.", exception.Reason);
            }
        }

        /// <summary>
        /// Basic test of the GET method. Gets a mock list of settings
        /// and verifies it comes back as expected from the raw response
        /// content.
        /// </summary>
        [TestMethod]
        public void GetXboxSettingsTest()
        {
            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.XboxSettingsApi, HttpMethods.Get);

            Task<XboxSettingList> getSettingsTask = TestHelpers.Portal.GetXboxSettingsAsync();
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
        /// Basic test of the GET method when called for a
        /// single setting.
        /// </summary>
        [TestMethod]
        public void GetSingleXboxSettingTest()
        {
            string settingName = "TVResolution";
            TestHelpers.MockHttpResponder.AddMockResponse(Path.Combine(DevicePortal.XboxSettingsApi, settingName), HttpMethods.Get);

            Task<XboxSetting> getSettingTask = TestHelpers.Portal.GetXboxSettingAsync(settingName);
            getSettingTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, getSettingTask.Status);

            XboxSetting setting = getSettingTask.Result;

            // Check some known things about this response.
            Assert.AreEqual(settingName, setting.Name);
            Assert.AreEqual("720p", setting.Value);
            Assert.AreEqual("Video", setting.Category);
            Assert.AreEqual("No", setting.RequiresReboot);
        }

        /// <summary>
        /// Basic test of the PUT method. Creates a XboxSettingList
        /// object and passes that to the server.
        /// </summary>
        [TestMethod]
        public void UpdateXboxSettingsTest()
        {
            XboxSetting setting = new XboxSetting();
            setting.Name = "TVResolution";
            setting.Value = "1080p";

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.NoContent);
            TestHelpers.MockHttpResponder.AddMockResponse(Path.Combine(DevicePortal.XboxSettingsApi, setting.Name), HttpMethods.Put);

            Task<XboxSetting> updateSettingsTask = TestHelpers.Portal.UpdateXboxSettingAsync(setting);
            updateSettingsTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, updateSettingsTask.Status);

            XboxSetting recievedSetting = updateSettingsTask.Result;

            // Check some known things about this response.
            Assert.AreEqual(setting.Name, recievedSetting.Name);

            Assert.AreEqual("1080p", recievedSetting.Value);
            Assert.AreEqual("Video", recievedSetting.Category);
            Assert.AreEqual("No", recievedSetting.RequiresReboot);
        }

        /// <summary>
        /// Simple test of Xbox Screenshot API.
        /// </summary>
        [TestMethod]
        public void XboxScreenshotTest()
        {
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StreamContent(new FileStream(Path.Combine("MockData", this.PlatformType.ToString(), this.FriendlyOperatingSystemVersion, "xbox_screenshot.png"), FileMode.Open));

            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.GetXboxScreenshotApi, response, HttpMethods.Get);

            Task<Stream> screenshotTask = TestHelpers.Portal.TakeXboxScreenshotAsync();
            screenshotTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, screenshotTask.Status);
        }

        /// <summary>
        /// Simple test of Xbox Fiddler API.
        /// </summary>
        [TestMethod]
        public void EnableFiddlerTest()
        {
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.FiddlerSetupApi, response, HttpMethods.Post);

            Task fiddlerEnableTask = TestHelpers.Portal.EnableFiddlerTracingAsync("localhost", "8888");
            fiddlerEnableTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, fiddlerEnableTask.Status);

            response = new HttpResponseMessage(HttpStatusCode.OK);
            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.FiddlerSetupApi, response, HttpMethods.Delete);

            Task fiddlerDisableTask = TestHelpers.Portal.DisableFiddlerTracingAsync();
            fiddlerDisableTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, fiddlerDisableTask.Status);
        }

        /// <summary>
        /// Simple test of Xbox Live Sandbox Get API
        /// </summary>
        [TestMethod]
        public void GetXboxLiveSandboxTest()
        {
            string expectedSandboxValue = "XDKS.1";

            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.XboxLiveSandboxApi, this.PlatformType, this.FriendlyOperatingSystemVersion, HttpMethods.Get);

            Task<Sandbox> getSandboxTask = TestHelpers.Portal.GetXboxLiveSandboxAsync();
            getSandboxTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, getSandboxTask.Status);

            Assert.AreEqual(expectedSandboxValue, getSandboxTask.Result.Value);
        }

        /// <summary>
        /// Simple test of Xbox Live Sandbox Set API
        /// </summary>
        [TestMethod]
        public void SetXboxLiveSandboxTest()
        {
            string sandboxValue = "NewSandboxId";

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(string.Format("{{\"Sandbox\" : \"{0}\"}}", sandboxValue));

            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.XboxLiveSandboxApi, response, HttpMethods.Put);

            Task<Sandbox> setSandboxTask = TestHelpers.Portal.SetXboxLiveSandboxAsync(sandboxValue);
            setSandboxTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, setSandboxTask.Status);

            Assert.AreEqual(sandboxValue, setSandboxTask.Result.Value);
        }

        /// <summary>
        /// Validate the <see cref="RunningProcesses" /> returned from the tests.
        /// </summary>
        /// <param name="runningProcesses">The <see cref="RunningProcesses" /> to validate.</param>
        private static void ValidateRunningProcessesAsync(RunningProcesses runningProcesses)
        {
            List<DeviceProcessInfo> processes = runningProcesses.Processes;

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
            Assert.AreEqual(systemIdleprocess.PrivateWorkingSet, 4096U);
            Assert.AreEqual(systemIdleprocess.ProcessId, 0U);
            Assert.IsNull(systemIdleprocess.Publisher);
            Assert.AreEqual(systemIdleprocess.SessionId, 0U);
            Assert.AreEqual(systemIdleprocess.TotalCommit, 0U);
            Assert.AreEqual(systemIdleprocess.UserName, "NT AUTHORITY\\SYSTEM");
            Assert.IsNull(systemIdleprocess.Version);
            Assert.AreEqual(systemIdleprocess.VirtualSize, 65536U);
            Assert.AreEqual(systemIdleprocess.WorkingSet, 4096U);

            DeviceProcessInfo devHomeProcess = processes[56];
            Assert.AreEqual(devHomeProcess.AppName, "Dev Home");
            Assert.AreEqual(devHomeProcess.CpuUsage, 0);
            Assert.IsFalse(devHomeProcess.IsRunning);
            Assert.IsFalse(devHomeProcess.IsXAP);
            Assert.AreEqual(devHomeProcess.Name, "WWAHost.exe");
            Assert.AreEqual(devHomeProcess.PackageFullName, "Microsoft.Xbox.DevHome_100.1607.9000.0_x64__8wekyb3d8bbwe");
            Assert.AreEqual(devHomeProcess.PageFile, 47067136U);
            Assert.AreEqual(devHomeProcess.PrivateWorkingSet, 32796672U);
            Assert.AreEqual(devHomeProcess.ProcessId, 3424U);
            Assert.AreEqual(devHomeProcess.Publisher, "CN=Microsoft Corporation, O=Microsoft Corporation, L=Redmond, S=Washington, C=US");
            Assert.AreEqual(devHomeProcess.SessionId, 0U);
            Assert.AreEqual(devHomeProcess.TotalCommit, 49213440U);

            Assert.AreEqual(devHomeProcess.UserName, "TESTXBOX\\DefaultAccount");
            Assert.AreEqual(devHomeProcess.Version.Build, 9000U);
            Assert.AreEqual(devHomeProcess.Version.Major, 100U);
            Assert.AreEqual(devHomeProcess.Version.Minor, 1607U);
            Assert.AreEqual(devHomeProcess.Version.Revision, 0U);
            Assert.AreEqual(devHomeProcess.VirtualSize, 2234032066560U);
            Assert.AreEqual(devHomeProcess.WorkingSet, 79466496U);
        }

        /// <summary>
        /// Validate the <see cref="SystemPerformanceInformation" /> returned from the tests.
        /// </summary>
        /// <param name="systemPerfInfo">The <see cref="SystemPerformanceInformation" /> to validate.</param>
        private static void ValidateSystemPerm(SystemPerformanceInformation systemPerfInfo)
        {
            // Check some known things about this response.
            Assert.AreEqual(systemPerfInfo.AvailablePages, 369054U);
            Assert.AreEqual(systemPerfInfo.CommitLimit, 784851U);
            Assert.AreEqual(systemPerfInfo.CommittedPages, 322627U);
            Assert.AreEqual(systemPerfInfo.CpuLoad, 1U);
            Assert.AreEqual(systemPerfInfo.IoOtherSpeed, 3692U);
            Assert.AreEqual(systemPerfInfo.IoReadSpeed, 36U);
            Assert.AreEqual(systemPerfInfo.IoWriteSpeed, 6480U);
            Assert.AreEqual(systemPerfInfo.NonPagedPoolPages, 42504U);
            Assert.AreEqual(systemPerfInfo.PageSize, 4096U);
            Assert.AreEqual(systemPerfInfo.PagedPoolPages, 30697U);
            Assert.AreEqual(systemPerfInfo.TotalInstalledKb, 1048592U);
            Assert.AreEqual(systemPerfInfo.TotalPages, 655360U);

            Assert.AreEqual(systemPerfInfo.GpuData.Adapters.Count, 1);
            GpuAdapter gpuAdapter = systemPerfInfo.GpuData.Adapters[0];
            Assert.AreEqual(gpuAdapter.DedicatedMemory, 268435456U);
            Assert.AreEqual(gpuAdapter.DedicatedMemoryUsed, 79282176U);
            Assert.AreEqual(gpuAdapter.Description, "ROOT\\SraKmd\\0000");
            Assert.AreEqual(gpuAdapter.SystemMemory, 1342177280U);
            Assert.AreEqual(gpuAdapter.SystemMemoryUsed, 10203136U);

            Assert.AreEqual(gpuAdapter.EnginesUtilization.Count, 7);
            double enguineUtilization = gpuAdapter.EnginesUtilization[0];
            Assert.AreEqual(enguineUtilization, 0.001146);

            NetworkPerformanceData networkPerformanceData = systemPerfInfo.NetworkData;
            Assert.AreEqual(networkPerformanceData.BytesIn, 15000U);
            Assert.AreEqual(networkPerformanceData.BytesOut, 0U);
        }
    }
}