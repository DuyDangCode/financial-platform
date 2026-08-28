using FinancialPlatform.Domain.Entities;
using FinancialPlatform.Application.Abstractions.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FinancialPlatform.Infrastructure.Persistence;

public static class SeedData
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger(nameof(SeedData));

        await dbContext.Database.EnsureCreatedAsync();

        if (await dbContext.Users.AnyAsync())
        {
            return;
        }

        var demoUser = User.Create(
            "demo",
            "demo@financialplatform.com",
            passwordHasher.Hash("Demo@123"),
            firstName: "Demo",
            lastName: "User",
            displayName: "Demo User");

        dbContext.Users.Add(demoUser);

        await dbContext.SaveChangesAsync();

        logger.LogInformation("Seeded demo user: demo@financialplatform.com / Demo@123");
    }
}
