namespace FinancialPlatform.Application.Features.Authentication.Commands.Register;

public sealed record RegisterCommand(
    string Email,
    string Password,
    string UserName,
    string? FirstName,
    string? LastName,
    string? DisplayName,
    string? PhoneNumber
);
