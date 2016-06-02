// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE.TXT in the project root license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal
{
    public partial class DevicePortal
    {
        private static readonly String _rebootApi = "api/control/restart";
        private static readonly String _shutdownApi = "api/control/shutdown";

        /// <summary>
        /// Reboots the device.
        /// </summary>
        public async Task Reboot()
        {
            await Post(_rebootApi);
        }

        /// <summary>
        /// Shuts down the device.
        /// </summary>
        public async Task Shutdown()
        {
            await Post(_shutdownApi);
        }
    }
}
