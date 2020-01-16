//----------------------------------------------------------------------------------------------
// <copyright file="IWebSocketFactory.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.Tools.WindowsDevicePortal
{
    public interface IWebSocketFactory
    {
        WebSocket<T> Create<T>(IDevicePortalConnection connection, Func<object, X509Certificate, X509Chain, SslPolicyErrors, bool> serverCertificateValidationHandler, bool sendStreams = false);
    }

    internal class DefaultWebSocketFactory : IWebSocketFactory
    {
        public WebSocket<T> Create<T>(IDevicePortalConnection connection, Func<object, X509Certificate, X509Chain, SslPolicyErrors, bool> serverCertificateValidationHandler, bool sendStreams = false) =>
            new WebSocket<T>(connection, serverCertificateValidationHandler, sendStreams);
    }
}
