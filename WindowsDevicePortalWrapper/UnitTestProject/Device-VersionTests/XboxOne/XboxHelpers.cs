//----------------------------------------------------------------------------------------------
// <copyright file="XboxHelpers.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Microsoft.Tools.WindowsDevicePortal.DevicePortal;

namespace Microsoft.Tools.WindowsDevicePortal.Tests
{
    /// <summary>
    /// Helpers for Xbox tests.
    /// </summary>
    public class XboxHelpers
    {
        /// <summary>
        /// Helper method for verifying OS info based on a given version.
        /// </summary>
        /// <param name="friendlyOperatingSystemVersion">The friendly version of the OS we are targeting.</param>
        /// <param name="operatingSystemVersion">The version of the OS we are targeting.</param>
        public static void VerifyOsInformation(string friendlyOperatingSystemVersion, string operatingSystemVersion)
        {
            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.MachineNameApi, DevicePortalPlatforms.XboxOne, friendlyOperatingSystemVersion, HttpMethods.Get);

            Task<string> getNameTask = TestHelpers.Portal.GetDeviceNameAsync();
            getNameTask.Wait();

            Assert.AreEqual(operatingSystemVersion, TestHelpers.Portal.OperatingSystemVersion);
            Assert.AreEqual(DevicePortalPlatforms.XboxOne, TestHelpers.Portal.Platform);
            Assert.AreEqual("XboxOneName", getNameTask.Result);
        }
    }
}
