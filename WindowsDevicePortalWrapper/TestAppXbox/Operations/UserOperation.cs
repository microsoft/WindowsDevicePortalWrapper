using System;
using Microsoft.Tools.WindowsDevicePortal;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace TestApp
{
    internal class UserOperation
    {
        private const String XblUserUsageMessage = "Usage:\n" +
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

        internal static void HandleOperation(DevicePortal portal, ParameterHelper parameters)
        {
            if (parameters.HasFlag(ParameterHelper.HelpFlag))
            {
                Console.WriteLine(XblUserUsageMessage);
                return;
            }

            String opType = parameters.GetParameterValue("subop");

            if (String.IsNullOrWhiteSpace(opType))
            {
                Console.WriteLine("Missing subop parameter");
                Console.WriteLine();
                Console.WriteLine(XblUserUsageMessage);
                return;
            }

            opType = opType.ToLowerInvariant();

            if (opType.Equals("list"))
            {
                Task<UserList> getUsers = portal.GetXboxLiveUsers();

                getUsers.Wait();
                Console.WriteLine(getUsers.Result);
            }
            else if (opType.Equals("addsponsored"))
            {
                UserInfo user = new UserInfo();

                user.SponsoredUser = true;
                user.SignedIn = true;

                UserList userList = new UserList();
                userList.Add(user);
                Task updateUsers = portal.UpdateXboxLiveUsers(userList);
                updateUsers.Wait();
            }
            else if (opType.Equals("signin") || opType.Equals("signout") || opType.Equals("delete") || opType.Equals("autosignin"))
            {
                UserInfo user = new UserInfo();

                if (parameters.HasParameter("id"))
                {
                    uint userId = 0;
                    if (!UInt32.TryParse(parameters.GetParameterValue("id"), out userId))
                    {
                        Console.WriteLine(String.Format("Failed to parse id to an unsigned integer: {0}", parameters.GetParameterValue("id")));
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

                if (opType.Equals("signin"))
                {
                    // Optional password (only used on first signin)
                    user.Password = parameters.GetParameterValue("msapwd");

                    user.SignedIn = true;
                }
                else if (opType.Equals("signout"))
                {
                    user.SignedIn = false;
                }
                else if (opType.Equals("delete"))
                {
                    user.Delete = true;
                }
                else if (opType.Equals("autosignin"))
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
                Task updateUsers = portal.UpdateXboxLiveUsers(userList);
                updateUsers.Wait();
            }
            else
            {
                Console.WriteLine(String.Format("Unrecognized subop: {0}", opType));
                Console.WriteLine();
                Console.WriteLine(XblUserUsageMessage);
                return;
            }
        }
    }
}