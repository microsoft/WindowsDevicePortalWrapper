//----------------------------------------------------------------------------------------------
// <copyright file="AppFileExplorerTests.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Microsoft.Tools.WindowsDevicePortal.DevicePortal;

namespace Microsoft.Tools.WindowsDevicePortal.Tests
{
    /// <summary>
    /// Test class for AppFileExplorer APIs
    /// </summary>
    [TestClass]
    public class AppFileExplorerTests : BaseTests
    {
        /// <summary>
        /// Simple test which gets a response with a couple of known folders
        /// and verifies they are returned correctly.
        /// </summary>
        [TestMethod]
        public void AppFileExplorerGetKnownFolderTest()
        {
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            HttpContent content = new StringContent(
                "{\"KnownFolders\" : [\"KnownFolderOne\",\"KnownFolderTwo\",\"KnownFolderThree\"]}",
                System.Text.Encoding.UTF8,
                "application/json");

            response.Content = content;

            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.KnownFoldersApi, response, HttpMethods.Get);

            Task<KnownFolders> getKnownFoldersTask = TestHelpers.Portal.GetKnownFoldersAsync();
            getKnownFoldersTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, getKnownFoldersTask.Status);

            List<string> knownFolders = getKnownFoldersTask.Result.Folders;

            // Check some known things about this response.
            Assert.AreEqual(3, knownFolders.Count);
            Assert.AreEqual("KnownFolderOne", knownFolders[0]);
            Assert.AreEqual("KnownFolderTwo", knownFolders[1]);
            Assert.AreEqual("KnownFolderThree", knownFolders[2]);
        }

        /// <summary>
        /// Tests getting the contents of a folder that is not for
        /// an application (eg developer folder, documents folder).
        /// </summary>
        [TestMethod]
        public void AppFileExplorerGetFolderContentsTest()
        {
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            HttpContent content = new StringContent(
                "{ \"Items\" : [{\"CurrentDir\" : \"\", \"DateCreated\" : 131117780894053204, \"Id\" : \"FolderOne\", \"Name\" : \"FolderOne\", \"SubPath\" : \"\\\\FolderOne\", \"Type\" : 16}," +
                "{\"CurrentDir\" : \"\", \"DateCreated\" : 131098833024438070, \"Id\" : \"FolderTwo\", \"Name\" : \"FolderTwo\", \"SubPath\" : \"\\\\FolderTwo\", \"Type\" : 16}," +
                "{\"CurrentDir\" : \"\", \"DateCreated\" : 131117780895076062, \"FileSize\" : 2985, \"Id\" : \"fakefile.xml\", \"Name\" : \"fakefile.xml\", \"SubPath\" : \"\", \"Type\" : 32}]}",
                System.Text.Encoding.UTF8,
                "application/json");

            response.Content = content;

            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.GetFilesApi, response, HttpMethods.Get);

            Task<FolderContents> getFolderContentsTask = TestHelpers.Portal.GetFolderContentsAsync("KnownFolderOne");
            getFolderContentsTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, getFolderContentsTask.Status);

            List<FileOrFolderInformation> directoryContents = getFolderContentsTask.Result.Contents;

            // Check some known things about this response.
            Assert.AreEqual(3, directoryContents.Count);

            Assert.AreEqual(string.Empty, directoryContents[0].CurrentDir);
            Assert.AreEqual(131117780894053204, directoryContents[0].DateCreated);
            Assert.AreEqual("FolderOne", directoryContents[0].Id);
            Assert.AreEqual("FolderOne", directoryContents[0].Name);
            Assert.AreEqual("\\FolderOne", directoryContents[0].SubPath);
            Assert.AreEqual(16, directoryContents[0].Type);

            Assert.AreEqual(string.Empty, directoryContents[1].CurrentDir);
            Assert.AreEqual(131098833024438070, directoryContents[1].DateCreated);
            Assert.AreEqual("FolderTwo", directoryContents[1].Id);
            Assert.AreEqual("FolderTwo", directoryContents[1].Name);
            Assert.AreEqual("\\FolderTwo", directoryContents[1].SubPath);
            Assert.AreEqual(16, directoryContents[1].Type);

            Assert.AreEqual(string.Empty, directoryContents[2].CurrentDir);
            Assert.AreEqual(131117780895076062, directoryContents[2].DateCreated);
            Assert.AreEqual("fakefile.xml", directoryContents[2].Id);
            Assert.AreEqual("fakefile.xml", directoryContents[2].Name);
            Assert.AreEqual(string.Empty, directoryContents[2].SubPath);
            Assert.AreEqual(32, directoryContents[2].Type);
            Assert.AreEqual(2985, directoryContents[2].SizeInBytes);
        }

        /// <summary>
        /// Tests download method for downloading a file.
        /// </summary>
        [TestMethod]
        public void AppFileExplorerDownloadFileTest()
        {
            Stream stream = new FileStream("MockData\\Defaults\\api_os_devicefamily_Default.dat", FileMode.Open, FileAccess.Read);
            long fileLength = stream.Length;

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            HttpContent content = new StreamContent(stream);
            response.Content = content;

            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.GetFileApi, response, HttpMethods.Get);

            Task<Stream> getFileTask = TestHelpers.Portal.GetFileAsync("knownfolder", "FileToDownload.txt", "SubFolder\\SubFolder2");
            getFileTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, getFileTask.Status);

            Assert.AreEqual(fileLength, getFileTask.Result.Length);
        }

        /// <summary>
        /// Tests upload method for uploading a file.
        /// </summary>
        [TestMethod]
        public void AppFileExplorerUploadFileTest()
        {
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);

            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.GetFileApi, response, HttpMethods.Post);

            Task uploadFileTask = TestHelpers.Portal.UploadFileAsync("knownfolder", "MockData\\Defaults\\api_os_devicefamily_Default.dat", "SubFolder\\SubFolder2");
            uploadFileTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, uploadFileTask.Status);
        }

        /// <summary>
        /// Tests failure of method for uploading a file when the file
        /// doesn't exist.
        /// </summary>
        [TestMethod]
        public void AppFileExplorerUploadFileTest_Failure()
        {
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);

            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.GetFileApi, response, HttpMethods.Post);

            try
            {
                Task uploadFileTask = TestHelpers.Portal.UploadFileAsync("knownfolder", "NonExistentFilePath\\NonExistentFile.txt", "SubFolder\\SubFolder2");
                uploadFileTask.Wait();

                Assert.Fail("Should not have succeeded if uploading a file which doesn't exist.");
            }
            catch (AggregateException e)
            {
                Assert.IsInstanceOfType(e.InnerException, typeof(IOException));
            }
        }

        /// <summary>
        /// Tests delete method for deleting a file.
        /// </summary>
        [TestMethod]
        public void AppFileExplorerDeleteFileTest()
        {
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);

            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.GetFileApi, response, HttpMethods.Delete);

            Task deleteFileTask = TestHelpers.Portal.DeleteFileAsync("knownfolder", "FileToDelete.txt", "SubFolder\\SubFolder2");
            deleteFileTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, deleteFileTask.Status);
        }

        /// <summary>
        /// Tests rename method for renaming a file
        /// </summary>
        [TestMethod]
        public void AppFileExplorerRenameFileTest()
        {
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);

            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.RenameFileApi, response, HttpMethods.Post);

            Task renameFileTask = TestHelpers.Portal.RenameFileAsync("knownfolder", "FileToRename.txt", "NewFileName.txt", "SubFolder\\SubFolder2");
            renameFileTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, renameFileTask.Status);
        }
    }
}
