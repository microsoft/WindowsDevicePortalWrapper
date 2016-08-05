//----------------------------------------------------------------------------------------------
// <copyright file="HoloLens_rs1_release.cs" company="Microsoft Corporation">
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
    /// Test class for HoloLens_rs1_release version
    /// </summary>
    [TestClass]
    public class HoloLens_rs1_release : BaseTests
    {
        /// <summary>
        /// Gets the Platform type these tests are targeting.
        /// </summary>
        protected override DevicePortalPlatforms PlatformType
        {
            get
            {
                return DevicePortalPlatforms.HoloLens;
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
                return "14393.0.x86fre.rs1_release.160715-1616";
            }
        }

        /// <summary>
        /// Gets the battery state using a mock generated on a HoloLens.
        /// </summary>
        [TestMethod]
        public void GetBatteryState_HoloLens_1607()
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
            Assert.AreEqual(true, batteryState.IsBatteryPresent);
            Assert.AreEqual(4294967295, batteryState.EstimatedTimeRaw);
            Assert.AreEqual(15079, batteryState.MaximumCapacity);
            Assert.AreEqual(14921, batteryState.RemainingCapacity);
        }

        /// <summary>
        /// Gets the device name using a mock generated on a HoloLens.
        /// </summary>
        [TestMethod]
        public void GetDeviceName_HoloLens_1607()
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
            Assert.AreEqual("MyHoloLens", getTask.Result);
        }

        /// <summary>
        /// Gets the user's interpupilary distance using a mock generated on a HoloLens.
        /// </summary>
        [TestMethod]
        public void GetIpd_HoloLens_1607()
        {
            TestHelpers.MockHttpResponder.AddMockResponse(
                DevicePortal.HolographicIpdApi, 
                this.PlatformType, 
                this.FriendlyOperatingSystemVersion, 
                HttpMethods.Get);

            Task<float> getTask  = TestHelpers.Portal.GetInterPupilaryDistance();
            getTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, getTask.Status);

            // Check some known things about this response.
            Assert.AreEqual(67.5, getTask.Result);
        }

        /// <summary>
        /// Gets the list of Mixed Reality Capture files using a mock generated on a HoloLens.
        /// </summary>
        [TestMethod]
        public void GetMrcFileList_HoloLens_1607()
        {
            TestHelpers.MockHttpResponder.AddMockResponse(
                DevicePortal.MrcFileListApi, 
                this.PlatformType, 
                this.FriendlyOperatingSystemVersion, 
                HttpMethods.Get);

            Task<MrcFileList> getTask  = TestHelpers.Portal.GetMrcFileList();
            getTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, getTask.Status);

            // Check some known things about this response.
            MrcFileList fileList = getTask.Result;
            Assert.AreEqual(4, fileList.Files.Count);
            Assert.AreEqual(131139576909916579, fileList.Files[1].CreationTimeRaw);
            Assert.AreEqual("20160725_150130_HoloLens.jpg", fileList.Files[1].FileName);
            Assert.AreEqual((uint)290929, fileList.Files[1].FileSize);
       }

        /// <summary>
        /// Gets the Mixed Reality Capture status using a mock generated on a HoloLens.
        /// </summary>
        [TestMethod]
        public void GetMrcStatus_HoloLens_1607()
        {
            TestHelpers.MockHttpResponder.AddMockResponse(
                DevicePortal.MrcStatusApi, 
                this.PlatformType, 
                this.FriendlyOperatingSystemVersion, 
                HttpMethods.Get);

            Task<MrcStatus> getTask  = TestHelpers.Portal.GetMrcStatus();
            getTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, getTask.Status);

            // Check some known things about this response.
            ProcessStatus processStatus = getTask.Result.Status;
            Assert.AreEqual("Running", processStatus.MrcProcess);
        }

        /// <summary>
        /// Gets the power state using a mock generated on a HoloLens.
        /// </summary>
        [TestMethod]
        public void GetPowerState_HoloLens_1607()
        {
            TestHelpers.MockHttpResponder.AddMockResponse(
                DevicePortal.PowerStateApi, 
                this.PlatformType, 
                this.FriendlyOperatingSystemVersion, 
                HttpMethods.Get);

            Task<PowerState> getTask  = TestHelpers.Portal.GetPowerState();
            getTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, getTask.Status);

            // Check some known things about this response.
            PowerState powerState = getTask.Result;
            Assert.AreEqual(false, powerState.InLowPowerState);
            Assert.AreEqual(true, powerState.IsLowPowerStateAvailable);
        }

        /// <summary>
        /// Gets the thermal stage using a mock generated on a HoloLens.
        /// </summary>
        [TestMethod]
        public void GetThermalStage_HoloLens_1607()
        {
            TestHelpers.MockHttpResponder.AddMockResponse(
                DevicePortal.ThermalStageApi, 
                this.PlatformType, 
                this.FriendlyOperatingSystemVersion, 
                HttpMethods.Get);

            Task<ThermalStages> getTask  = TestHelpers.Portal.GetThermalStage();
            getTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, getTask.Status);

            // Check some known things about this response.
            Assert.AreEqual(ThermalStages.Normal, getTask.Result);
        }

        /// <summary>
        /// Validates the Operating System information using a mock generated on a HoloLens.
        /// </summary>
        [TestMethod]
        public void ValidateOsInfo_HoloLens_1607()
        {
            HoloLensHelpers.VerifyOsInformation(
                this.FriendlyOperatingSystemVersion, 
                this.OperatingSystemVersion);
        }
    }
}
