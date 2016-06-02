using System;
using System.Net;
using System.Net.Http;

namespace Microsoft.Tools.WindowsDevicePortal
{
    public class DevicePortalException : Exception
    {
        public HttpStatusCode StatusCode { get; private set; }
        
        public String Reason { get; private set; }

        public Uri RequestUri { get; private set; }

        public DevicePortalException(HttpResponseMessage responseMessage, 
                                    String message = "",
                                    Exception innerException = null) : this(responseMessage.StatusCode, 
                                                                            responseMessage.ReasonPhrase, 
                                                                            responseMessage.RequestMessage.RequestUri,
                                                                            message,
                                                                            innerException)
        { }

        public DevicePortalException(HttpStatusCode statusCode, 
                                    String reason, 
                                    Uri requestUri = null,
                                    String message = "",
                                    Exception innerException = null) : base(message, 
                                                                            innerException)
        {
            StatusCode = statusCode;
            Reason = reason;
            RequestUri = requestUri;
        }
    }
}
