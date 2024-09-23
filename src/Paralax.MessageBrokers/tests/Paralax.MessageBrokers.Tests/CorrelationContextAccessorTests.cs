using Xunit;
using Paralax.MessageBrokers;
using System.Threading.Tasks;

public class CorrelationContextAccessorTests
{
    private readonly CorrelationContextAccessor _accessor;

    public CorrelationContextAccessorTests()
    {
        _accessor = new CorrelationContextAccessor();
    }

    [Fact]
    public void CorrelationContext_Should_BeNull_WhenNotSet()
    {
        // Act
        var context = _accessor.CorrelationContext;

        // Assert
        Assert.Null(context);
    }

    [Fact]
    public void CorrelationContext_Should_Store_And_Retrieve_Context()
    {
        // Arrange
        var expectedContext = new { TransactionId = "12345", UserId = "67890" };

        // Act
        _accessor.CorrelationContext = expectedContext;
        var actualContext = _accessor.CorrelationContext;

        // Assert
        Assert.Equal(expectedContext, actualContext);
    }

    [Fact]
    public void CorrelationContext_Should_BeNull_AfterSettingToNull()
    {
        // Arrange
        var context = new { TransactionId = "12345" };
        _accessor.CorrelationContext = context;

        // Act
        _accessor.CorrelationContext = null;

        // Assert
        Assert.Null(_accessor.CorrelationContext);
    }

    [Fact]
    public async Task CorrelationContext_Should_BeScopedPerAsyncFlow()
    {
        // Arrange
        var context1 = new { TransactionId = "tx1" };
        var context2 = new { TransactionId = "tx2" };

        // Act
        _accessor.CorrelationContext = context1;

        await Task.Run(() =>
        {
            // Set a different context within the task
            _accessor.CorrelationContext = context2;
            Assert.Equal(context2, _accessor.CorrelationContext);
        });

        // Ensure original context is restored after async task
        Assert.Equal(context1, _accessor.CorrelationContext);
    }
}
