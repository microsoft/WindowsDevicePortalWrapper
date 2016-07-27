//----------------------------------------------------------------------------------------------
// <copyright file="BaseTests.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Microsoft.Tools.WindowsDevicePortal.DevicePortal;

namespace Microsoft.Tools.WindowsDevicePortal.Tests
{
    /// <summary>
    ///  Base class to handle establishing a connecting and resetting mocks
    ///  which other test classes can inherit from.
    /// </summary>
    public abstract class BaseTests
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseTests"/> class.
        /// This allows us to do common initialization since ClassInitialize
        /// and AssemblyInitialize methods are not called in the base class
        /// and only in derived classes.
        /// </summary>
        public BaseTests()
        {
            TestHelpers.EstablishMockConnection(this.PlatformType, this.FriendlyOperatingSystemVersion);
        }

        /// <summary>
        /// Gets the overridable Platform type.
        /// </summary>
        protected virtual DevicePortalPlatforms PlatformType
        {
            get
            {
                return DevicePortalPlatforms.Unknown;
            }
        }

        /// <summary>
        /// Gets the overridable OS Version.
        /// </summary>
        protected virtual string OperatingSystemVersion
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the overridable friendly OS Version used to find the file names of mock data.
        /// </summary>
        protected virtual string FriendlyOperatingSystemVersion
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Cleanup which should run after each test.
        /// </summary>
        [TestCleanup]
        public void TestCleanup()
        {
            TestHelpers.MockHttpResponder.ResetMockResponses();
        }
    }
}