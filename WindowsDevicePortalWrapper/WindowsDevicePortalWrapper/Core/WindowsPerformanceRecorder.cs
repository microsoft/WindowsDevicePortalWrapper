//----------------------------------------------------------------------------------------------
// <copyright file="WindowsPerformanceRecorder.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <content>
    /// Wrappers for DNS methods
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// API for starting and stopping a Windows performance recorder boot performance trace session.
        /// </summary>
        public static readonly string WindowsPerformanceBootTraceApi = "api/wpr/boottrace";

        /// <summary>
        /// API for starting a Windows performance recorder trace using a custom profile.
        /// </summary>
        public static readonly string WindowsPerformanceCustomTraceApi = "api/wpr/customtrace";

        /// <summary>
        /// API for starting and stopping a Windows performance recorder trace session.
        /// </summary>
        public static readonly string WindowsPerformanceTraceApi = "api/wpr/trace";

        /// <summary>
        /// API for getting the status of a Windows performance recorder trace session.
        /// </summary>
        public static readonly string WindowsPerformanceTraceStatusApi = "api/wpr/status";
    }
}
