using System.ComponentModel.DataAnnotations;

namespace FinancialPlatform.Api.Models.Request;

public class ChangePasswordRequest
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required, MinLength(8), MaxLength(128)]
    public string NewPassword { get; set; } = string.Empty;
}
