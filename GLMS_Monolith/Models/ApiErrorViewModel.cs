namespace GLMS_Monolith.Models;

public class ApiErrorViewModel
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = "Something went wrong while contacting the API.";
}
