﻿//----------------------------------------------------------------------------------------------
// <copyright file="TestHelpers.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Tools.WindowsDevicePortal.Tests
{
    /// <summary>
    /// Static helpers for tests.
    /// </summary>
    public static class TestHelpers
    {
        /// <summary>
        /// Gets a static portal object used by all tests.
        /// </summary>
        public static DevicePortal Portal { get; private set; }

        /// <summary>
        /// Gets a static mock HTTP wrapper for setting response overrides.
        /// </summary>
        public static MockHttpResponder MockHttpResponder { get; private set; }

        /// <summary>
        /// Helper for establishing a mock connection to a DevicePortal object.
        /// </summary>
        public static void EstablishMockConnection()
        {
            TestHelpers.MockHttpResponder = new MockHttpResponder();
            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.DeviceFamilyApi);
            TestHelpers.MockHttpResponder.AddMockResponse(DevicePortal.OsInfoApi);

            TestHelpers.Portal = new DevicePortal(new MockDevicePortalConnection());

            Task connectTask = TestHelpers.Portal.Connect(updateConnection: false);
            connectTask.Wait();

            Assert.AreEqual(HttpStatusCode.OK, TestHelpers.Portal.ConnectionHttpStatusCode);
        }
    }
}