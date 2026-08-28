namespace FinancialPlatform.Domain.Exceptions;

public class InvalidOrExpiredTokenException : DomainException
{
    public InvalidOrExpiredTokenException()
        : base("The token is invalid or has expired.") { }
}
