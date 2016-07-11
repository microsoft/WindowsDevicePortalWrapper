//----------------------------------------------------------------------------------------------
// <copyright file="BaseTests.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            TestHelpers.EstablishMockConnection();
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