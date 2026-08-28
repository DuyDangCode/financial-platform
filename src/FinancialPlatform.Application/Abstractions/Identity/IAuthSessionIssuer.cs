using FinancialPlatform.Application.Features.Authentication.DTOs;
using FinancialPlatform.Domain.Entities;

namespace FinancialPlatform.Application.Abstractions.Identity;

public interface IAuthSessionIssuer
{
    Task<LoginResponse> IssueAsync(User user, CancellationToken cancellationToken = default);
}
