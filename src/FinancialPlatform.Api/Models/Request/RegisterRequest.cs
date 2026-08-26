using System.ComponentModel.DataAnnotations;

namespace FinancialPlatform.Api.Models.Request;

public class RegisterRequest
{
    [Required, MaxLength(256)]
    public string UserName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8), MaxLength(128)]
    public string Password { get; set; } = string.Empty;

    [MaxLength(128)]
    public string? FirstName { get; set; }

    [MaxLength(128)]
    public string? LastName { get; set; }

    [MaxLength(256)]
    public string? DisplayName { get; set; }

    [Phone, MaxLength(32)]
    public string? PhoneNumber { get; set; }
}
