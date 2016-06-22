//----------------------------------------------------------------------------------------------
// <copyright file="DevicePortalException.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Security;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <summary>
    /// Base exception class for a Device Portal exception
    /// </summary>
    [Serializable]    
    public class DevicePortalException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DevicePortalException"/> class.
        /// </summary>
        /// <param name="responseMessage">Http response message</param>
        /// <param name="message">Optional exception message</param>
        /// <param name="innerException">Optional inner exception</param>
        public DevicePortalException(
            HttpResponseMessage responseMessage,
            string message = "",
            Exception innerException = null) : this(
                                                    responseMessage.StatusCode,
                                                    responseMessage.ReasonPhrase,
                                                    responseMessage.RequestMessage.RequestUri,
                                                    message,
                                                    innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DevicePortalException"/> class.
        /// </summary>
        /// <param name="statusCode">Http status code</param>
        /// <param name="reason">Reason for exception</param>
        /// <param name="requestUri">Request URI which threw the exception</param>
        /// <param name="message">Optional message</param>
        /// <param name="innerException">Optional inner exception</param>
        public DevicePortalException(
            HttpStatusCode statusCode,
            string reason,
            Uri requestUri = null,
            string message = "",
            Exception innerException = null) : base(
                                                    message,
                                                    innerException)
        {
            this.StatusCode = statusCode;
            this.Reason = reason;
            this.RequestUri = requestUri;
        }

        /// <summary>
        /// Gets the HTTP Status code
        /// </summary>
        public HttpStatusCode StatusCode { get; private set; }
        
        /// <summary>
        /// Gets a reason for the exception
        /// </summary>
        public string Reason { get; private set; }

        /// <summary>
        /// Gets the request URI that threw the exception
        /// </summary>
        public Uri RequestUri { get; private set; }

        /// <summary>
        /// Get object data override
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // TODO - look at an example of how this function is implemented
            base.GetObjectData(info, context);
        }
    }
}
