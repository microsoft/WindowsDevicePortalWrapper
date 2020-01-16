﻿//----------------------------------------------------------------------------------------------
// <copyright file="CredManager.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Net;
using AdysTech.CredentialManager;

namespace XboxWdpDriver
{
    /// <summary>
    /// Manages storing and retrieving WDP creds on Windows 8 and above.
    /// </summary>
    public static class CredManager
    {
        /// <summary>
        /// Retrieves any stored credentials for this target.
        /// </summary>
        /// <param name="target">The target the credentials are stored for.</param>
        /// <param name="userName">The stored username.</param>
        /// <param name="password">The stored password.</param>
        public static void RetrieveStoredCreds(string target, ref string userName, ref string password)
        {
            try
            {
                // Set the first stored cred as our network creds.
                if (CredentialManager.GetCredentials(target) is NetworkCredential creds)
                {
                    userName = creds.UserName;
                    password = creds.Password;
                }
            }
            catch (Exception)
            {
                // Do nothing. No credentials were stored. If they are needed, REST calls will fail with Unauthorized.
            }
        }

        /// <summary>
        /// Updates the stored credentials for the target.
        /// </summary>
        /// <param name="target">The target for which to update credentials.</param>
        /// <param name="userName">The new username.</param>
        /// <param name="password">The new password.</param>
        public static void UpdateStoredCreds(string target, string userName, string password)
        {
            try
            {
                // Remove any existing stored creds for this address and add these ones.
                CredentialManager.RemoveCredentials(target);
            }
            catch (Exception)
            {
                // Do nothing. This is expected if no credentials have been previously stored
            }

            CredentialManager.SaveCredentials(target, new NetworkCredential(userName, password));
        }
    }
}
