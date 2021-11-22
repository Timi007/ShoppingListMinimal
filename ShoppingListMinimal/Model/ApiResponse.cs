namespace ShoppingListMinimal.Model
{
    public class ApiResponse
    {
        public ApiResponse()
        {
        }

        public ApiResponse(int statusCode, string message, string error)
        {
            StatusCode = statusCode;
            Message = message;
            Error = error;
        }

        public int StatusCode { get; init; }
        public string Message { get; init; } = string.Empty;
        public string Error { get; init; } = string.Empty;
    }
}
