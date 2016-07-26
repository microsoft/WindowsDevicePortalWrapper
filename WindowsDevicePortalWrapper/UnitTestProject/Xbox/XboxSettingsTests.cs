//----------------------------------------------------------------------------------------------
// <copyright file="XboxSettingsTests.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Microsoft.Tools.WindowsDevicePortal.DevicePortal;

namespace Microsoft.Tools.WindowsDevicePortal.Tests
{
    /// <summary>
    /// Test class for Xbox Settings APIs
    /// </summary>
    [TestClass]
    public class XboxSettingsTests : BaseTests
    {
        /// <summary>
        /// Basic test of the GET method. Gets a mock list of settings
        /// and verifies it comes back as expected from the raw response
        /// content.
        /// </summary>
        [TestMethod]
        public void GetXboxSettingsTest()
        {
            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.XboxSettingsApi, HttpMethods.Get);

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
        /// Basic test of the GET method when called for a
        /// single setting.
        /// </summary>
        [TestMethod]
        public void GetSingleXboxSettingTest()
        {
            string settingName = "TVResolution";
            TestHelpers.MockHttpResponder.AddMockResponse(Path.Combine(DevicePortal.XboxSettingsApi, settingName), HttpMethods.Get);

            Task<XboxSetting> getSettingTask = TestHelpers.Portal.GetXboxSetting(settingName);
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

            Task<XboxSetting> updateSettingsTask = TestHelpers.Portal.UpdateXboxSetting(setting);
            updateSettingsTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, updateSettingsTask.Status);

            XboxSetting recievedSetting = updateSettingsTask.Result;

            // Check some known things about this response.
            Assert.AreEqual(setting.Name, recievedSetting.Name);
            
            Assert.AreEqual("1080p", recievedSetting.Value);
            Assert.AreEqual("Video", recievedSetting.Category);
            Assert.AreEqual("No", recievedSetting.RequiresReboot);
        }
    }
}
