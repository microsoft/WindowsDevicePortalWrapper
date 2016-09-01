//----------------------------------------------------------------------------------------------
// <copyright file="XboxDevicePortalConnectionFactory.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------
using System.Security;
using Microsoft.Tools.WindowsDevicePortal;

namespace DeviceLab
{
    /// <summary>
    /// Factory creates an IDevicePortalConnection instance that is specific to Xbox One devices
    /// </summary>
    public class XboxDevicePortalConnectionFactory : IDevicePortalConnectionFactory
    {
        /// <summary>
        /// Gets a friendly name for the XboxDevicePortalConnectionFactory that can be presented to the user
        /// </summary>
        public string Name
        {
            get { return "Xbox One"; }
        }

        /// <summary>
        /// Create an instance of XboxDevicePortalConnectionFactory
        /// </summary>
        /// <param name="address">IP address to use for connectin</param>
        /// <param name="userName">User name to use for authenticating</param>
        /// <param name="password">Password to use for authenticating</param>
        /// <returns>The new IDevicePortalConnection instance</returns>
        public IDevicePortalConnection CreateConnection(
            string address,
            string userName,
            SecureString password)
        {
            return new XboxDevicePortalConnection(address, userName, password);
        }
    }
}
