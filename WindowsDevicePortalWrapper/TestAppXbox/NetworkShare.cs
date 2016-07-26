//----------------------------------------------------------------------------------------------
// <copyright file="NetworkShare.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Runtime.InteropServices;

namespace XboxWdpDriver
{
    /// <summary>
    /// Helper class for connecting to Network Shares
    /// </summary>
    public static class NetworkShare
    {
        /// <summary>
        /// Resource type constant value.
        /// </summary>
        private const int ResourceTypeDisk = 0x00000001;

        /// <summary>
        /// Connects to the remote share
        /// </summary>
        /// <param name="uri">The URI for the network share.</param>
        /// <param name="username">The username for accessing the share.</param>
        /// <param name="password">The password for accessing the share.</param>
        /// <returns>Result of the connection, 0 for success.</returns>
        public static int ConnectToShare(string uri, string username, string password)
        {
            //Create netresource and point it at the share
            NETRESOURCE nr;

            nr.Scope = 0;
            nr.Type = ResourceTypeDisk;
            nr.DisplayType = 0;
            nr.Usage = 0;
            nr.LocalName = string.Empty;
            nr.RemoteName = uri;
            nr.Comment = string.Empty;
            nr.Provider = string.Empty;

            // Create the share
            return WNetUseConnection(IntPtr.Zero, ref nr, password, username, 0, null, null, null);
        }

        /// <summary>
        /// Remove the share from cache.
        /// </summary>
        /// <param name="uri">The URI for the network share.</param>
        /// <param name="force">Whether to force the disconnect.</param>
        /// <returns>Result of the connection, 0 for success.</returns>
        public static int DisconnectFromShare(string uri, bool force)
        {
            //remove the share
            return WNetCancelConnection(uri, force);
        }

        /// <summary>
        /// Makes a connection to a network resource.
        /// </summary>
        /// <param name="handleToOwner">Handle to a window that the provider of network resources can use as an owner window for dialog boxes.</param>
        /// <param name="netResource">Pointer to a NETRESOURCE structure that specifies details of the proposed connection. </param>
        /// <param name="password">Pointer to a constant null-terminated string that specifies a password to be used in making the network connection. </param>
        /// <param name="userName">Pointer to a constant null-terminated string that specifies a user name for making the connection. </param>
        /// <param name="flags">Set of bit flags describing the connection.</param>
        /// <param name="accessName">Pointer to a buffer that receives system requests on the connection.</param>
        /// <param name="bufferSize">Pointer to a variable that specifies the size of the lpAccessName buffer, in characters.</param>
        /// <param name="result">Pointer to a variable that receives additional information about the connection.</param>
        /// <returns>If the function succeeds, the return value is 0.</returns>
        [DllImport("Mpr.dll")]
        private static extern int WNetUseConnection(
            IntPtr handleToOwner,
            ref NETRESOURCE netResource,
            string password,
            string userName,
            int flags,
            string accessName,
            string bufferSize,
            string result);

        /// <summary>
        /// Cancels an existing network connection.
        /// </summary>
        /// <param name="uri">Pointer to a constant null-terminated string that specifies the name of either the redirected local device or the remote network resource to disconnect from. </param>
        /// <param name="shouldForce">Specifies whether or not the disconnection should occur if there are open files or jobs on the connection.</param>
        /// <returns>If the function succeeds, the return value is 0.</returns>
        [DllImport("Mpr.dll")]
        private static extern int WNetCancelConnection(
            string uri,
            bool shouldForce);

        /// <summary>
        /// Specifies details of a proposed network connection.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct NETRESOURCE
        {
            /// <summary>
            /// Specifies the scope of the resource to connect to. Unused in the wrapper.
            /// </summary>
            public int Scope;

            /// <summary>
            /// Specifies the type of resource to connect to.
            /// </summary>
            public int Type;

            /// <summary>
            /// Specifies the display type of resource to connect to. Unused in the wrapper.
            /// </summary>
            public int DisplayType;

            /// <summary>
            /// Specifies the usage of resource to connect to. Unused in the wrapper.
            /// </summary>
            public int Usage;

            /// <summary>
            /// Specifies the local name of resource to connect to. Unused in the wrapper.
            /// </summary>
            public string LocalName;

            /// <summary>
            /// Specifies the remote name of resource to connect to.
            /// </summary>
            public string RemoteName;

            /// <summary>
            /// Specifies a comment about the connection. Unused in the wrapper.
            /// </summary>
            public string Comment;

            /// <summary>
            /// Specifies the provider. Unused in the wrapper.
            /// </summary>
            public string Provider;
        }
    }
}