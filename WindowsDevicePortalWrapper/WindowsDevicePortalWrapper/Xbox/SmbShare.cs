// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE.TXT in the project root license information.

using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal
{
    public partial class DevicePortal
    {
        private static readonly String _getSmbShareInfo = "/ext/smb/developerfolder";

        public async Task<SmbInfo> GetSmbShareInfo()
        {
            return await Get<SmbInfo>(_getSmbShareInfo);
        }
    }

    #region Data contract

    [DataContract]
    public class SmbInfo
    {
        [DataMember(Name = "Path")]
        public string Path { get; set; }

        [DataMember(Name = "Username")]
        public string Username { get; set; }

        [DataMember(Name = "Password")]
        public string Password { get; set; }

        public override string ToString()
        {
            return Path;
        }
    }
    #endregion // Data contract
}

