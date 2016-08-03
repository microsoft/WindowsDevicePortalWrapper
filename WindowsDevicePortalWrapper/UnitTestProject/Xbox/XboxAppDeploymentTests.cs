//----------------------------------------------------------------------------------------------
// <copyright file="XboxAppDeploymentTests.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Microsoft.Tools.WindowsDevicePortal.DevicePortal;

namespace Microsoft.Tools.WindowsDevicePortal.Tests
{
    /// <summary>
    /// Test class for XboxAppDeployment APIs
    /// </summary>
    [TestClass]
    public class XboxAppDeploymentTests : BaseTests
    {
        /// <summary>
        /// First basic test.
        /// </summary>
        [TestMethod]
        public void XboxAppDeploymentTest()
        {
            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.RegisterPackageApi, HttpMethods.Post);

            Task registerTask = TestHelpers.Portal.RegisterApplication("SomeLooseFolder");
            registerTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, registerTask.Status);
        }
    }
}
