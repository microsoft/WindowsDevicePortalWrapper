//----------------------------------------------------------------------------------------------
// <copyright file="UserManagementTests.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Microsoft.Tools.WindowsDevicePortal.DevicePortal;

namespace Microsoft.Tools.WindowsDevicePortal.Tests
{
    /// <summary>
    /// Test class for UserManagement APIs
    /// </summary>
    [TestClass]
    public class UserManagementTests : BaseTests
    {
        /// <summary>
        /// Basic test of the GET method. Gets a mock list of users
        /// and verifies it comes back as expected from the raw response
        /// content.
        /// </summary>
        [TestMethod]
        public void GetXboxLiveUserListTest()
        {
            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.XboxLiveUserApi, HttpMethods.Get);

            Task<UserList> getUserTask = TestHelpers.Portal.GetXboxLiveUsers();
            getUserTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, getUserTask.Status);

            List<UserInfo> users = getUserTask.Result.Users;

            // Check some known things about this response.
            Assert.AreEqual(2, users.Count);

            Assert.AreEqual("fakeMsa@fakedomain.com", users[0].EmailAddress);
            Assert.AreEqual("fakeGamertag", users[0].Gamertag);
            Assert.AreEqual(16u, users[0].UserId);
            Assert.AreEqual("12345667890123456", users[0].XboxUserId);
            Assert.AreEqual(true, users[0].SignedIn);
            Assert.AreEqual(false, users[0].AutoSignIn);

            Assert.IsNull(users[1].EmailAddress);
            Assert.AreEqual("fakeGamertag(1)", users[1].Gamertag);
            Assert.AreEqual(1u, users[1].UserId);
            Assert.AreEqual("7036874539097560", users[1].XboxUserId);
            Assert.AreEqual(true, users[1].SponsoredUser);
            Assert.AreEqual(true, users[1].SignedIn);
        }

        /// <summary>
        /// Basic test of the PUT method. Creates a UserList
        /// object and passes that to the server.
        /// </summary>
        [TestMethod]
        public void UpdateXboxLiveUsersTest()
        {
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.NoContent);
            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.XboxLiveUserApi, response, HttpMethods.Put);

            UserList users = new UserList();
            UserInfo user = new UserInfo();
            user.EmailAddress = "fakeMsa@fakeDomain.com";
            user.Password = "someFakePassword!";
            user.SignedIn = true;
            users.Add(user);

            Task updateUsersTask = TestHelpers.Portal.UpdateXboxLiveUsers(users);
            updateUsersTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, updateUsersTask.Status);
        }

        /// <summary>
        /// Tests the failure case of trying to add a sponsored user
        /// when the maximum number is already on the console.
        /// </summary>
        [TestMethod]
        public void AddSponsoredUserTest_Failure()
        {
            HttpResponseMessage response = new HttpResponseMessage((HttpStatusCode)422);
            HttpContent content = new StringContent(
                "{\"ErrorCode\":-2136866553,\"ErrorMessage\":\"The maximum number of sponsored users is already signed in.\"}", 
                System.Text.Encoding.UTF8, 
                "application/json");

            response.Content = content;

            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.XboxLiveUserApi, response, HttpMethods.Put);

            UserList users = new UserList();
            UserInfo user = new UserInfo();
            user.SponsoredUser = true;
            users.Add(user);

            try
            {
                Task updateUsersTask = TestHelpers.Portal.UpdateXboxLiveUsers(users);
                updateUsersTask.Wait();

                Assert.Fail("Expected an exception due to mock responder returning failure HRESULT.");
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is AggregateException);
                Assert.IsNotNull(e.InnerException);
                Assert.IsTrue(e.InnerException is DevicePortalException);

                DevicePortalException exception = e.InnerException as DevicePortalException;

                Assert.AreEqual(-2136866553, exception.HResult);
                Assert.AreEqual("The maximum number of sponsored users is already signed in.", exception.Reason);
            }
        }
    }
}
