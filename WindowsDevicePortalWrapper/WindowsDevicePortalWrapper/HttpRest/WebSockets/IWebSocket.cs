//----------------------------------------------------------------------------------------------
// <copyright file="IWebSocket.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal
{
    public interface IWebSocket
    {
        Task ConnectAsync(Uri endpoint);
    }
}
