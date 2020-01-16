//----------------------------------------------------------------------------------------------
// <copyright file="MockWebSocketFactory.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.Tools.WindowsDevicePortal.Tests
{
    internal class MockWebSocketFactory : IWebSocketFactory
    {
        public WebSocket<T> Create<T>(IDevicePortalConnection connection, Func<object, X509Certificate, X509Chain, SslPolicyErrors, bool> serverCertificateValidationHandler, bool sendStreams = false) =>
            new MockWebSocket<T>(connection, serverCertificateValidationHandler, sendStreams);
    }
}
