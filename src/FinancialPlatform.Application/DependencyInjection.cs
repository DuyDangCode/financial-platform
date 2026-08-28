using FinancialPlatform.Application.Features.Authentication.Commands.ChangePassword;
using FinancialPlatform.Application.Features.Authentication.Commands.ForgotPassword;
using FinancialPlatform.Application.Features.Authentication.Commands.Login;
using FinancialPlatform.Application.Features.Authentication.Commands.Logout;
using FinancialPlatform.Application.Features.Authentication.Commands.Refresh;
using FinancialPlatform.Application.Features.Authentication.Commands.Register;
using FinancialPlatform.Application.Features.Authentication.Commands.ResetPassword;
using Microsoft.Extensions.DependencyInjection;

namespace FinancialPlatform.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<LoginCommandHandler>();
        services.AddScoped<RegisterCommandHandler>();
        services.AddScoped<RefreshCommandHandler>();
        services.AddScoped<LogoutCommandHandler>();
        services.AddScoped<ChangePasswordCommandHandler>();
        services.AddScoped<ForgotPasswordCommandHandler>();
        services.AddScoped<ResetPasswordCommandHandler>();

        return services;
    }
}
