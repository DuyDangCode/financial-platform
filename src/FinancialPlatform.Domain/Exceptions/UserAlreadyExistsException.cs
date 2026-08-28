namespace FinancialPlatform.Domain.Exceptions;

public class UserAlreadyExistsException : DomainException
{
    public UserAlreadyExistsException()
        : base("A user with this email already exists.") { }
}
