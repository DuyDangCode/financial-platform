namespace FinancialPlatform.Domain.Entities;

using FinancialPlatform.Domain.Exeptions;
using FinancialPlatform.Domain.Interface;

public class User : BaseEntity
{
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? DisplayName { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; } = true;

    public static async Task<User> Create(User newUser, IUserRepository userRepository)
    {
        var holderUser = await userRepository.GetByEmailAsync(newUser.Email);
        if (holderUser is not null)
        {
            throw new ExistUserException();
        }

        await userRepository.AddAsync(newUser);
        return new User() { UserName = newUser.UserName, Email = newUser.Email };
    }
}
