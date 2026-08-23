namespace FinancialPlatform.Api.Models.Response;

public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<ValidationError>? ValidationErrors { get; set; }
}