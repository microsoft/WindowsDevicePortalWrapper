//----------------------------------------------------------------------------------------------
// <copyright file="HoloLensHelpers.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Microsoft.Tools.WindowsDevicePortal.DevicePortal;

namespace Microsoft.Tools.WindowsDevicePortal.Tests
{
    /// <summary>
    /// Helpers for HoloLens tests.
    /// </summary>
    public static class HoloLensHelpers
    {
        /// <summary>
        /// Validate the <see cref="SystemPerformanceInformation" /> returned from the HoloLens tests.
        /// </summary>
        /// <param name="systemPerfInfo">The <see cref="SystemPerformanceInformation" /> to validate.</param>
        internal static void ValidateSystemPerf(SystemPerformanceInformation systemPerfInfo)
        {
            // Check some known things about this response.
            Assert.AreEqual(275897, systemPerfInfo.AvailablePages);
            Assert.AreEqual(764290, systemPerfInfo.CommitLimit);
            Assert.AreEqual(225486, systemPerfInfo.CommittedPages);
            Assert.AreEqual(20, systemPerfInfo.CpuLoad);
            Assert.AreEqual(4337544, systemPerfInfo.IoOtherSpeed);
            Assert.AreEqual(1717438, systemPerfInfo.IoReadSpeed);
            Assert.AreEqual(788621, systemPerfInfo.IoWriteSpeed);
            Assert.AreEqual(15470, systemPerfInfo.NonPagedPoolPages);
            Assert.AreEqual(4096, systemPerfInfo.PageSize);
            Assert.AreEqual(18894, systemPerfInfo.PagedPoolPages);
            Assert.AreEqual(2097152, systemPerfInfo.TotalInstalledKb);
            Assert.AreEqual(502146, systemPerfInfo.TotalPages);

            Assert.AreEqual(systemPerfInfo.GpuData.Adapters.Count, 1);
            GpuAdapter gpuAdapter = systemPerfInfo.GpuData.Adapters[0];
            Assert.AreEqual((uint)119537664, gpuAdapter.DedicatedMemory);
            Assert.AreEqual((uint)65536, gpuAdapter.DedicatedMemoryUsed);
            Assert.AreEqual("HoloLens Graphics", gpuAdapter.Description);
            Assert.AreEqual((uint)1028395008, gpuAdapter.SystemMemory);
            Assert.AreEqual((uint)48513024, gpuAdapter.SystemMemoryUsed);

            Assert.AreEqual(9, gpuAdapter.EnginesUtilization.Count);
            Assert.AreEqual("7.098184", gpuAdapter.EnginesUtilization[0].ToString("n6"));

            NetworkPerformanceData networkPerformanceData = systemPerfInfo.NetworkData;
            Assert.AreEqual(0, networkPerformanceData.BytesIn);
            Assert.AreEqual(0, networkPerformanceData.BytesOut);
        }
        
        /// <summary>
        /// Helper method for verifying OS info based on a given version.
        /// </summary>
        /// <param name="friendlyOperatingSystemVersion">The friendly version of the OS we are targeting.</param>
        /// <param name="operatingSystemVersion">The version of the OS we are targeting.</param>
        internal static void VerifyOsInformation(
            string friendlyOperatingSystemVersion, 
            string operatingSystemVersion)
        {
            TestHelpers.MockHttpResponder.AddMockResponse(
                DevicePortal.MachineNameApi, 
                DevicePortalPlatforms.HoloLens, 
                friendlyOperatingSystemVersion, 
                HttpMethods.Get);

            Task<string> getNameTask = TestHelpers.Portal.GetDeviceName();
            getNameTask.Wait();

            Assert.AreEqual(operatingSystemVersion, TestHelpers.Portal.OperatingSystemVersion);
            Assert.AreEqual(DevicePortalPlatforms.HoloLens, TestHelpers.Portal.Platform);
            Assert.AreEqual("MyHoloLens", getNameTask.Result);
        }
    }
}
