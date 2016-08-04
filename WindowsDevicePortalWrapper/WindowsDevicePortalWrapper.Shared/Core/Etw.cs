//----------------------------------------------------------------------------------------------
// <copyright file="Etw.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <content>
    /// Wrappers for ETW methods
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// API to create a realtime ETW session.
        /// </summary>
        public static readonly string CreateRealtimeEtwSessionApi = "api/etw/session/realtime";

        /// <summary>
        /// API to getthe list of registered custom ETW providers.
        /// </summary>
        public static readonly string GetCustomEtwProvidersApi = "api/etw/customproviders";

        /// <summary>
        /// API to getthe list of registered ETW providers.
        /// </summary>
        public static readonly string GetEtwProvidersApi = "api/etw/providers";
    }
}
