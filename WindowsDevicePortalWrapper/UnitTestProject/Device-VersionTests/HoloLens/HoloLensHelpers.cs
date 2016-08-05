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
        /// Helper method for verifying OS info based on a given version.
        /// </summary>
        /// <param name="friendlyOperatingSystemVersion">The friendly version of the OS we are targeting.</param>
        /// <param name="operatingSystemVersion">The version of the OS we are targeting.</param>
        public static void VerifyOsInformation(
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
