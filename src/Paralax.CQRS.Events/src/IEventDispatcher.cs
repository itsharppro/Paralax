using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Paralax.CQRS.Events
{
    public interface IEventDispatcher
    {
        Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : class, IEvent;
    }
}