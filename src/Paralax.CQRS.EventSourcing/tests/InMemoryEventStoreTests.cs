using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Paralax.CQRS.Events;
using Paralax.CQRS.EventSourcing;

namespace Paralax.CQRS.EventSourcing.Tests
{
    public class InMemoryEventStoreTests
    {
        private readonly InMemoryEventStore _eventStore;

        public InMemoryEventStoreTests()
        {
            _eventStore = new InMemoryEventStore();
        }

        [Fact]
        public async Task Can_Save_And_Retrieve_Events()
        {
            // Arrange
            var aggregateId = Guid.NewGuid();
            var events = new List<IEvent>
            {
                new TestEvent(aggregateId, "First Event"),
                new TestEvent(aggregateId, "Second Event")
            };

            // Act
            await _eventStore.SaveEventsAsync(aggregateId, events, 0);
            var retrievedEvents = await _eventStore.GetEventsAsync(aggregateId);

            // Assert
            Assert.Equal(2, retrievedEvents.Count());
            Assert.Contains(retrievedEvents, e => ((TestEvent)e).Description == "First Event");
            Assert.Contains(retrievedEvents, e => ((TestEvent)e).Description == "Second Event");
        }

        [Fact]
        public async Task GetEvents_Returns_Empty_When_No_Events_Found()
        {
            // Arrange
            var aggregateId = Guid.NewGuid();

            // Act
            var events = await _eventStore.GetEventsAsync(aggregateId);

            // Assert
            Assert.Empty(events);
        }

        public class TestEvent : IEvent
        {
            public Guid AggregateId { get; }
            public string Description { get; }

            public TestEvent(Guid aggregateId, string description)
            {
                AggregateId = aggregateId;
                Description = description;
            }
        }
    }
}
