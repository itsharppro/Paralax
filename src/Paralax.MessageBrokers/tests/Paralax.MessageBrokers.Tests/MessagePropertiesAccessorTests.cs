using Xunit;
using Paralax.MessageBrokers;
using System.Threading.Tasks;

public class MessagePropertiesAccessorTests
{
    private readonly MessagePropertiesAccessor _accessor;

    public MessagePropertiesAccessorTests()
    {
        _accessor = new MessagePropertiesAccessor();
    }

    [Fact]
    public void MessageProperties_Should_BeNull_WhenNotSet()
    {
        // Act
        var properties = _accessor.MessageProperties;

        // Assert
        Assert.Null(properties);
    }

    [Fact]
    public void MessageProperties_Should_Store_And_Retrieve_Properties()
    {
        // Arrange
        var expectedProperties = new MessageProperties
        {
            MessageId = "msg1",
            CorrelationId = "corr1",
            Timestamp = 123456789
        };

        // Act
        _accessor.MessageProperties = expectedProperties;
        var actualProperties = _accessor.MessageProperties;

        // Assert
        Assert.Equal(expectedProperties, actualProperties);
    }

    [Fact]
    public void MessageProperties_Should_BeNull_AfterSettingToNull()
    {
        // Arrange
        var properties = new MessageProperties { MessageId = "msg1" };
        _accessor.MessageProperties = properties;

        // Act
        _accessor.MessageProperties = null;

        // Assert
        Assert.Null(_accessor.MessageProperties);
    }

    [Fact]
    public async Task MessageProperties_Should_BeScopedPerAsyncFlow()
    {
        // Arrange
        var properties1 = new MessageProperties { MessageId = "msg1" };
        var properties2 = new MessageProperties { MessageId = "msg2" };

        // Act
        _accessor.MessageProperties = properties1;

        await Task.Run(() =>
        {
            // Set a different set of properties within the task
            _accessor.MessageProperties = properties2;
            Assert.Equal(properties2, _accessor.MessageProperties);
        });

        // Ensure original properties are maintained after async operation
        Assert.Equal(properties1, _accessor.MessageProperties);
    }
}
