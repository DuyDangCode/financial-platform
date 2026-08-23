namespace FinancialPlatform.Domain.Exeptions;

public class ExistUserException : Exception
{
    public ExistUserException()
        : base("User is existed") { }
}
