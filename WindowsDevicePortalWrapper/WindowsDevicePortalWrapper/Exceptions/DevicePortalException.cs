//----------------------------------------------------------------------------------------------
// <copyright file="DevicePortalException.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Security;
using System.Threading.Tasks;

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
        /// <param name="statusCode">The Http status code.</param>
        /// <param name="errorResponse">Http parsed error response message.</param>
        /// <param name="requestUri">Request URI which threw the exception.</param>
        /// <param name="message">Optional exception message.</param>
        /// <param name="innerException">Optional inner exception.</param>
        public DevicePortalException(
            HttpStatusCode statusCode,
            HttpErrorResponse errorResponse,
            Uri requestUri = null,
            string message = "",
            Exception innerException = null) : this(
                                                    statusCode,
                                                    errorResponse.Reason,
                                                    requestUri,
                                                    message,
                                                    innerException)
        {
            this.HResult = errorResponse.ErrorCode;
            this.Reason = errorResponse.ErrorMessage;

            // If we didn't get the Hresult and reason from these properties, try the other ones.
            if (this.HResult == 0)
            {
                this.HResult = errorResponse.Code;
            }

            if (string.IsNullOrEmpty(this.Reason))
            {
                this.Reason = errorResponse.Reason;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DevicePortalException"/> class.
        /// </summary>
        /// <param name="statusCode">Http status code.</param>
        /// <param name="reason">Reason for exception.</param>
        /// <param name="requestUri">Request URI which threw the exception.</param>
        /// <param name="message">Optional message.</param>
        /// <param name="innerException">Optional inner exception.</param>
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
        /// Gets the HTTP Status code.
        /// </summary>
        public HttpStatusCode StatusCode { get; private set; }
        
        /// <summary>
        /// Gets a reason for the exception.
        /// </summary>
        public string Reason { get; private set; }

        /// <summary>
        /// Gets the request URI that threw the exception.
        /// </summary>
        public Uri RequestUri { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DevicePortalException"/> class.
        /// </summary>
        /// <param name="responseMessage">Http response message.</param>
        /// <param name="message">Optional exception message.</param>
        /// <param name="innerException">Optional inner exception.</param>
        /// <returns>async task</returns>
        public static async Task<DevicePortalException> CreateAsync(
            HttpResponseMessage responseMessage,
            string message = "",
            Exception innerException = null)
        {
            DevicePortalException error = new DevicePortalException(
                                                    responseMessage.StatusCode,
                                                    responseMessage.ReasonPhrase,
                                                    responseMessage.RequestMessage != null ? responseMessage.RequestMessage.RequestUri : null,
                                                    message,
                                                    innerException);
            try
            {
                if (responseMessage.Content != null)
                {
                    Stream dataStream = null;
                    using (HttpContent content = responseMessage.Content)
                    {
                        dataStream = new MemoryStream();

                        await content.CopyToAsync(dataStream).ConfigureAwait(false);

                        // Ensure we point the stream at the origin.
                        dataStream.Position = 0;
                    }

                    if (dataStream != null)
                    {
                        HttpErrorResponse errorResponse = dataStream.ReadJson<HttpErrorResponse>();

                        if (errorResponse != null)
                        {
                            error.HResult = errorResponse.ErrorCode;
                            error.Reason = errorResponse.ErrorMessage;

                            // If we didn't get the Hresult and reason from these properties, try the other ones.
                            if (error.HResult == 0)
                            {
                                error.HResult = errorResponse.Code;
                            }

                            if (string.IsNullOrEmpty(error.Reason))
                            {
                                error.Reason = errorResponse.Reason;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Do nothing if we fail to get additional error details from the response body.
            }

            return error;
        }

        /// <summary>
        /// Get object data override
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        #region data contract

        /// <summary>
        /// Object containing additional error information from
        /// an HTTP response.
        /// </summary>
        [DataContract]
        public class HttpErrorResponse
        {
            /// <summary>
            /// Gets the ErrorCode
            /// </summary>
            [DataMember(Name = "ErrorCode")]
            public int ErrorCode { get; private set; }

            /// <summary>
            /// Gets the Code (used by some endpoints instead of ErrorCode).
            /// </summary>
            [DataMember(Name = "Code")]
            public int Code { get; private set; }

            /// <summary>
            /// Gets the ErrorMessage
            /// </summary>
            [DataMember(Name = "ErrorMessage")]
            public string ErrorMessage { get; private set; }

            /// <summary>
            /// Gets the Reason (used by some endpoints instead of ErrorMessage).
            /// </summary>
            [DataMember(Name = "Reason")]
            public string Reason { get; private set; }

            /// <summary>
            /// Gets a value indicating whether the operation succeeded. For an error this should generally be false if present.
            /// </summary>
            [DataMember(Name = "Success")]
            public bool Success { get; private set; }
        }

        #endregion
    }
}
