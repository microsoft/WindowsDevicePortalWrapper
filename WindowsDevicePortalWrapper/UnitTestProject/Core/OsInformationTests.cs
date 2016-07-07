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
    public class OsInformationTests
    {
        /// <summary>
        /// Shared class initialization (establishes Mock Device Portal Connection).
        /// </summary>
        /// <param name="context">Test Context.</param>
        [ClassInitialize]
        public static void EstablishMockConnection(TestContext context)
        {
            TestHelpers.EstablishMockConnection();
        }

        /// <summary>
        /// Cleanup which should run after each test.
        /// </summary>
        [TestCleanup]
        public void TestCleanup()
        {
            TestHelpers.MockHttpWrapper.ResetMockResponses();
        }

        /// <summary>
        /// First basic test.
        /// </summary>
        [TestMethod]
        public void OsInformationTest()
        {
            TestHelpers.MockHttpWrapper.AddMockResponse(DevicePortal.MachineNameApi);

            Task<string> getNameTask = TestHelpers.Portal.GetDeviceName();
            getNameTask.Wait();

            Assert.IsNotNull(TestHelpers.Portal.OperatingSystemVersion);
            Assert.IsNotNull(TestHelpers.Portal.Platform);
            Assert.IsNotNull(getNameTask.Result);
        }
    }
}
