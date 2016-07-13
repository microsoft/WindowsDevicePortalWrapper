//----------------------------------------------------------------------------------------------
// <copyright file="ResponseHelpers.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System.Runtime.Serialization;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <content>
    /// Methods for working with Http responses.
    /// </content>
    public partial class DevicePortal
    {
        #region Data contract

        /// <summary>
        /// A null response class when we don't care about the response
        /// body.
        /// </summary>
        [DataContract]
        private class NullResponse
        {
        }

        #endregion
    }
}
