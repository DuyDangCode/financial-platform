using System.ComponentModel.DataAnnotations;

namespace FinancialPlatform.Api.Models.Request;

public class RefreshRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
