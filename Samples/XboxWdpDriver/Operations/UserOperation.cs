//----------------------------------------------------------------------------------------------
// <copyright file="UserOperation.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Microsoft.Tools.WindowsDevicePortal;
using static Microsoft.Tools.WindowsDevicePortal.DevicePortal;

namespace XboxWdpDriver
{
    /// <summary>
    /// Helper for Xbox Live User related operations
    /// </summary>
    public class UserOperation
    {
        /// <summary>
        /// Usage message for this operation
        /// </summary>
        private const string XblUserUsageMessage = "Usage:\n" +
            "  /subop:list\n" +
            "        Lists all Xbox Live Users on the console\n" +
            "  /subop:signin <user identifier (/msa:<msa> or /id:<id>)> [/msapwd:<password>]\n" +
            "        Signs in the given user, adding them to the console if necessary\n" +
            "  /subop:signout <user identifier (/msa:<msa> or /id:<id>)>\n" +
            "        Signs the given user out of the console\n" +
            "  /subop:addsponsored\n" +
            "        Adds a sponsored user to the console\n" +
            "  /subop:autosignin <user identifier (/msa:<msa> or /id:<id>)> <state (/on or /off)>\n" +
            "        Turns autosignin on or off for a given user\n" +
            "  /subop:delete <user identifier (/msa:<msa> or /id:<id>)>\n" +
            "        Deletes the given user from the console\n";

        /// <summary>
        /// Main entry point for handling a user operation
        /// </summary>
        /// <param name="portal">DevicePortal reference for communicating with the device.</param>
        /// <param name="parameters">Parsed command line parameters.</param>
        public static void HandleOperation(DevicePortal portal, ParameterHelper parameters)
        {
            if (parameters.HasFlag(ParameterHelper.HelpFlag))
            {
                Console.WriteLine(XblUserUsageMessage);
                return;
            }

            string operationType = parameters.GetParameterValue("subop");

            if (string.IsNullOrWhiteSpace(operationType))
            {
                Console.WriteLine("Missing subop parameter");
                Console.WriteLine();
                Console.WriteLine(XblUserUsageMessage);
                return;
            }

            operationType = operationType.ToLowerInvariant();

            if (operationType.Equals("list"))
            {
                Task<UserList> getUsers = portal.GetXboxLiveUsersAsync();

                getUsers.Wait();
                Console.WriteLine(getUsers.Result);
            }
            else if (operationType.Equals("addsponsored"))
            {
                UserInfo user = new UserInfo();

                user.SponsoredUser = true;
                user.SignedIn = true;

                UserList userList = new UserList();
                userList.Add(user);

                UpdateXboxLiveUsers(portal, userList);
            }
            else if (operationType.Equals("signin") || operationType.Equals("signout") || operationType.Equals("delete") || operationType.Equals("autosignin"))
            {
                UserInfo user = new UserInfo();

                if (parameters.HasParameter("id"))
                {
                    uint userId = 0;
                    if (!uint.TryParse(parameters.GetParameterValue("id"), out userId))
                    {
                        Console.WriteLine(string.Format("Failed to parse id to an unsigned integer: {0}", parameters.GetParameterValue("id")));
                        return;
                    }

                    user.UserId = userId;
                }
                else
                {
                    user.EmailAddress = parameters.GetParameterValue("msa");

                    if (user.EmailAddress == null)
                    {
                        Console.WriteLine("Must provide either msa or id to this operation");
                        Console.WriteLine();
                        Console.WriteLine(XblUserUsageMessage);
                        return;
                    }
                }

                if (operationType.Equals("signin"))
                {
                    // Optional password (only used on first signin)
                    user.Password = parameters.GetParameterValue("msapwd");

                    user.SignedIn = true;
                }
                else if (operationType.Equals("signout"))
                {
                    user.SignedIn = false;
                }
                else if (operationType.Equals("delete"))
                {
                    user.Delete = true;
                }
                else if (operationType.Equals("autosignin"))
                {
                    if (parameters.HasFlag("on"))
                    {
                        user.AutoSignIn = true;
                    }
                    else if (parameters.HasFlag("off"))
                    {
                        user.AutoSignIn = false;
                    }
                    else
                    {
                        Console.WriteLine("autosignin operation requires the state (/on or /off).");
                        Console.WriteLine();
                        Console.WriteLine(XblUserUsageMessage);
                        return;
                    }
                }

                UserList userList = new UserList();
                userList.Add(user);

                UpdateXboxLiveUsers(portal, userList);
            }
            else
            {
                Console.WriteLine(string.Format("Unrecognized subop: {0}", operationType));
                Console.WriteLine();
                Console.WriteLine(XblUserUsageMessage);
                return;
            }
        }

        /// <summary>
        ///  Helper to make the REST call and handle exceptions.
        /// </summary>
        /// <param name="portal">DevicePortal reference for communicating with the device.</param>
        /// <param name="userList">UserList object for updating the remote device.</param>
        private static void UpdateXboxLiveUsers(DevicePortal portal, UserList userList)
        {
            try
            {
                Task updateUsers = portal.UpdateXboxLiveUsersAsync(userList);
                updateUsers.Wait();
            }
            catch (AggregateException e)
            {
                if (e.InnerException is DevicePortalException)
                {
                    DevicePortalException innerException = e.InnerException as DevicePortalException;

                    Console.WriteLine(string.Format("Exception encountered: 0x{0:X} : {1}", innerException.HResult, innerException.Reason));
                }
                else if (e.InnerException is OperationCanceledException)
                {
                    Console.WriteLine("The operation was cancelled.");
                }
                else
                {
                    Console.WriteLine(string.Format("Unexpected exception encountered: {0}", e.Message));
                }

                return;
            }
        }
    }
}