//----------------------------------------------------------------------------------------------
// <copyright file="EtwTests.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Microsoft.Tools.WindowsDevicePortal.DevicePortal;

namespace Microsoft.Tools.WindowsDevicePortal.Tests.Core
{
    /// <summary>
    /// Test class for ETW APIs.
    /// </summary>
    [TestClass]
    public class EtwTests : BaseTests
    {
        /// <summary>
        /// Basic test of GET method for getting a list of custom registered ETW providers.
        /// </summary>
        [TestMethod]
        public void GetCustomEtwProvidersTest()
        {
            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.CustomEtwProvidersApi, HttpMethods.Get);

            Task<EtwProviders> getCustomEtwProvidersTask = TestHelpers.Portal.GetCustomEtwProvidersAsync();
            getCustomEtwProvidersTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, getCustomEtwProvidersTask.Status);

            ValidateEtwProviders(getCustomEtwProvidersTask.Result);
        }

        /// <summary>
        /// Basic test of GET method for getting a list of registered ETW providers.
        /// </summary>
        [TestMethod]
        public void GetEtwProvidersTest()
        {
            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.EtwProvidersApi, HttpMethods.Get);

            Task<EtwProviders> getEtwProvidersTask = TestHelpers.Portal.GetEtwProvidersAsync();
            getEtwProvidersTask.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, getEtwProvidersTask.Status);

            ValidateEtwProviders(getEtwProvidersTask.Result);
        }

        /// <summary>
        /// Basic test of Get methof for getting ETW events.
        /// </summary>
        [TestMethod]
        public void GetEtwEventsTest()
        {
            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.RealtimeEtwSessionApi, HttpMethods.WebSocket);

            ManualResetEvent etwEventsReceived = new ManualResetEvent(false);
            EtwEvents etwEvents = null;

            WindowsDevicePortal.WebSocketMessageReceivedEventHandler<EtwEvents> etwEventsReceivedHandler =
                delegate(DevicePortal sender, WebSocketMessageReceivedEventArgs<EtwEvents> args)
                {
                    if (args.Message != null)
                    {
                        etwEvents = args.Message;
                        etwEventsReceived.Set();
                    }
                };

            TestHelpers.Portal.RealtimeEventsMessageReceived += etwEventsReceivedHandler;

            Task startListeningForEtwEventsTask = TestHelpers.Portal.StartListeningForEtwEventsAsync();
            startListeningForEtwEventsTask.Wait();
            Assert.AreEqual(TaskStatus.RanToCompletion, startListeningForEtwEventsTask.Status);

            etwEventsReceived.WaitOne();

            Task stopListeningForEtwEventsTask = TestHelpers.Portal.StopListeningForEtwEventsAsync();
            stopListeningForEtwEventsTask.Wait();
            Assert.AreEqual(TaskStatus.RanToCompletion, stopListeningForEtwEventsTask.Status);

            TestHelpers.Portal.RealtimeEventsMessageReceived -= etwEventsReceivedHandler;

            ValidateEtwEvents(etwEvents);
        }

        /// <summary>
        /// Validate the <see cref="EtwEvents"/>  returned from the tests.
        /// </summary>
        /// <param name="etwEvents">The <see cref="EtwEvents"/> to validate.</param>
        private static void ValidateEtwEvents(EtwEvents etwEvents)
        {
            Assert.IsNotNull(etwEvents);
        }
        
        /// <summary>
        /// Validate the <see cref="EtwProviders"/> returned from the tests. 
        /// </summary>
        /// <param name="etw">The <see cref="EtwProviders"/> to validate.</param>
        private static void ValidateEtwProviders(EtwProviders etw)
        {
            Assert.IsTrue(etw.Providers.Count > 0);
            Assert.IsTrue(etw.Providers.All(etwProvider => !string.IsNullOrEmpty(etwProvider.Name)));
        }
    }
}
