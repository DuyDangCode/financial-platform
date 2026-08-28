using FinancialPlatform.Domain.Entities;
using FinancialPlatform.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinancialPlatform.Infrastructure.Persistence.Repositories;

public class PasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private readonly AppDbContext _dbContext;

    public PasswordResetTokenRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PasswordResetToken?> GetActiveByCodeAsync(
        string code,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbContext.PasswordResetTokens
            .Where(t => t.Code == code && t.UsedAt == null && t.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddAsync(PasswordResetToken passwordResetToken, CancellationToken cancellationToken = default)
    {
        await _dbContext.PasswordResetTokens.AddAsync(passwordResetToken, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task InvalidateAllForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var activeTokens = await _dbContext.PasswordResetTokens
            .Where(t => t.UserId == userId && t.UsedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.MarkUsed();
        }

        if (activeTokens.Count > 0)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
