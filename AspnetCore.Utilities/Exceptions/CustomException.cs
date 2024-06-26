using Newtonsoft.Json.Linq;

namespace AspnetCore.Utilities.Exceptions
{
    public class CustomException : Exception
    {
        public int StatusCode { get; set; }
        public object Data2 { get; set; }
        public string ContentType { get; set; } = @"application/json";

        public CustomException(string message, int statusCode, object data = null)
            : base(message)
        {
            this.StatusCode = statusCode;
            this.Data2 = data;
        }

        public CustomException(int statusCode, Exception inner)
            : this(inner.ToString(), statusCode) { }

        public CustomException(int statusCode, JObject errorObject)
            : this(errorObject.ToString(), statusCode)
        {
            this.ContentType = @"application/json";
        }
    }
}