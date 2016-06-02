// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE.TXT in the project root license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal
{
    public partial class DevicePortal
    {
        private static readonly String _userApi = "ext/user";

        public async Task<UserList> GetXboxLiveUsers()
        {
            return await Get<UserList>(_userApi);
        }

        public async Task UpdateXboxLiveUsers(UserList users)
        {
            await Put(_userApi, users);
        }
    }

    #region Data contract

    [DataContract]
    public class UserList
    {
        public UserList()
        {
            Users = new List<UserInfo>();
        }

        [DataMember(Name = "Users")]
        public List<UserInfo> Users { get; set; }

        public override string ToString()
        {
            string userString = "";
            foreach (UserInfo user in Users)
            {
                userString += "User: " + user + "\n";
            }
            return userString;
        }

        public void Add(UserInfo newUser)
        {
            Users.Add(newUser);
        }
    }

    [DataContract]
    public class UserInfo
    {
        [DataMember(Name = "UserId", EmitDefaultValue = false)]
        public UInt32? UserId { get; set; }

        [DataMember(Name = "EmailAddress", EmitDefaultValue = false)]
        public string EmailAddress { get; set; }

        [DataMember(Name = "AutoSignIn", EmitDefaultValue = false)]
        public bool? AutoSignIn { get; set; }

        [DataMember(Name = "Gamertag", EmitDefaultValue = false)]
        public string Gamertag { get; set; }

        [DataMember(Name = "SignedIn", EmitDefaultValue = false)]
        public bool? SignedIn { get; set; }

        [DataMember(Name = "SponsoredUser", EmitDefaultValue = false)]
        public bool? SponsoredUser { get; set; }

        [DataMember(Name = "XboxUserId", EmitDefaultValue = false)]
        public string XboxUserId { get; set; }

        public override string ToString()
        {
            return EmailAddress + " : " + Gamertag;
        }
    }
    #endregion // Data contract
}
