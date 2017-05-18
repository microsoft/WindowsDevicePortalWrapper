//----------------------------------------------------------------------------------------------
// <copyright file="IoT_rs1_release.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Microsoft.Tools.WindowsDevicePortal.DevicePortal;

namespace Microsoft.Tools.WindowsDevicePortal.Tests
{
    /// <summary>
    /// Test class for IoT_rs1_release version
    /// </summary>
    [TestClass]
    public class IoT_rs1_release : BaseTests
    {
        /// <summary>
        /// Gets the Platform type these tests are targeting.
        /// </summary>
        protected override DevicePortalPlatforms PlatformType
        {
            get
            {
                return DevicePortalPlatforms.IoTRaspberryPi3;
            }
        }

        /// <summary>
        /// Gets the friendly OS Version these tests are targeting.
        /// </summary>
        protected override string FriendlyOperatingSystemVersion
        {
            get
            {
                return "rs1_release";
            }
        }

        /// <summary>
        /// Gets the OS Version these tests are targeting.
        /// </summary>
        protected override string OperatingSystemVersion
        {
            get
            {
                return "14393.0.armfre.rs1_release.160715-1616";
            }
        }
        
        /// <summary>
        /// Gets the battery state using the mock data generated on a IoT RasberryPi3.
        /// </summary>
        [TestMethod]
        public void GetBatteryState_IoT()
        {
            TestHelpers.MockHttpResponder.AddMockResponse(
                DevicePortal.BatteryStateApi, 
                this.PlatformType, 
                this.FriendlyOperatingSystemVersion, 
                HttpMethods.Get);

            Task<BatteryState> getTask  = TestHelpers.Portal.GetBatteryStateAsync();
            getTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, getTask.Status);

            // Check some known things about this response.
            BatteryState batteryState = getTask.Result;
            Assert.AreEqual(true, batteryState.IsOnAcPower);
            Assert.AreEqual(false, batteryState.IsBatteryPresent);
            Assert.AreEqual(0U, batteryState.EstimatedTimeRaw);
            Assert.AreEqual(0, batteryState.MaximumCapacity);
            Assert.AreEqual(0, batteryState.RemainingCapacity);
        }

        /// <summary>
        /// Gets the device name using the mock data generated on a IoT device RasberryPi3.
        /// </summary>
        [TestMethod]
        public void GetDeviceName_IoT()
        {
            TestHelpers.MockHttpResponder.AddMockResponse(
                DevicePortal.MachineNameApi, 
                this.PlatformType, 
                this.FriendlyOperatingSystemVersion, 
                HttpMethods.Get);

            Task<string> getTask  = TestHelpers.Portal.GetDeviceNameAsync();
            getTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, getTask.Status);

