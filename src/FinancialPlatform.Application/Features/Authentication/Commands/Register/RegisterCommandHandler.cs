namespace FinancialPlatform.Application.Features.Authentication.Commands.Register;

using FinancialPlatform.Application.Abstractions.Identity;
using FinancialPlatform.Application.Features.Authentication.DTOs;
using FinancialPlatform.Domain.Entities;
using FinancialPlatform.Domain.Exceptions;
using FinancialPlatform.Domain.Interfaces;

public class RegisterCommandHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuthSessionIssuer _authSessionIssuer;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IAuthSessionIssuer authSessionIssuer
    )
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _authSessionIssuer = authSessionIssuer;
    }

    public async Task<LoginResponse> Handle(RegisterCommand command, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userRepository.GetByEmailAsync(command.Email, cancellationToken);
        if (existingUser is not null)
        {
            throw new UserAlreadyExistsException();
        }

        var user = User.Create(
            command.UserName,
            command.Email,
            _passwordHasher.Hash(command.Password),
            command.FirstName,
            command.LastName,
            command.DisplayName,
            command.PhoneNumber);

        await _userRepository.AddAsync(user, cancellationToken);

        return await _authSessionIssuer.IssueAsync(user, cancellationToken);
    }
}
