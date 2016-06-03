// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE.TXT in the project root license information.

using System;
using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal
{
    public partial class DevicePortal
    {
        private static readonly String _registerPackageApi = "api/app/packagemanager/register";

        /// <summary>
        /// Registers a loose app on the console
        /// </summary>
        public async Task RegisterApplication(string folderName)
        {
            await Post(_registerPackageApi,
                        String.Format("folder={0}", Utilities.Hex64Encode(folderName)));
        }
    }
}
