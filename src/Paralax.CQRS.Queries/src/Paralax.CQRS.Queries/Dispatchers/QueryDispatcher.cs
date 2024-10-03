using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Paralax.CQRS.Queries.Dispatchers
{
    internal sealed class QueryDispatcher : IQueryDispatcher
    {
        private readonly IServiceProvider _serviceProvider;

        public QueryDispatcher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<TResult> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
        {
            using var scope = _serviceProvider.CreateScope();
            var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResult));
            var handler = scope.ServiceProvider.GetRequiredService(handlerType);

            // We get the 'HandleAsync' method directly from the handler
            var handleAsyncMethod = handlerType.GetMethod(nameof(IQueryHandler<IQuery<TResult>, TResult>.HandleAsync));
            // if (handleAsyncMethod == null)
            // {
            //     throw new InvalidOperationException($"Handler for query '{query.GetType().Name}' does not contain a valid 'HandleAsync' method.");
            // }

            // var taskResult = handleAsyncMethod.Invoke(handler, new object[] { query, cancellationToken });

            // if (taskResult is Task<TResult> resultTask)
            // {
            //     return await resultTask;
            // }

            // throw new InvalidOperationException($"HandleAsync method for '{query.GetType().Name}' returned an invalid result.");
             return await (Task<TResult>) handlerType
            .GetMethod(nameof(IQueryHandler<IQuery<TResult>, TResult>.HandleAsync))?
            .Invoke(handler, new object[] {query, cancellationToken});
        }

        public async Task<TResult> QueryAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default)
            where TQuery : class, IQuery<TResult>
        {
            using var scope = _serviceProvider.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<IQueryHandler<TQuery, TResult>>();
            return await handler.HandleAsync(query, cancellationToken);
        }
    }
}
