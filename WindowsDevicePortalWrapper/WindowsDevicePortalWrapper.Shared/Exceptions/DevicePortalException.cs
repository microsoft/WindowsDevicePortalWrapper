//----------------------------------------------------------------------------------------------
// <copyright file="DevicePortalException.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.IO;
#if !WINDOWS_UWP
using System.Net;
using System.Net.Http;
#endif // !WINDOWS_UWP
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Security;
using System.Threading.Tasks;
#if WINDOWS_UWP
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.Web.Http;
#endif // WINDOWS_UWP

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <summary>
    /// Base exception class for a Device Portal exception
    /// </summary>
#if !WINDOWS_UWP
    [Serializable]    
#endif // !WINDOWS_UWP
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
                                                    responseMessage.RequestMessage != null ? responseMessage.RequestMessage.RequestUri : null,
                                                    message,
                                                    innerException)
        {
            try
            {
                if (responseMessage.Content != null)
                {
                    Stream dataStream = null;
#if !WINDOWS_UWP
                using (HttpContent content = responseMessage.Content)
                {
                    dataStream = new MemoryStream();

                    Task copyTask = content.CopyToAsync(dataStream);
                    copyTask.ConfigureAwait(false);
                    copyTask.Wait();

                    // Ensure we point the stream at the origin.
                    dataStream.Position = 0;
                }
#else // WINDOWS_UWP
                    IBuffer dataBuffer = null;
                    using (IHttpContent messageContent = responseMessage.Content)
                    {
                        IAsyncOperationWithProgress<IBuffer, ulong> bufferOperation = messageContent.ReadAsBufferAsync();
                        while (bufferOperation.Status != AsyncStatus.Completed)
                        {
                        }

                        dataBuffer = bufferOperation.GetResults();

                        if (dataBuffer != null)
                        {
                            dataStream = dataBuffer.AsStream();
                        }
                    }
#endif  // WINDOWS_UWP

                    if (dataStream != null)
                    {
                        DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(HttpErrorResponse));

                        HttpErrorResponse errorResponse = (HttpErrorResponse)serializer.ReadObject(dataStream);

                        this.HResult = errorResponse.ErrorCode;
                        this.Reason = errorResponse.ErrorMessage;
                    }
                }
            }
            catch (Exception)
            {
                // Do nothing if we fail to get additional error details from the response body.
            }
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

#if !WINDOWS_UWP
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
#endif // !WINDOWS_UWP

        #region data contract

        /// <summary>
        /// Object containing additional error information from
        /// an HTTP response.
        /// </summary>
        [DataContract]
        private class HttpErrorResponse
        {
            /// <summary>
            /// Gets or sets the ErrorCode
            /// </summary>
            [DataMember(Name = "ErrorCode")]
            public int ErrorCode { get; set; }

            /// <summary>
            /// Gets or sets the ErrorMessage
            /// </summary>
            [DataMember(Name = "ErrorMessage")]
            public string ErrorMessage { get; set; }
        }

        #endregion
    }
}
