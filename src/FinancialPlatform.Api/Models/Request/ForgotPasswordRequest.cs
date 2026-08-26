using System.ComponentModel.DataAnnotations;

namespace FinancialPlatform.Api.Models.Request;

public class ForgotPasswordRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}
