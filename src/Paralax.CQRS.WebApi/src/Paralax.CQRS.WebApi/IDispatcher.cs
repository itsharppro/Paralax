using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Paralax.CQRS.Events;
using Paralax.CQRS.Queries;

namespace Paralax.CQRS.WebApi
{
    public interface IDispatcher
    {
        Task SendAsync<T>(T command, CancellationToken cancellationToken = default) where T : class, ICommand;
        Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : class, IEvent;
        Task<TResult> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default);
    }
}