//----------------------------------------------------------------------------------------------
// <copyright file="IoT_rs1_release.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

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
            Assert.AreEqual("
                
                
                Device (Personal Area Network)", adapter.Description);
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
    }
}
