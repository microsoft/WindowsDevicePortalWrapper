//----------------------------------------------------------------------------------------------
// <copyright file="GenericDevicePortalConnectionFactory.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------
using System.Security;
using Microsoft.Tools.WindowsDevicePortal;

namespace DeviceLab
{
    /// <summary>
    /// Fractory creates a generic IDevicePortalConnection instance
    /// </summary>
    public class GenericDevicePortalConnectionFactory : IDevicePortalConnectionFactory
    {
        /// <summary>
        /// Gets a friendly name for the GenericDevicePortalConnectionFactory that can be presented to the user
        /// </summary>
        public string Name
        {
            get { return "Generic Device"; }
        }

        /// <summary>
        /// Create an instance of GenericDevicePortalConnectionFactory
        /// </summary>
        /// <param name="address">IP address to use for connectin</param>
        /// <param name="userName">User name to use for authenticating</param>
        /// <param name="password">Password to use for authenticating</param>
        /// <returns>The new IDevicePortalConnection instance</returns>
        public IDevicePortalConnection CreateConnection(string address, string userName, SecureString password)
        {
            return new DefaultDevicePortalConnection(address, userName, password);
        }
    }
}
