namespace FinancialPlatform.Api.Models.Response;

public class ForgotPasswordResponse
{
    public ForgotPasswordResponse(bool delivered, string? resetCode)
    {
        Delivered = delivered;
        ResetCode = resetCode;
    }

    public bool Delivered { get; }
    public string? ResetCode { get; }
}
