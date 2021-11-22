namespace ShoppingListMinimal.Model
{
    public class StatusCodeException : Exception
    {
        public StatusCodeException(string message) :
            this(StatusCodes.Status500InternalServerError, message)
        {
        }

        public StatusCodeException(string message, Exception innerException) :
            this(StatusCodes.Status500InternalServerError, message, innerException)
        {
        }

        public StatusCodeException(int statusCode, string message) :
            base(message)
        {
            StatusCode = statusCode;
        }

        public StatusCodeException(int statusCode, string message, Exception innerException) :
            base(message, innerException)
        {
            StatusCode = statusCode;
        }

        public int StatusCode { get; init; }
    }
}
