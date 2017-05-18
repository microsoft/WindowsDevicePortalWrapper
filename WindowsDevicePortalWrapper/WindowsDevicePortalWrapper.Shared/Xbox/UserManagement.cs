//----------------------------------------------------------------------------------------------
// <copyright file="UserManagement.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <content>
    /// UserManagement Wrappers
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// Endpoint for User management REST calls
        /// </summary>
        public static readonly string XboxLiveUserApi = "ext/user";

        /// <summary>
        /// Gets the Xbox Live user info for all users present on the device
        /// </summary>
        /// <returns>UserList object containing a List of UserInfo objects representing the users on the device.</returns>
        public async Task<UserList> GetXboxLiveUsersAsync()
        {
            if (this.Platform != DevicePortalPlatforms.XboxOne)
            {
                throw new NotSupportedException("This method is only supported on Xbox One.");
            }

            return await this.GetAsync<UserList>(XboxLiveUserApi);
        }

        /// <summary>
        /// Updates info for the Xbox Live users present on the device
        /// </summary>
        /// <param name="users">List of users to be updated.</param>
        /// <returns>Task for tracking async completion.</returns>
        public async Task UpdateXboxLiveUsersAsync(UserList users)
        {
            if (this.Platform != DevicePortalPlatforms.XboxOne)
            {
                throw new NotSupportedException("This method is only supported on Xbox One.");
            }

            await this.PutAsync(XboxLiveUserApi, users);
        }

        #region Data contract

        /// <summary>
        /// List of users
        /// </summary>
        [DataContract]
        public class UserList
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="UserList"/> class.
            /// </summary>
            public UserList()
            {
                this.Users = new List<UserInfo>();
            }

            /// <summary>
            ///  Gets the Users list
            /// </summary>
            [DataMember(Name = "Users")]
            public List<UserInfo> Users { get; private set; }

            /// <summary>
            /// Returns a string representation of a user list
            /// </summary>
            /// <returns>String representation of a user list</returns>
            public override string ToString()
            {
                string userString = string.Empty;
                foreach (UserInfo user in this.Users)
                {
                    userString += "User:\n" + user + "\n";
                }

                return userString;
            }

            /// <summary>
            /// Adds a new user
            /// </summary>
            /// <param name="newUser">New User to be added</param>
            public void Add(UserInfo newUser)
            {
                this.Users.Add(newUser);
            }
        }

        /// <summary>
        /// UserInfo object
        /// </summary>
        [DataContract]
        public class UserInfo
        {
            /// <summary>
            /// Gets or sets the UserId
            /// </summary>
            [DataMember(Name = "UserId", EmitDefaultValue = false)]
            public uint? UserId { get; set; }

            /// <summary>
            /// Gets or sets the EmailAddress
            /// </summary>
            [DataMember(Name = "EmailAddress", EmitDefaultValue = false)]
            public string EmailAddress { get; set; }

            /// <summary>
            /// Gets or sets the Password
            /// </summary>
            [DataMember(Name = "Password", EmitDefaultValue = false)]
            public string Password { get; set; }

            /// <summary>
            /// Gets or sets Auto sign-in for the user
            /// </summary>
            [DataMember(Name = "AutoSignIn", EmitDefaultValue = false)]
            public bool? AutoSignIn { get; set; }

            /// <summary>
            /// Gets the gamer tag
            /// </summary>
            [DataMember(Name = "Gamertag", EmitDefaultValue = false)]
            public string Gamertag { get; private set; }

            /// <summary>
            /// Gets or sets the user as signed in
            /// </summary>
            [DataMember(Name = "SignedIn", EmitDefaultValue = false)]
            public bool? SignedIn { get; set; }

            /// <summary>
            /// Gets or sets if the user should be deleted
            /// </summary>
            [DataMember(Name = "Delete", EmitDefaultValue = false)]
            public bool? Delete { get; set; }

            /// <summary>
            /// Gets or sets if this is a sponsored user
            /// </summary>
            [DataMember(Name = "SponsoredUser", EmitDefaultValue = false)]
            public bool? SponsoredUser { get; set; }

            /// <summary>
            /// Gets the Xbox User Id.
            /// </summary>
            [DataMember(Name = "XboxUserId", EmitDefaultValue = false)]
            public string XboxUserId { get; private set; }

            /// <summary>
            /// Returns a string representation of a user
            /// </summary>
            /// <returns>String representation of a user</returns>
            public override string ToString()
            {
                return "    Id: " + this.UserId + "\n" +
                        (this.SponsoredUser != true ? "    Email: " + this.EmailAddress + "\n" : "    Sponsored User\n") +
                        "    Gamertag: " + this.Gamertag + "\n" +
                        "    XboxUserId: " + this.XboxUserId + "\n" +
                        "    SignedIn: " + (this.SignedIn == true ? "yes" : "no") + "\n" +
                        (this.SponsoredUser != true ? "    AutoSignIn: " + (this.AutoSignIn == true ? "yes" : "no") + "\n" : string.Empty);
            }
        }
        #endregion // Data contract
    }
}
