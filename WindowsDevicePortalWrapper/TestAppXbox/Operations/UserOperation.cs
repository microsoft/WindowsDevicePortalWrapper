using System;
using Microsoft.Tools.WindowsDevicePortal;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace TestApp
{
    internal class UserOperation
    {
        private const String XblUserUsageMessage = "Usage:\n" +
            "  list\n" +
            "        Lists all Xbox Live Users on the console\n" +
            "  signin <msa or id> [<password>]\n" +
            "        Signs in the given user, adding them to the console if necessary\n" +
            "  signout <msa or id>\n" +
            "        Signs the given user out of the console\n" +
            "  addsponsored\n" +
            "        Adds a sponsored user to the console\n" +
            "  autosignin <msa or id> <on or off>\n" +
            "        Turns autosignin on or off for a given user\n" +
            "  delete <msa or id>\n" +
            "        Deletes the given user from the console\n";

        internal static void HandleOperation(DevicePortal portal, List<String> operationArgs)
        {
            if (operationArgs.Count == 0)
            {
                Console.WriteLine(XblUserUsageMessage);
                return;
            }

            String arg = operationArgs[0].ToLower();

            if (arg.Equals("list"))
            {
                Task<UserList> getUsers = portal.GetXboxLiveUsers();

                getUsers.Wait();
                Console.WriteLine(getUsers.Result);
            }
            else if (arg.Equals("addsponsored"))
            {
                UserInfo user = new UserInfo();

                user.SponsoredUser = true;
                user.SignedIn = true;

                UserList userList = new UserList();
                userList.Add(user);
                Task updateUsers = portal.UpdateXboxLiveUsers(userList);
                updateUsers.Wait();
            }
            else if (arg.Equals("signin") || arg.Equals("signout") || arg.Equals("delete") || arg.Equals("autosignin"))
            {
                if (operationArgs.Count < 2)
                {
                    throw new Exception("Not enough params to user operation.\n" + XblUserUsageMessage);
                }

                UserInfo user = new UserInfo();

                String userIdentifier = operationArgs[1];
                uint userId;
                if (UInt32.TryParse(userIdentifier, out userId))
                {
                    user.UserId = userId;
                }
                else
                {
                    user.EmailAddress = userIdentifier;
                }

                if (arg.Equals("signin"))
                {
                    if (operationArgs.Count >= 3)
                    {
                        user.Password = operationArgs[2];
                    }

                    user.SignedIn = true;
                }
                else if (arg.Equals("signout"))
                {
                    user.SignedIn = false;
                }
                else if (arg.Equals("delete"))
                {
                    user.Delete = true;
                }
                else if (arg.Equals("autosignin"))
                {
                    if (operationArgs.Count >= 3)
                    {
                        String autoSignin = operationArgs[2].ToLower();
                        if (autoSignin.Equals("on"))
                        {
                            user.AutoSignIn = true;
                        }
                        else if (autoSignin.Equals("off"))
                        {
                            user.AutoSignIn = false;
                        }
                        else
                        {
                            throw new Exception("Final param to autosignin must be on or off.\n" + XblUserUsageMessage);
                        }
                    }
                    else
                    {
                        throw new Exception("Not enough params to autosignin operation.\n" + XblUserUsageMessage);
                    }
                }

                UserList userList = new UserList();
                userList.Add(user);
                Task updateUsers = portal.UpdateXboxLiveUsers(userList);
                updateUsers.Wait();
            }
            else
            {
                throw new Exception(String.Format("Unrecognized argument: {0}\n{1}", arg, XblUserUsageMessage));
            }
        }
    }
}