            // Check some known things about this response.
            Assert.AreEqual("myrpi3", getTask.Result);
        }

        /// <summary>
        /// Gets the IP configuration using the mock data generated on a RasberryPi3.
        /// </summary>
        [TestMethod]
        public void GetIpConfig_IoT()
        {
            TestHelpers.MockHttpResponder.AddMockResponse(
                DevicePortal.IpConfigApi, 
                this.PlatformType, 
                this.FriendlyOperatingSystemVersion, 
                HttpMethods.Get);

            Task<IpConfiguration> getTask  = TestHelpers.Portal.GetIpConfigAsync();
            getTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, getTask.Status);

            IpConfiguration ipconfig = getTask.Result;

            // Check some known things about this response.
            NetworkAdapterInfo adapter = ipconfig.Adapters[0];
            Assert.AreEqual("Bluetooth Device (Personal Area Network)", adapter.Description);
            Assert.AreEqual(4, adapter.Index);
            IpAddressInfo ipAddress = adapter.IpAddresses[0];
            Assert.AreEqual("0.0.0.0", ipAddress.Address);
            Assert.AreEqual("0.0.0.0", ipAddress.SubnetMask);
        }

        /// <summary>
        /// Gets the controller driver information using the mock data generated on a RasberryPi3.
        /// </summary>
        [TestMethod]
        public void GetControllerDriverInfo_IoT()
        {
            TestHelpers.MockHttpResponder.AddMockResponse(
                DevicePortal.ControllerDriverApi,
                this.PlatformType,
                this.FriendlyOperatingSystemVersion,
                HttpMethods.Get);

            Task<ControllerDriverInfo> getTask = TestHelpers.Portal.GetControllerDriverInfoAsync();
            getTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, getTask.Status);
            ControllerDriverInfo controllerDriver = getTask.Result;

            // Check some known things about this response.
            Assert.AreEqual("Inbox Driver", controllerDriver.CurrentDriver);
            Assert.AreEqual("1", controllerDriver.RequestReboot);
        }

        /// <summary>
        /// Gets the current date time information using the mock data generated on a RasberryPi3.
        /// </summary>
        [TestMethod]
        public void GetCurrentDateTimeInfo_IoT()
        {
            TestHelpers.MockHttpResponder.AddMockResponse(
                DevicePortal.DateTimeInfoApi,
                this.PlatformType,
                this.FriendlyOperatingSystemVersion,
                HttpMethods.Get);

            Task<DateTimeInfo> getTask = TestHelpers.Portal.GetDateTimeInfoAsync();
            getTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, getTask.Status);
            DateTimeInfo dateTime = getTask.Result;

            // Check some known things about this response.           
            Assert.AreEqual(22, dateTime.CurrentDateTime.Day);
        }

        /// <summary>
        /// Gets the timezone information using the mock data generated on a RasberryPi3.
        /// </summary>
        [TestMethod]
        public void GetTimezoneInfo_IoT()
        {
            TestHelpers.MockHttpResponder.AddMockResponse(
                DevicePortal.TimezoneInfoApi,
                this.PlatformType,
                this.FriendlyOperatingSystemVersion,
                HttpMethods.Get);

            Task<TimezoneInfo> getTask = TestHelpers.Portal.GetTimezoneInfoAsync();
            getTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, getTask.Status);
            TimezoneInfo timezone = getTask.Result;

            // Check some known things about this response.
            Assert.AreEqual("(UTC-06:00) Central Time (US & Canada)", timezone.CurrentTimeZone.Description);
            Assert.AreEqual("(UTC-11:00) Coordinated Universal Time-11", timezone.Timezones[1].Description);
        }

        /// <summary>
        /// Gets the display resolution using the mock data generated on a RasberryPi3.
        /// </summary>
        [TestMethod]
        public void GetDisplayResolutionInfo_IoT()
        {
            TestHelpers.MockHttpResponder.AddMockResponse(
                DevicePortal.DisplayResolutionApi,
                this.PlatformType,
                this.FriendlyOperatingSystemVersion,
                HttpMethods.Get);

            Task<DisplayResolutionInfo> getTask = TestHelpers.Portal.GetDisplayResolutionInfoAsync();
            getTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, getTask.Status);
            DisplayResolutionInfo displayRes = getTask.Result;

            // Check some known things about this response.
            Assert.AreEqual("1680x1050 (60Hz)", displayRes.CurrentResolution.ResolutionDetail);
        }

        /// <summary>
        /// Gets the display orientation using the mock data generated on a RasberryPi3.
        /// </summary>
        [TestMethod]
        public void GetDisplayOrientationInfo_IoT()
        {
            TestHelpers.MockHttpResponder.AddMockResponse(
                DevicePortal.DisplayOrientationApi,
                this.PlatformType,
                this.FriendlyOperatingSystemVersion,
                HttpMethods.Get);

            Task<DisplayOrientationInfo> getTask = TestHelpers.Portal.GetDisplayOrientationInfoAsync();
            getTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, getTask.Status);
            DisplayOrientationInfo displayOrientation = getTask.Result;

            // Check some known things about this response.
            Assert.AreEqual(0, displayOrientation.Orientation);
        }

        /// <summary>
        /// Gets the device information using the mock data generated on a RasberryPi3.
        /// </summary>
        [TestMethod]
        public void GetDeviceInfo_IoT()
        {
            TestHelpers.MockHttpResponder.AddMockResponse(
                DevicePortal.IoTOsInfoApi,
                this.PlatformType,
                this.FriendlyOperatingSystemVersion,
                HttpMethods.Get);

            Task<IoTOSInfo> getTask = TestHelpers.Portal.GetIoTOSInfoAsync();
            getTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, getTask.Status);
            IoTOSInfo deviceIoTInfo = getTask.Result;

            // Check some known things about this response.
            Assert.AreEqual("Raspberry Pi 3", deviceIoTInfo.Model);
            Assert.AreEqual("beta2", deviceIoTInfo.Name);
            Assert.AreEqual("10.0.14393.67", deviceIoTInfo.OSVersion);
        }

        /// <summary>
        /// Gets the status information using the mock data generated on a RasberryPi3.
        /// </summary>
        [TestMethod]
        public void GetStatusInfo_IoT()
        {
            TestHelpers.MockHttpResponder.AddMockResponse(
                DevicePortal.StatusApi,
                this.PlatformType,
                this.FriendlyOperatingSystemVersion,
                HttpMethods.Get);

            Task<StatusInfo> getTask = TestHelpers.Portal.GetStatusInfoAsync();
            getTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, getTask.Status);
            StatusInfo stats = getTask.Result;

            // Check some known things about this response.
            Assert.AreEqual("2016-09-02 at 15:31", stats.LastCheckTime);
            Assert.AreEqual("2016-08-18 at 00:00", stats.LastUpdateTime);
            Assert.AreEqual("Your device is up to date.", stats.UpdateStatusMessage);
        }

        /// <summary>
        /// Gets the update install time information using the mock data generated on a RasberryPi3.
        /// </summary>
        [TestMethod]
        public void GetUpdateInstallTime_IoT()
        {
            TestHelpers.MockHttpResponder.AddMockResponse(
                DevicePortal.InstallTimeApi,
                this.PlatformType,
                this.FriendlyOperatingSystemVersion,
                HttpMethods.Get);

            Task<UpdateInstallTimeInfo> getTask = TestHelpers.Portal.GetUpdateInstallTimeAsync();
            getTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, getTask.Status);
            UpdateInstallTimeInfo installTime = getTask.Result;

            // Check some known things about this response.
            Assert.AreEqual(0, installTime.RebootScheduled);
        }

        /// <summary>
        /// Gets the remote settings status information using the mock data generated on a RasberryPi3.
        /// </summary>
        [TestMethod]
        public void GetRemoteSettingsStatus_IoT()
        {
            TestHelpers.MockHttpResponder.AddMockResponse(
                DevicePortal.RemoteSettingsStatusApi,
                this.PlatformType,
                this.FriendlyOperatingSystemVersion,
                HttpMethods.Get);

            Task<RemoteSettingsStatusInfo> getTask = TestHelpers.Portal.GetRemoteSettingsStatusInfoAsync();
            getTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, getTask.Status);
            RemoteSettingsStatusInfo installTime = getTask.Result;

            // Check some known things about this response.
            Assert.AreEqual(false, installTime.IsRunning);
            Assert.AreEqual(false, installTime.IsScheduled);
        }

        /// <summary>
        /// Gets the SoftAP settings information using the mock data generated on a RasberryPi3.
        /// </summary>
        [TestMethod]
        public void GetSoftAPSettings_IoT()
        {
            TestHelpers.MockHttpResponder.AddMockResponse(
                DevicePortal.SoftAPSettingsApi,
                this.PlatformType,
                this.FriendlyOperatingSystemVersion,
                HttpMethods.Get);

            Task<SoftAPSettingsInfo> getTask = TestHelpers.Portal.GetSoftAPSettingsInfoAsync();
            getTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, getTask.Status);
            SoftAPSettingsInfo softAPSettings = getTask.Result;

            // Check some known things about this response.
            Assert.AreEqual("true", softAPSettings.SoftAPEnabled);
            Assert.AreEqual("SoftAPSsid", softAPSettings.SoftApSsid);
        }

        /// <summary>
        /// Gets the AllJoyn settings information using the mock data generated on a RasberryPi3.
        /// </summary>
        [TestMethod]
        public void GetAllJoynSettings_IoT()
        {
            TestHelpers.MockHttpResponder.AddMockResponse(
                DevicePortal.AllJoynSettingsApi,
                this.PlatformType,
                this.FriendlyOperatingSystemVersion,
                HttpMethods.Get);

            Task<AllJoynSettingsInfo> getTask = TestHelpers.Portal.GetAllJoynSettingsInfoAsync();
            getTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, getTask.Status);
            AllJoynSettingsInfo allJoynSettings = getTask.Result;

            // Check some known things about this response.
            Assert.AreEqual("IoTCore Onboarding service", allJoynSettings.AllJoynOnboardingDefaultDescription);
            Assert.AreEqual("Microsoft", allJoynSettings.AllJoynOnboardingDefaultManufacturer);
        }

        /// <summary>
        /// Simple test of setting the device name for a IoT device
        /// </summary>
        [TestMethod]
        public void SetDeviceNameTest_IoT()
        {
            string deviceName = "test_IoT";

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.NoContent);
            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.DeviceNameApi, response, HttpMethods.Post);

            Task setIoTDeviceName = TestHelpers.Portal.SetIoTDeviceNameAsync(deviceName);
            setIoTDeviceName.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, setIoTDeviceName.Status);
        }

        /// <summary>
        /// Simple test to set SoftAp Settings
        /// </summary>
        [TestMethod]
        public void SetSoftApSettingsTest_IoT()
        {
            string softApEnabled = "true";
            string softApSsid = "SoftAPSsid";
            string softApPassword = "p@ssw0rd";

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.NoContent);
            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.SoftAPSettingsApi, response, HttpMethods.Post);

            Task setSoftApSettings = TestHelpers.Portal.SetSoftApSettingsAsync(softApEnabled, softApSsid, softApPassword);
            setSoftApSettings.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, setSoftApSettings.Status);
        }

        /// <summary>
        /// Simple test to set AllJoyn Settings
        /// </summary>
        [TestMethod]
        public void SetAllJoynSettingsTest_IoT()
        {
            string allJoynStatus = "true";
            string allJoynDescription = "IoTCore Onboarding service";
            string allJoynManufacturer = "Microsoft";
            string allJoynModelNumber = "IoTCore Onboarding";

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.NoContent);
            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.AllJoynSettingsApi, response, HttpMethods.Post);

            Task setAllJoynSettings = TestHelpers.Portal.SetAllJoynSettingsAsync(allJoynStatus, allJoynDescription, allJoynManufacturer, allJoynModelNumber);
            setAllJoynSettings.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, setAllJoynSettings.Status);
        }

        /// <summary>
        /// Simple test of a failed attempt to reset the password
        /// </summary>
        [TestMethod]
        public void SetNewPasswordTest_IoT()
        {
            string oldPassword = "invalid password";
            string newPassword = "qwert";
            int errorCode = 86;
            string status = "Change password failed";

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(string.Format("{{\"ErrorCode\" : {0}, \"Status\" : \"{1}\"}}", errorCode, status));

            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.ResetPasswordApi, response, HttpMethods.Post);

            Task<ErrorInformation> setIoTNewPassword = TestHelpers.Portal.SetNewPasswordAsync(oldPassword, newPassword);
            setIoTNewPassword.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, setIoTNewPassword.Status);
            Assert.AreEqual(errorCode, setIoTNewPassword.Result.ErrorCode);
            Assert.AreEqual(status, setIoTNewPassword.Result.Status);
        }

        /// <summary>
        /// Simple test to set the new remote debugging pin for an IoT device
        /// </summary>
        [TestMethod]
        public void SetNewRemoteDebuggingPinTest_IoT()
        {
            string newPin = "123";

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.NoContent);
            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.NewRemoteDebuggingPinApi, response, HttpMethods.Post);

            Task setIoTNewRemoteDebuggingPin = TestHelpers.Portal.SetNewRemoteDebuggingPinAsync(newPin);
            setIoTNewRemoteDebuggingPin.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, setIoTNewRemoteDebuggingPin.Status);
        }

        /// <summary>
        /// Simple test to set a new Controller driver on the IoT device
        /// </summary>
        [TestMethod]
        public void SetControllersDriversTest_IoT()
        {
            string newDriver = "Inbox Driver";
            string requestReboot = "1";

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(string.Format("{{\"RequestReboot\" : \"{0}\"}}", requestReboot));

            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.ControllerDriverApi, response, HttpMethods.Post);

            Task<ControllerDriverInfo> setIoTControllersDrivers = TestHelpers.Portal.SetControllersDriversAsync(newDriver);
            setIoTControllersDrivers.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, setIoTControllersDrivers.Status);
            Assert.AreEqual(requestReboot, setIoTControllersDrivers.Result.RequestReboot);
        }

        /// <summary>
        /// Simple test to set the timezone of an IoT device
        /// </summary>
        [TestMethod]
        public void SetTimeZoneTest_IoT()
        {
            int index = 0;

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.NoContent);
            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.SetTimeZoneApi, response, HttpMethods.Post);

            Task setIoTTimeZone = TestHelpers.Portal.SetTimeZoneAsync(index);
            setIoTTimeZone.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, setIoTTimeZone.Status);
        }

        /// <summary>
        /// Simple test to set the display resolution of an IoT device
        /// </summary>
        [TestMethod]
        public void SetDisplayResolutionTest_IoT()
        {
            string displayResolution = "1600x1200 (75Hz)";

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.NoContent);
            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.DisplayResolutionApi, response, HttpMethods.Post);

            Task setDisplayResolution = TestHelpers.Portal.SetDisplayResolutionAsync(displayResolution);
            setDisplayResolution.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, setDisplayResolution.Status);
        }

         /// <summary>
        /// Simple test to set the display orientation of an IoT device
        /// </summary>
        [TestMethod]
        public void SetDisplayOrientationTest_IoT()
        {
            string displayOrientation = "90";

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.NoContent);
            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.DisplayOrientationApi, response, HttpMethods.Post);

            Task setIoTDisplayOrientation = TestHelpers.Portal.SetDisplayOrientationAsync(displayOrientation);
            setIoTDisplayOrientation.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, setIoTDisplayOrientation.Status);
        }

        /// <summary>
        /// Gets the list of applications using the mock data generated on a RasberryPi3.
        /// </summary>
        [TestMethod]
        public void GetAppsListInfo_IoT()
        {
            TestHelpers.MockHttpResponder.AddMockResponse(
                DevicePortal.AppsListApi,
                this.PlatformType,
                this.FriendlyOperatingSystemVersion,
                HttpMethods.Get);

            Task<AppsListInfo> getTask = TestHelpers.Portal.GetAppsListInfoAsync();
            getTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, getTask.Status);
            AppsListInfo appsList = getTask.Result;

            // Check some known things about this response.
            Assert.AreEqual("23983CETAthensQuality.IoTCoreSmartDisplay_7grdn1j1n8awe!App", appsList.DefaultApp);
        }

        /// <summary>
        /// Gets the list of applications using the mock data generated on a RasberryPi3.
        /// </summary>
        [TestMethod]
        public void GetHeadlessAppsListInfo_IoT()
        {
            TestHelpers.MockHttpResponder.AddMockResponse(
                DevicePortal.HeadlessAppsListApi,
                this.PlatformType,
                this.FriendlyOperatingSystemVersion,
                HttpMethods.Get);

            Task<HeadlessAppsListInfo> getTask = TestHelpers.Portal.GetHeadlessAppsListInfoAsync();
            getTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, getTask.Status);
            HeadlessAppsListInfo appsList = getTask.Result;

            // Check some known things about this response.
            Assert.AreEqual(true, appsList.AppPackages.First().IsStartup);
        }

        /// <summary>
        /// Simple test to set the application as a startup app.
        /// </summary>
        [TestMethod]
        public void UpdateStartupAppTest_IoT()
        {
            string startupApp = "23983CETAthensQuality.IoTCoreSmartDisplay_7grdn1j1n8awe!App";

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.NoContent);
            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.AppsListApi, response, HttpMethods.Post);

            Task setIoTStartupApp = TestHelpers.Portal.UpdateStartupAppAsync(startupApp);
            setIoTStartupApp.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, setIoTStartupApp.Status);
        }

        /// <summary>
        /// Simple test to set the application as a startup app.
        /// </summary>
        [TestMethod]
        public void UpdateHeadlessStartupAppTest_IoT()
        {
            string startupApp = "ZWaveAdapterHeadlessAdapterApp_1w720vyc4ccym!ZWaveHeadlessAdapterApp";

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.NoContent);
            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.HeadlessStartupAppApi, response, HttpMethods.Post);

            Task setIoTHeadlessStartupApp = TestHelpers.Portal.UpdateHeadlessStartupAppAsync(startupApp);
            setIoTHeadlessStartupApp.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, setIoTHeadlessStartupApp.Status);
        }

        /// <summary>
        /// Simple test to set the application as a startup app.
        /// </summary>
        [TestMethod]
        public void RemoveHeadlessStartupAppTest_IoT()
        {
            string startupApp = "ZWaveAdapterHeadlessAdapterApp_1w720vyc4ccym!ZWaveHeadlessAdapterApp";

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.NoContent);
            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.HeadlessStartupAppApi, response, HttpMethods.Delete);

            Task removeIoTHeadlessStartupApp = TestHelpers.Portal.RemoveHeadlessStartupAppAsync(startupApp);
            removeIoTHeadlessStartupApp.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, removeIoTHeadlessStartupApp.Status);
        }

        /// <summary>
        /// Simple test to set the application as a startup app.
        /// </summary>
        [TestMethod]
        public void ActivatePackageTest_IoT()
        {
            string appId = "ZWaveAdapterHeadlessAdapterApp_1w720vyc4ccym!ZWaveHeadlessAdapterApp";

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.NoContent);
            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.ActivatePackageApi, response, HttpMethods.Post);

            Task activatePackage = TestHelpers.Portal.ActivatePackageAsync(appId);
            activatePackage.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, activatePackage.Status);
        }

        /// <summary>
        /// Gets the list of audio devices connected to the IoT device.
        /// </summary>
        [TestMethod]
        public void GetAudioDeviceListInfo_IoT()
        {
            TestHelpers.MockHttpResponder.AddMockResponse(
                DevicePortal.AudioDeviceListApi,
                this.PlatformType,
                this.FriendlyOperatingSystemVersion,
                HttpMethods.Get);

            Task<AudioDeviceListInfo> getTask = TestHelpers.Portal.GetAudioDeviceListInfoAsync();
            getTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, getTask.Status);
            AudioDeviceListInfo audioDeviceList = getTask.Result;

            // Check some known things about this response.
            Assert.AreEqual("Headset Microphone (2- GN 2000 MS USB)", audioDeviceList.CaptureName);
        }

        /// <summary>
        /// Test to set the render volume in audio devices connected to the IoT device.
        /// </summary>
        [TestMethod]
        public void SetRenderVolumeTest_IoT()
        {
            string renderVolume = "80";

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.NoContent);
            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.SetRenderVolumeApi, response, HttpMethods.Post);

            Task renderVolumeTask = TestHelpers.Portal.SetRenderVolumeAsync(renderVolume);
            renderVolumeTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, renderVolumeTask.Status);
        }

        /// <summary>
        /// Simple test to set the capture volume in audio devices connected to the IoT device.
        /// </summary>
        [TestMethod]
        public void SetCaptureVolumeTest_IoT()
        {
            string captureVolume = "80";

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.NoContent);
            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.SetCaptureVolumeApi, response, HttpMethods.Post);

            Task captureVolumeTask = TestHelpers.Portal.SetCaptureVolumeAsync(captureVolume);
            captureVolumeTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, captureVolumeTask.Status);
        }

        /// <summary>
        /// Simple test to start the internt sonnection sharing in the IoT device.
        /// </summary>
        [TestMethod]
        public void IcSharingStartTest_IoT()
        {
            string privateInterfaceIndex = "0";
            string publicInterfaceIndex = "1";

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.NoContent);
            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.IcSharingApi, response, HttpMethods.Post);

            Task icsStart = TestHelpers.Portal.IcSharingStartAsync(privateInterfaceIndex, publicInterfaceIndex);
            icsStart.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, icsStart.Status);
        }

        /// <summary>
        /// Simple test to stop the internt sonnection sharing in the IoT device.
        /// </summary>
        [TestMethod]
        public void IcSharingStopTest_IoT()
        {
            string privateInterfaceIndex = "0";
            string publicInterfaceIndex = "1";

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.NoContent);
            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.IcSharingApi, response, HttpMethods.Delete);

            Task icsStop = TestHelpers.Portal.IcSharingStopAsync(privateInterfaceIndex, publicInterfaceIndex);
            icsStop.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, icsStop.Status);
        }

        /// <summary>
        /// Gets the internet connection sharing interface information of the IoT device.
        /// </summary>
        [TestMethod]
        public void GetIcsInterfacesInfo_IoT()
        {
            TestHelpers.MockHttpResponder.AddMockResponse(
                DevicePortal.IcsInterfacesApi,
                this.PlatformType,
                this.FriendlyOperatingSystemVersion,
                HttpMethods.Get);

            Task<IscInterfacesInfo> getTask = TestHelpers.Portal.GetIcsInterfacesInfoAsync();
            getTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, getTask.Status);
            IscInterfacesInfo icsInterfaceInfo = getTask.Result;

            // Check some known things about this response.
            Assert.AreEqual("Broadcom 802.11n Wireless SDIO Adapter", icsInterfaceInfo.PrivateInterfaces.First());
        }

        /// <summary>
        /// Simple test to Run Command on the IoT device.
        /// </summary>
        [TestMethod]
        public void SetRunCommandTest_IoT()
        {
            string command = "tlist";
            string runAsDefaultAccount = "true";

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.NoContent);
            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.RunCommandApi, response, HttpMethods.Post);

            Task runCommand = TestHelpers.Portal.RunCommandAsync(command, runAsDefaultAccount);
            runCommand.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, runCommand.Status);
        }

        /// <summary>
        /// Simple test to Run Command without output on the IoT device.
        /// </summary>
        [TestMethod]
        public void SetRunCommandWithoutOutputTest_IoT()
        {
            string command = "tlist";
            string runAsDefaultAccount = "true";
            string timeout = "10000";
            string output = "Mockl output";
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(string.Format("{{\"output\" : \"{0}\"}}", output));

            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.RunCommandWithoutOutputApi, response, HttpMethods.Post);

            Task<RunCommandOutputInfo> runCommandWithoutOutput = TestHelpers.Portal.RunCommandWithoutOutputAsync(command, runAsDefaultAccount, timeout);
            runCommandWithoutOutput.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, runCommandWithoutOutput.Status);
        }
   
        /// <summary>
        /// Gets the Remote Settings Status information of the IoT device.
        /// </summary>
        [TestMethod]
        public void GetRemoteSettingsStatusInfo_IoT()
        {
            TestHelpers.MockHttpResponder.AddMockResponse(
                DevicePortal.RemoteSettingsStatusApi,
                this.PlatformType,
                this.FriendlyOperatingSystemVersion,
                HttpMethods.Get);

            Task<RemoteSettingsStatusInfo> getTask = TestHelpers.Portal.GetRemoteSettingsStatusInfoAsync();
            getTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, getTask.Status);
            RemoteSettingsStatusInfo remoteSettingsStatusInfo = getTask.Result;

            // Check some known things about this response.
            Assert.AreEqual(false, remoteSettingsStatusInfo.IsRunning);
        }

        /// <summary>
        /// Simple test to Enable the Remote Settings on the IoT device now.
        /// </summary>
        [TestMethod]
        public void RemoteSettingsEnableTest_IoT()
        {
            string isRunning = "true";
            string isScheduled = "true";
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(string.Format("{{\"IsRunning\" : {0}, \"IsScheduled\" : {1}}}", isRunning, isScheduled));

             TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.RemoteSettingsEnableApi, response, HttpMethods.Post);

            Task<RemoteSettingsStatusInfo> remoteSettingsEnable = TestHelpers.Portal.RemoteSettingsEnableAsync();
            remoteSettingsEnable.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, remoteSettingsEnable.Status);
        }

        /// <summary>
        /// Simple test to Disable the Remote Settings on the IoT device now.
        /// </summary>
        [TestMethod]
        public void RemoteSettingsDisableTest_IoT()
        {
            string isRunning = "false";
            string isScheduled = "false";
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(string.Format("{{\"IsRunning\" : {0}, \"IsScheduled\" : {1}}}", isRunning, isScheduled));

            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.RemoteSettingsDisableApi, response, HttpMethods.Post);

            Task<RemoteSettingsStatusInfo> remoteSettingsDisable = TestHelpers.Portal.RemoteSettingsDisableAsync();
            remoteSettingsDisable.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, remoteSettingsDisable.Status);
        }

        /// <summary>
        /// Tests the Get response for TPM Settings of the IoT device.
        /// </summary>
        [TestMethod]
        public void GetTpmSettingsInfo_IoT()
        {
            TestHelpers.MockHttpResponder.AddMockResponse(
                DevicePortal.TpmSettingsApi,
                this.PlatformType,
                this.FriendlyOperatingSystemVersion,
                HttpMethods.Get);

            Task<TpmSettingsInfo> getTask = TestHelpers.Portal.GetTpmSettingsInfoAsync();
            getTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, getTask.Status);
            TpmSettingsInfo tpmSettings = getTask.Result;

            // Check some known things about this response.
            Assert.AreEqual("TpmOk", tpmSettings.TPMStatus);
        }

        /// <summary>
        /// Tests the Get response for TPM ACPI Tables Information.
        /// </summary>
        [TestMethod]
        public void GetTpmAcpiTablesInfo_IoT()
        {
            TestHelpers.MockHttpResponder.AddMockResponse(
                DevicePortal.TpmAcpiTablesApi,
                this.PlatformType,
                this.FriendlyOperatingSystemVersion,
                HttpMethods.Get);

            Task<TpmAcpiTablesInfo> getTask = TestHelpers.Portal.GetTpmAcpiTablesInfoAsync();
            getTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, getTask.Status);
            TpmAcpiTablesInfo tpmAcpiTables = getTask.Result;

            // Check some known things about this response.
            Assert.AreEqual("Software TPM Emulator (NoSecurity)", tpmAcpiTables.AcpiTables[1]);
        }

        /// <summary>
        /// Tests the Get response for TPM Logical Device Settings for the IoT device.
        /// </summary>
        [TestMethod]
        public void GetTpmLogicalDeviceSettingsInfo_IoT()
        {
            int logicalDeviceId = 1;

            TestHelpers.MockHttpResponder.AddMockResponse(
                string.Format("{0}/{1}", DevicePortal.TpmSettingsApi, logicalDeviceId),
                this.PlatformType,
                this.FriendlyOperatingSystemVersion,
                HttpMethods.Get);

            Task<TpmLogicalDeviceSettingsInfo> getTask = TestHelpers.Portal.GetTpmLogicalDeviceSettingsInfoAsync(logicalDeviceId);
            getTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, getTask.Status);
            TpmLogicalDeviceSettingsInfo tpmSettings = getTask.Result;

            // Check some known things about this response.
            Assert.AreEqual(" a28a2715cb06bf65c8c41c5e0e3885b2d02321b8d450bb9409f322e708bc0099", tpmSettings.DeviceId);
        }

        /// <summary>
        /// Simple test to Set the TPM ACPI Tables on the IoT device.
        /// </summary>
        [TestMethod]
        public void SetTpmAcpiTablesTest_IoT()
        {
            string acpiTableIndex = "1";

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.NoContent);
            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.TpmAcpiTablesApi, response, HttpMethods.Post);

            Task tpmAcpiTables = TestHelpers.Portal.SetTpmAcpiTablesInfoAsync(acpiTableIndex);
            tpmAcpiTables.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, tpmAcpiTables.Status);
        }

        /// <summary>
        /// Simple test to Set the TPM Logical Device Settings on the IoT device.
        /// </summary>
        [TestMethod]
        public void SetTpmLogicalDeviceSettingsTest_IoT()
        {
            int logicalDeviceId = 1;
            string azureUri = string.Empty;
            string azureKey = string.Empty;

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.NoContent);
            TestHelpers.MockHttpResponder.AddMockResponse(string.Format("{0}/{1}", DevicePortal.TpmSettingsApi, logicalDeviceId), response, HttpMethods.Post);

            Task tpmLogicalDeviceSettings = TestHelpers.Portal.SetTpmLogicalDeviceSettingsInfoAsync(logicalDeviceId, azureUri, azureKey);
            tpmLogicalDeviceSettings.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, tpmLogicalDeviceSettings.Status);
        }

        /// <summary>
        /// Simple test to Reset the TPM Logical Device Settings on the IoT device.
        /// </summary>
        [TestMethod]
        public void ResetTpmLogicalDeviceSettingsTest_IoT()
        {
            int logicalDeviceId = 1;
           
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.NoContent);
            TestHelpers.MockHttpResponder.AddMockResponse(string.Format("{0}/{1}", DevicePortal.TpmSettingsApi, logicalDeviceId), response, HttpMethods.Delete);

            Task tpmLogicalDeviceSettings = TestHelpers.Portal.ResetTpmLogicalDeviceSettingsInfoAsync(logicalDeviceId);
            tpmLogicalDeviceSettings.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, tpmLogicalDeviceSettings.Status);
        }

        /// <summary>
        /// Tests the Get response for TPM Azure Token Info for the IoT device.
        /// </summary>
        [TestMethod]
        public void GetTpmAzureTokenInfo_IoT()
        {
            int logicalDeviceId = 0;
            string validity = "18000";

            TestHelpers.MockHttpResponder.AddMockResponse(
               string.Format("{0}/{1}", DevicePortal.TpmAzureTokenApi, logicalDeviceId),
                this.PlatformType,
                this.FriendlyOperatingSystemVersion,
                HttpMethods.Get);

            Task<TpmAzureTokenInfo> getTask = TestHelpers.Portal.GetTpmAzureTokenInfoAsync(logicalDeviceId, validity);
            getTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, getTask.Status);
            TpmAzureTokenInfo tpmAzureToken = getTask.Result;

            // Check some known things about this response.
            Assert.AreEqual("HostName=q;DeviceId=q;SharedAccessSignature=SharedAccessSignature sr=q/devices/q&sig=SCPRMahoAA%2belCZ3KvTLFEcVk2C2SYZ0G00zqZ5yH2k%3d&se=1473400166", tpmAzureToken.AzureToken);
        }
    }
}
