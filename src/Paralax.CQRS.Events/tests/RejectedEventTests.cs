using Paralax.CQRS.Events;
using Xunit;

namespace Paralax.CQRS.Events.Tests
{
    public class RejectedEventTests
    {
        [Fact]
        public void RejectedEvent_For_ShouldCreateRejectedEventWithCorrectMessage()
        {
            // Arrange
            var eventName = "TestEvent";

            // Act
            var rejectedEvent = RejectedEvent.For(eventName);

            // Assert
            Assert.Equal("There was an error when executing: TestEvent", rejectedEvent.Reason);
            Assert.Equal("TestEvent_error", rejectedEvent.Code);
        }

        [Fact]
        public void RejectedEvent_Create_ShouldCreateRejectedEventWithProvidedReasonAndCode()
        {
            // Arrange
            var reason = "Invalid data";
            var code = "InvalidData";

            // Act
            var rejectedEvent = RejectedEvent.Create(reason, code);

            // Assert
            Assert.Equal(reason, rejectedEvent.Reason);
            Assert.Equal(code, rejectedEvent.Code);
        }
    }
}
