//----------------------------------------------------------------------------------------------
// <copyright file="WindowsErrorReportingTests.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Microsoft.Tools.WindowsDevicePortal.DevicePortal;

namespace Microsoft.Tools.WindowsDevicePortal.Tests.Core
{
    /// <summary>
    /// Test class for Windows Error Reporting (WER) APIs.
    /// </summary>
    [TestClass]
    public class WindowsErrorReportingTests : BaseTests
    {
        /// <summary>
        /// Basic test of GET method for getting a list of Windows Error Reporting (WER) reports.
        /// </summary>
        [TestMethod]
        public void GetWindowsErrorReportsTest()
        {
            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.WindowsErrorReportsApi, HttpMethods.Get);

            Task<WerDeviceReports> getWerReportsTask = TestHelpers.Portal.GetWindowsErrorReportsAsync();
            getWerReportsTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, getWerReportsTask.Status);

            List<WerUserReports> deviceReports = getWerReportsTask.Result.UserReports;
            Assert.AreEqual(6, deviceReports.Count);
            deviceReports.ForEach(userReport =>
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(userReport.UserName));
                userReport.Reports.ForEach(report =>
                {
                    Assert.AreNotEqual(0, report.CreationTime);
                    Assert.IsFalse(string.IsNullOrWhiteSpace(report.Name));
                    switch (report.Type.ToLower())
                    {
                        case "queue":
                        case "archive":
                            break;
                        default:
                            Assert.Fail($"Expected the report type to be 'Queue' or 'Archive'. Actual value is '{report.Type}'.");
                            break;
                    }
                });
            });
        }

        /// <summary>
        /// Basic test of GET method for getting a list of Windows Error Reporting (WER) report files.
        /// </summary>
        [TestMethod]
        public void GetWindowsErrorReportFilesTest()
        {
            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.WindowsErrorReportingFilesApi, HttpMethods.Get);

            Task<WerFiles> getWerReportingFilesTask = TestHelpers.Portal.GetWindowsErrorReportingFileListAsync(string.Empty, string.Empty, string.Empty);
            getWerReportingFilesTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, getWerReportingFilesTask.Status);

            List<WerFileInformation> list = getWerReportingFilesTask.Result.Files;
            Assert.AreEqual(2, list.Count);
            list.ForEach(file =>
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(file.Name));
                Assert.AreNotEqual(0, file.Size);
            });
        }
    }
}
