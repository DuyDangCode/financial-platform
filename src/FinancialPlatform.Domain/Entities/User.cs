namespace FinancialPlatform.Domain.Entities;

using FinancialPlatform.Domain.Exceptions;

public class User : BaseEntity
{
    public string UserName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string? FirstName { get; private set; }
    public string? LastName { get; private set; }
    public string? DisplayName { get; private set; }
    public string? PhoneNumber { get; private set; }
    public bool IsActive { get; private set; } = true;

    private User() { }

    private User(
        string userName,
        string email,
        string passwordHash,
        string? firstName,
        string? lastName,
        string? displayName,
        string? phoneNumber)
    {
        SetUserName(userName);
        SetEmail(email);
        SetPasswordHash(passwordHash);
        FirstName = firstName;
        LastName = lastName;
        DisplayName = displayName;
        PhoneNumber = phoneNumber;
    }

    public static User Create(
        string userName,
        string email,
        string passwordHash,
        string? firstName = null,
        string? lastName = null,
        string? displayName = null,
        string? phoneNumber = null)
    {
        return new User(userName, email, passwordHash, firstName, lastName, displayName, phoneNumber);
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    public void UpdateProfile(string? firstName, string? lastName, string? displayName, string? phoneNumber)
    {
        FirstName = firstName;
        LastName = lastName;
        DisplayName = displayName;
        PhoneNumber = phoneNumber;
    }

    public void SetPassword(string passwordHash) => SetPasswordHash(passwordHash);

    private void SetUserName(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
            throw new DomainException("Username is required.");

        if (userName.Length > 256)
            throw new DomainException("Username must not exceed 256 characters.");

        UserName = userName.Trim();
    }

    private void SetEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email is required.");

        if (email.Length > 256 || !email.Contains('@'))
            throw new DomainException("Email is invalid.");

        Email = email.Trim().ToLowerInvariant();
    }

    private void SetPasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("Password hash is required.");

        if (passwordHash.Length > 512)
            throw new DomainException("Password hash must not exceed 512 characters.");

        PasswordHash = passwordHash;
    }
}
