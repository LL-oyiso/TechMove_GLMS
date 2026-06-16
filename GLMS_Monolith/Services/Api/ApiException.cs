namespace GLMS_Monolith.Services.Api;

public class ApiException : Exception
{
    public int StatusCode { get; }

    public ApiException(int statusCode, string message) : base(message)
    {
        StatusCode = statusCode;
    }
}

public class ApiValidationException : ApiException
{
    public IDictionary<string, string[]> Errors { get; }

    public ApiValidationException(IDictionary<string, string[]> errors)
        : base(400, "One or more validation errors occurred.")
    {
        Errors = errors;
    }
}
