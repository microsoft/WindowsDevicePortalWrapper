//----------------------------------------------------------------------------------------------
// <copyright file="XboxAppDeploymentTests.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Tools.WindowsDevicePortal.Tests
{
    /// <summary>
    /// Test class for XboxAppDeployment APIs
    /// </summary>
    [TestClass]
    public class XboxAppDeploymentTests
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
        /// First basic test.
        /// </summary>
        [TestMethod]
        public void XboxAppDeploymentTest()
        {
            TestHelpers.MockHttpWrapper.AddMockResponse(DevicePortal.RegisterPackageApi);

            Task registerTask = TestHelpers.Portal.RegisterApplication("SomeLooseFolder");
            registerTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, registerTask.Status);
        }
    }
}
