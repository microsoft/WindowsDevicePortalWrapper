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
    }
}
