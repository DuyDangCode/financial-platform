using FinancialPlatform.Application.Features.Authentication.Commands.Login;
using Microsoft.Extensions.DependencyInjection;

namespace FinancialPlatform.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<LoginCommandHandler>();

        return services;
    }
}