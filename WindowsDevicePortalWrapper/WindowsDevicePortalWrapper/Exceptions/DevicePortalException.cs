using System;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Security;

namespace Microsoft.Tools.WindowsDevicePortal
{
    [Serializable]    
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

        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // TODO - look at an example of how this function is implemented
            base.GetObjectData(info, context);
        }
    }
}
