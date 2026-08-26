namespace FinancialPlatform.UnitTests.Domain.Entities;

using FinancialPlatform.Domain.Entities;
using FinancialPlatform.Domain.Exceptions;

public class UserTests
{
    private const string ValidUserName = "johndoe";
    private const string ValidEmail = "John.Doe@Example.com";
    private const string ValidPasswordHash = "AQAAAAIAAYagAAAAEHash==";

    [Fact]
    public void Create_WithValidParameters_SetsProperties()
    {
        var user = User.Create(
            ValidUserName,
            ValidEmail,
            ValidPasswordHash,
            "John",
            "Doe",
            "JD",
            "+1234567890");

        Assert.Equal(ValidUserName, user.UserName);
        Assert.Equal("john.doe@example.com", user.Email);
        Assert.Equal(ValidPasswordHash, user.PasswordHash);
        Assert.Equal("John", user.FirstName);
        Assert.Equal("Doe", user.LastName);
        Assert.Equal("JD", user.DisplayName);
        Assert.Equal("+1234567890", user.PhoneNumber);
        Assert.True(user.IsActive);
    }

    [Fact]
    public void Create_WithOptionalParametersNull_LeavesProfileFieldsNull()
    {
        var user = User.Create(ValidUserName, ValidEmail, ValidPasswordHash);

        Assert.Null(user.FirstName);
        Assert.Null(user.LastName);
        Assert.Null(user.DisplayName);
        Assert.Null(user.PhoneNumber);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhiteSpaceUserName_ThrowsDomainException(string? userName)
    {
        var exception = Assert.Throws<DomainException>(
            () => User.Create(userName!, ValidEmail, ValidPasswordHash));

        Assert.Equal("Username is required.", exception.Message);
    }

    [Fact]
    public void Create_WithUserNameLongerThan256Characters_ThrowsDomainException()
    {
        var userName = new string('a', 257);

        var exception = Assert.Throws<DomainException>(
            () => User.Create(userName, ValidEmail, ValidPasswordHash));

        Assert.Equal("Username must not exceed 256 characters.", exception.Message);
    }

    [Fact]
    public void Create_TrimsUserName()
    {
        var user = User.Create($"  {ValidUserName}  ", ValidEmail, ValidPasswordHash);

        Assert.Equal(ValidUserName, user.UserName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhiteSpaceEmail_ThrowsDomainException(string? email)
    {
        var exception = Assert.Throws<DomainException>(
            () => User.Create(ValidUserName, email!, ValidPasswordHash));

        Assert.Equal("Email is required.", exception.Message);
    }

    [Theory]
    [InlineData("john.doe.example.com")]
    public void Create_WithInvalidEmail_ThrowsDomainException(string email)
    {
        var exception = Assert.Throws<DomainException>(
            () => User.Create(ValidUserName, email, ValidPasswordHash));

        Assert.Equal("Email is invalid.", exception.Message);
    }

    [Fact]
    public void Create_WithEmailLongerThan256Characters_ThrowsDomainException()
    {
        var localPart = new string('a', 250);
        var email = $"{localPart}@example.com";

        var exception = Assert.Throws<DomainException>(
            () => User.Create(ValidUserName, email, ValidPasswordHash));

        Assert.Equal("Email is invalid.", exception.Message);
    }

    [Fact]
    public void Create_NormalizesEmailToLowerCaseAndTrims()
    {
        var user = User.Create(ValidUserName, $"  John.Doe@Example.COM  ", ValidPasswordHash);

        Assert.Equal("john.doe@example.com", user.Email);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhiteSpacePasswordHash_ThrowsDomainException(string? passwordHash)
    {
        var exception = Assert.Throws<DomainException>(
            () => User.Create(ValidUserName, ValidEmail, passwordHash!));

        Assert.Equal("Password hash is required.", exception.Message);
    }

    [Fact]
    public void Create_WithPasswordHashLongerThan512Characters_ThrowsDomainException()
    {
        var passwordHash = new string('h', 513);

        var exception = Assert.Throws<DomainException>(
            () => User.Create(ValidUserName, ValidEmail, passwordHash));

        Assert.Equal("Password hash must not exceed 512 characters.", exception.Message);
    }

    [Fact]
    public void Activate_OnDeactivatedUser_SetsIsActiveTrue()
    {
        var user = User.Create(ValidUserName, ValidEmail, ValidPasswordHash);
        user.Deactivate();

        user.Activate();

        Assert.True(user.IsActive);
    }

    [Fact]
    public void Deactivate_ActiveUser_SetsIsActiveFalse()
    {
        var user = User.Create(ValidUserName, ValidEmail, ValidPasswordHash);

        user.Deactivate();

        Assert.False(user.IsActive);
    }

    [Fact]
    public void UpdateProfile_WithNewValues_SetsAllFields()
    {
        var user = User.Create(ValidUserName, ValidEmail, ValidPasswordHash, "OldFirst", "OldLast");

        user.UpdateProfile("Jane", "Smith", "JS", "+1987654321");

        Assert.Equal("Jane", user.FirstName);
        Assert.Equal("Smith", user.LastName);
        Assert.Equal("JS", user.DisplayName);
        Assert.Equal("+1987654321", user.PhoneNumber);
    }

    [Fact]
    public void UpdateProfile_WithNulls_ClearsProfileFields()
    {
        var user = User.Create(
            ValidUserName,
            ValidEmail,
            ValidPasswordHash,
            "John",
            "Doe",
            "JD",
            "+1234567890");

        user.UpdateProfile(null, null, null, null);

        Assert.Null(user.FirstName);
        Assert.Null(user.LastName);
        Assert.Null(user.DisplayName);
        Assert.Null(user.PhoneNumber);
    }

    [Fact]
    public void SetPassword_WithValidHash_ReplacesPasswordHash()
    {
        var user = User.Create(ValidUserName, ValidEmail, ValidPasswordHash);
        var newPasswordHash = "AQAAAAIAAYagAAAAENewHash==";

        user.SetPassword(newPasswordHash);

        Assert.Equal(newPasswordHash, user.PasswordHash);
    }

    [Fact]
    public void SetPassword_WithEmptyHash_ThrowsDomainException()
    {
        var user = User.Create(ValidUserName, ValidEmail, ValidPasswordHash);

        var exception = Assert.Throws<DomainException>(() => user.SetPassword(""));

        Assert.Equal("Password hash is required.", exception.Message);
    }
}
