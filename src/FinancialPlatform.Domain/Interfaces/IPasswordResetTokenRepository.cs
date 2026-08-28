using FinancialPlatform.Domain.Entities;

namespace FinancialPlatform.Domain.Interfaces;

public interface IPasswordResetTokenRepository
{
    Task<PasswordResetToken?> GetActiveByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task AddAsync(PasswordResetToken passwordResetToken, CancellationToken cancellationToken = default);
    Task InvalidateAllForUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
