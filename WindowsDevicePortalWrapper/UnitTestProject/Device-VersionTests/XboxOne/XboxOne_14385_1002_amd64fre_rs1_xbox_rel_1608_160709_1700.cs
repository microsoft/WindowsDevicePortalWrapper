//----------------------------------------------------------------------------------------------
// <copyright file="XboxOne_14385_1002_amd64fre_rs1_xbox_rel_1608_160709_1700.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Microsoft.Tools.WindowsDevicePortal.DevicePortal;

namespace Microsoft.Tools.WindowsDevicePortal.Tests
{
    /// <summary>
    /// Test class for XboxOne_14385_1002_amd64fre_rs1_xbox_rel_1608_160709_1700 version
    /// </summary>
    [TestClass]
    public class XboxOne_14385_1002_amd64fre_rs1_xbox_rel_1608_160709_1700 : BaseTests
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
            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.XboxLiveUserApi, this.PlatformType, this.OperatingSystemVersion);

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
        /// Basic test of GET for operating system info for 1508 OS.
        /// </summary>
        [TestMethod]
        public void GetOsInfo_XboxOne_1608()
        {
            XboxHelpers.VerifyOsInformation(this.OperatingSystemVersion);
        }

        /// <summary>
        /// Basic test of the GET method. Gets a mock list of settings
        /// and verifies it comes back as expected from the raw response
        /// content for this 1508 OS.
        /// </summary>
        [TestMethod]
        public void GetXboxSettingsTest_XboxOne_1608()
        {
            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.XboxSettingsApi, this.PlatformType, this.OperatingSystemVersion);

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
            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.KnownFoldersApi, this.PlatformType, this.OperatingSystemVersion);

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
            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.GetFilesApi, this.PlatformType, this.OperatingSystemVersion);

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
    }
}
