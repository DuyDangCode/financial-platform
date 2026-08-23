namespace FinancialPlatform.Application.Features.Authentication.Commands.Login;

public sealed record LoginCommand(string Email, string Password);