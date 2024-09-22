using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Paralax.CQRS.Events;
using Paralax.CQRS.EventSourcing;

namespace Paralax.CQRS.EventSourcing.Tests
{
    public class EventStoreRepositoryTests
    {
        private readonly Mock<IEventStore> _eventStoreMock;
        private readonly EventStoreRepository<TestAggregate> _repository;

        public EventStoreRepositoryTests()
        {
            _eventStoreMock = new Mock<IEventStore>();
            _repository = new EventStoreRepository<TestAggregate>(_eventStoreMock.Object);
        }

        [Fact]
        public async Task Can_Save_Aggregate_And_Mark_Events_As_Committed()
        {
            // Arrange
            var aggregate = new TestAggregate(Guid.NewGuid());
            aggregate.AddEvent("Test Event");

            // Act
            await _repository.SaveAsync(aggregate, 0);

            // Assert
            _eventStoreMock.Verify(x => x.SaveEventsAsync(aggregate.Id, It.IsAny<IEnumerable<IEvent>>(), 0), Times.Once);
            Assert.True(aggregate.AreEventsCommitted);
        }

        [Fact]
        public async Task Can_Load_Aggregate_From_Event_History()
        {
            // Arrange
            var aggregateId = Guid.NewGuid();
            var events = new List<IEvent>
            {
                new TestEvent(aggregateId, "First Event"),
                new TestEvent(aggregateId, "Second Event")
            };

            _eventStoreMock.Setup(x => x.GetEventsAsync(aggregateId)).ReturnsAsync(events);

            // Act
            var aggregate = await _repository.GetByIdAsync(aggregateId);

            // Assert
            Assert.Equal(2, aggregate.AppliedEvents.Count);
            Assert.Contains(aggregate.AppliedEvents, e => e.Description == "First Event");
            Assert.Contains(aggregate.AppliedEvents, e => e.Description == "Second Event");
        }

        public class TestAggregate : IEventSourced
        {
            private readonly List<IEvent> _uncommittedEvents = new();
            public List<TestEvent> AppliedEvents { get; } = new();
            public bool AreEventsCommitted { get; private set; }
            public Guid Id { get; private set; }
            public int Version { get; private set; }

            // Parameterless constructor for the repository to instantiate
            public TestAggregate()
            {
                Id = Guid.NewGuid();
            }

            // Existing constructor with Guid
            public TestAggregate(Guid id)
            {
                Id = id;
            }

            public void AddEvent(string description)
            {
                var @event = new TestEvent(Id, description);
                ApplyEvent(@event);
                _uncommittedEvents.Add(@event);
            }

            public void ApplyEvent(IEvent @event)
            {
                var testEvent = @event as TestEvent;
                if (testEvent != null)
                {
                    AppliedEvents.Add(testEvent);
                }
            }

            public IEnumerable<IEvent> GetUncommittedEvents()
            {
                return _uncommittedEvents;
            }

            public void MarkEventsAsCommitted()
            {
                AreEventsCommitted = true;
                _uncommittedEvents.Clear();
            }

            public void LoadFromHistory(IEnumerable<IEvent> history)
            {
                foreach (var @event in history)
                {
                    ApplyEvent(@event);
                }
            }
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
