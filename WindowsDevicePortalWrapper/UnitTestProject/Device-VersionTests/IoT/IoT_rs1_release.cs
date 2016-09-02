//----------------------------------------------------------------------------------------------
// <copyright file="IoT_rs1_release.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Microsoft.Tools.WindowsDevicePortal.DevicePortal;
using System.Net.Http;
using System.Net;

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

            Task<BatteryState> getTask  = TestHelpers.Portal.GetBatteryState();
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

            Task<string> getTask  = TestHelpers.Portal.GetDeviceName();
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

            Task<IpConfiguration> getTask  = TestHelpers.Portal.GetIpConfig();
            getTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, getTask.Status);

            IpConfiguration ipconfig = getTask.Result;

            // Check some known things about this response.
            NetworkAdapterInfo adapter = ipconfig.Adapters[0];
            Assert.AreEqual(" Device (Personal Area Network)", adapter.Description);
            Assert.AreEqual("b8-27-eb-8d-0b-c5", adapter.MacAddress);
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

            Task<ControllerDriverInfo> getTask = TestHelpers.Portal.GetControllerDriverInfo();
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

            Task<DateTimeInfo> getTask = TestHelpers.Portal.GetDateTimeInfo();
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

            Task<TimezoneInfo> getTask = TestHelpers.Portal.GetTimezoneInfo();
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

            Task<DisplayResolutionInfo> getTask = TestHelpers.Portal.GetDisplayResolutionInfo();
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

            Task<DisplayOrientationInfo> getTask = TestHelpers.Portal.GetDisplayOrientationInfo();
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

            Task<IoTOSInfo> getTask = TestHelpers.Portal.GetIoTOSInfo();
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

            Task<StatusInfo> getTask = TestHelpers.Portal.GetStatusInfo();
            getTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, getTask.Status);
            StatusInfo stats = getTask.Result;

            // Check some known things about this response.
            Assert.AreEqual("2016-08-22 at 14:15", stats.LastCheckTime);
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

            Task<UpdateInstallTimeInfo> getTask = TestHelpers.Portal.GetUpdateInstallTime();
            getTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, getTask.Status);
            UpdateInstallTimeInfo installTime = getTask.Result;

            // Check some known things about this response.
            Assert.AreEqual(0, installTime.Rebootscheduled); 
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

            Task<RemoteSettingsStatusInfo> getTask = TestHelpers.Portal.GetRemoteSettingsStatusInfo();
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

            Task<SoftAPSettingsInfo> getTask = TestHelpers.Portal.GetSoftAPSettingsInfo();
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

            Task<AllJoynSettingsInfo> getTask = TestHelpers.Portal.GetAllJoynSettingsInfo();
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
        public void SetIoTDeviceNameTest()
        {
            string deviceName = "test_IoT";

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.NoContent);
            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.DeviceNameApi, response, HttpMethods.Post);

            Task setIoTDeviceName = TestHelpers.Portal.SetIoTDeviceName(deviceName);
            setIoTDeviceName.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, setIoTDeviceName.Status);
        }

        /// <summary>
        /// Simple test of a failed attempt to reset the password
        /// </summary>
        [TestMethod]
        public void SetIoTNewPasswordTest()
        {
            string oldPassword = "invalid password";
            string newPassword = "qwert";
            int errorCode = 86;
            string status = "Change password failed";

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(string.Format("{{\"ErrorCode\" : {0}, \"Status\" : \"{1}\"}}", errorCode, status));

            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.ResetPasswordApi, response, HttpMethods.Post);

            Task<ErrorInformation> setIoTNewPassword = TestHelpers.Portal.SetNewPassword(oldPassword, newPassword);
            setIoTNewPassword.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, setIoTNewPassword.Status);
            Assert.AreEqual(errorCode, setIoTNewPassword.Result.ErrorCode);
            Assert.AreEqual(status, setIoTNewPassword.Result.Status);
        }

        /// <summary>
        /// Simple test to set the new remote debugging pin for an IoT device
        /// </summary>
        [TestMethod]
        public void SetIoTNewRemoteDebuggingPinTest()
        {
            string newPin = "123";

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.NoContent);
            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.NewRemoteDebuggingPinApi, response, HttpMethods.Post);

            Task setIoTNewRemoteDebuggingPin = TestHelpers.Portal.SetNewRemoteDebuggingPin(newPin);
            setIoTNewRemoteDebuggingPin.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, setIoTNewRemoteDebuggingPin.Status);
        }

        /// <summary>
        /// Simple test to set a new Controller driver on the IoT device
        /// </summary>
        [TestMethod]
        public void SetIoTControllersDriversTest()
        {
            string newDriver = "Inbox Driver";
            string requestReboot = "1";

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(string.Format("{{\"RequestReboot\" : \"{0}\"}}", requestReboot));

            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.ControllerDriverApi, response, HttpMethods.Post);

            Task<ControllerDriverInfo> setIoTControllersDrivers = TestHelpers.Portal.SetControllersDrivers(newDriver);
            setIoTControllersDrivers.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, setIoTControllersDrivers.Status);
            Assert.AreEqual(requestReboot, setIoTControllersDrivers.Result.RequestReboot );
        }

        /// <summary>
        /// Simple test to set the timezone of an IoT device
        /// </summary>
        [TestMethod]
        public void SetIoTTimeZoneTest()
        {
            int index = 0;

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.NoContent);
            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.SetTimeZoneApi, response, HttpMethods.Post);

            Task setIoTTimeZone = TestHelpers.Portal.SetTimeZone(index);
            setIoTTimeZone.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, setIoTTimeZone.Status);
        }

        /// <summary>
        /// Simple test to set the display resolution of an IoT device
        /// </summary>
        [TestMethod]
        public void SetIoTDisplayResolutionTest()
        {
            string displayResolution = "1600x1200 (75Hz)";

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.NoContent);
            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.DisplayResolutionApi, response, HttpMethods.Post);

            Task setDisplayResolution = TestHelpers.Portal.SetDisplayResolution(displayResolution);
            setDisplayResolution.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, setDisplayResolution.Status);
        }

         /// <summary>
        /// Simple test to set the display orientation of an IoT device
        /// </summary>
        [TestMethod]
        public void SetIoTDisplayOrientationTest()
        {
            string displayOrientation = "90";

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.NoContent);
            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.DisplayOrientationApi, response, HttpMethods.Post);

            Task setIoTDisplayOrientation = TestHelpers.Portal.SetDisplayOrientation(displayOrientation);
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

            Task<AppsListInfo> getTask = TestHelpers.Portal.GetAppsListInfo();
            getTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, getTask.Status);
            AppsListInfo appsList = getTask.Result;

            // Check some known things about this response.
            Assert.AreEqual("23983CETAthensQuality.IoTCoreSmartDisplay_7grdn1j1n8awe!App", appsList.DefaultApp);
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

            Task setIoTStartupApp = TestHelpers.Portal.UpdateStartupApp(startupApp);
            setIoTStartupApp.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, setIoTStartupApp.Status);
        }

        /// <summary>
        /// Gets the list of headless applications using the mock data generated on a RasberryPi3.
        /// </summary>
        [TestMethod]
        public void GetHeadlessAppListInfo_IoT()
        {
            TestHelpers.MockHttpResponder.AddMockResponse(
                DevicePortal.HeadlessAppsListApi,
                this.PlatformType,
                this.FriendlyOperatingSystemVersion,
                HttpMethods.Get);

            Task<HeadlessAppsListInfo> getTask = TestHelpers.Portal.GetHeadlessAppsListInfo();
            getTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, getTask.Status);
            HeadlessAppsListInfo headlessAppsList = getTask.Result;

            // Check some known things about this response.
            Assert.AreEqual("ZWaveAdapterHeadlessAdapterApp_1w720vyc4ccym!ZWaveHeadlessAdapterApp", headlessAppsList.AppPackages[0].PackageFullName);
        }

    }
}
