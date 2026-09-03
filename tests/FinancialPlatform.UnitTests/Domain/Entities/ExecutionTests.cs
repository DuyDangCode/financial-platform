namespace FinancialPlatform.UnitTests.Domain.Entities;

using FinancialPlatform.Domain.Entities;
using FinancialPlatform.Domain.Exceptions;

public class ExecutionTests
{
    private static readonly Guid OrderId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidParameters_SetsPropertiesWithDefaults()
    {
        var execution = Execution.Create(OrderId, 10m, 105.25m);

        Assert.Equal(OrderId, execution.OrderId);
        Assert.Equal(10m, execution.ExecutedQuantity);
        Assert.Equal(105.25m, execution.ExecutionPrice);
        Assert.Equal(0m, execution.Fee);
        Assert.True(
            (DateTime.UtcNow - execution.ExecutedAt).Duration() < TimeSpan.FromSeconds(5),
            "ExecutedAt should default to the current UTC time.");
    }

    [Fact]
    public void Create_WithFeeAndExecutedAt_UsesProvidedValues()
    {
        var executedAt = new DateTime(2026, 8, 29, 12, 0, 0, DateTimeKind.Utc);

        var execution = Execution.Create(OrderId, 2.5m, 99.99m, 1.25m, executedAt);

        Assert.Equal(1.25m, execution.Fee);
        Assert.Equal(executedAt, execution.ExecutedAt);
    }

    [Fact]
    public void Create_WithEmptyOrderId_ThrowsDomainException()
    {
        var exception = Assert.Throws<DomainException>(
            () => Execution.Create(Guid.Empty, 10m, 100m));

        Assert.Equal("Order id is required.", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithNonPositiveExecutedQuantity_ThrowsDomainException(decimal executedQuantity)
    {
        var exception = Assert.Throws<DomainException>(
            () => Execution.Create(OrderId, executedQuantity, 100m));

        Assert.Equal("Executed quantity must be greater than zero.", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithNonPositiveExecutionPrice_ThrowsDomainException(decimal executionPrice)
    {
        var exception = Assert.Throws<DomainException>(
            () => Execution.Create(OrderId, 10m, executionPrice));

        Assert.Equal("Execution price must be greater than zero.", exception.Message);
    }

    [Fact]
    public void Create_WithNegativeFee_ThrowsDomainException()
    {
        var exception = Assert.Throws<DomainException>(
            () => Execution.Create(OrderId, 10m, 100m, -0.5m));

        Assert.Equal("Fee must not be negative.", exception.Message);
    }
}