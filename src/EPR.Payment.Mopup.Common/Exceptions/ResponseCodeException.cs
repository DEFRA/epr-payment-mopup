using System.Net;

namespace EPR.Payment.Mopup.Common.Exceptions
{
    public class ResponseCodeException : Exception
    {
        public HttpStatusCode StatusCode { get; set; }

        public ResponseCodeException(
            HttpStatusCode statusCode,
            string message) : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
