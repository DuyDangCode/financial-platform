namespace FinancialPlatform.Api.Models.Response;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public ErrorResponse? Error { get; set; }

    public static ApiResponse<T> SuccessResponse(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<T> FailResponse(ErrorResponse error)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Error = error
        };
    }
}