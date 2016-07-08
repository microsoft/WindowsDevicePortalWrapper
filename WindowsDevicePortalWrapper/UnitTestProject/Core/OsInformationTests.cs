//----------------------------------------------------------------------------------------------
// <copyright file="OsInformationTests.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.MachineNameApi);

            Task<string> getNameTask = TestHelpers.Portal.GetDeviceName();
            getNameTask.Wait();

            Assert.IsNotNull(TestHelpers.Portal.OperatingSystemVersion);
            Assert.IsNotNull(TestHelpers.Portal.Platform);
            Assert.IsNotNull(getNameTask.Result);
        }
    }
}
