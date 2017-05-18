//----------------------------------------------------------------------------------------------
// <copyright file="MockDevicePortalConnection.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using Microsoft.Tools.WindowsDevicePortal;
using static Microsoft.Tools.WindowsDevicePortal.DevicePortal;

namespace Microsoft.Tools.WindowsDevicePortal.Tests
{
    /// <summary>
    /// Mock implementation of IDevicePortalConnection
    /// </summary>
    public class MockDevicePortalConnection : IDevicePortalConnection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MockDevicePortalConnection"/> class.
        /// </summary>
        public MockDevicePortalConnection()
        {
        }

        /// <summary>
        /// Gets Connection property
        /// </summary>
        public Uri Connection
        {
            get
            {
                return new Uri("http://localhost");
            }
        }

        /// <summary>
        /// Gets Web Socket Connection property
        /// </summary>
        public Uri WebSocketConnection
        {
            get
            {
                return new Uri("ws://localhost");
            }
        }

        /// <summary>
        /// Gets Credentials property
        /// </summary>
        public NetworkCredential Credentials
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets device family
        /// </summary>
        public string Family
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets device OS Info
        /// </summary>
        public OperatingSystemInformation OsInfo
        {
            get;
            set;
        }

        /// <summary>
        /// The Mock will never update the connection.
        /// </summary>
        /// <param name="requiresHttps">https required</param>
        public void UpdateConnection(bool requiresHttps)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///  The Mock will never update the connection.
        /// </summary>
        /// <param name="ipConfig">Object that describes the current network configuration.</param>
        /// <param name="requiresHttps">True if an https connection is required, false otherwise.</param>
        /// <param name="preservePort">True if the previous connection's port is to continue to be used, false otherwise.</param>
        public void UpdateConnection(
            IpConfiguration ipConfig, 
            bool requiresHttps,
            bool preservePort)
        {
            throw new NotImplementedException();
        }
    }
}
