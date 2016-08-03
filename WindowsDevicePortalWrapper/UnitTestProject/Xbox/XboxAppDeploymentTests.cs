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
        /// Basic test of the register API.
        /// </summary>
        [TestMethod]
        public void XboxAppRegisterTest()
        {
            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.RegisterPackageApi, HttpMethods.Post);

            Task registerTask = TestHelpers.Portal.RegisterApplication("SomeLooseFolder");
            registerTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, registerTask.Status);
        }

        /// <summary>
        /// Basic test of the folder upload API.
        /// </summary>
        [TestMethod]
        public void XboxAppUploadFolderTest()
        {
            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.UploadPackageFolderApi, HttpMethods.Post);

            Task uploadTask = TestHelpers.Portal.UploadPackageFolder("MockData\\XboxOne", "DestinationLooseFolder");
            uploadTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, uploadTask.Status);
        }
    }
}
