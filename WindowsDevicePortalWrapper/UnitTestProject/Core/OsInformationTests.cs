//----------------------------------------------------------------------------------------------
// <copyright file="OsInformationTests.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Microsoft.Tools.WindowsDevicePortal.DevicePortal;

namespace Microsoft.Tools.WindowsDevicePortal.Tests
{
    /// <summary>
    /// Test class for OsInformation APIs
    /// </summary>
    [TestClass]
    public class OsInformationTests : BaseTests
    {
        /// <summary>
        /// First basic test.
        /// </summary>
        [TestMethod]
        public void OsInformationTest()
        {
            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.MachineNameApi, HttpMethods.Get);

            Task<string> getNameTask = TestHelpers.Portal.GetDeviceNameAsync();
            getNameTask.Wait();

            Assert.IsNotNull(TestHelpers.Portal.OperatingSystemVersion);
            Assert.IsNotNull(TestHelpers.Portal.Platform);
            Assert.IsNotNull(getNameTask.Result);
        }
    }
}
