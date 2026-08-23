using FinancialPlatform.Domain.Entities;
using FinancialPlatform.Domain.Interface;
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
        var passwordHasher = scope.ServiceProvider.GetRequiredService<FinancialPlatform.Application.Abstractions.Identity.IPasswordHasher>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger(nameof(SeedData));

        await dbContext.Database.EnsureCreatedAsync();

        if (await dbContext.Users.AnyAsync())
        {
            return;
        }

        dbContext.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            UserName = "demo",
            Email = "demo@financialplatform.com",
            PasswordHash = passwordHasher.Hash("Demo@123"),
            FirstName = "Demo",
            LastName = "User",
            DisplayName = "Demo User",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync();

        logger.LogInformation("Seeded demo user: demo@financialplatform.com / Demo@123");
    }
}