using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Paralax.CQRS.Commands.Dispatchers
{
    internal sealed class CommandDispatcher : ICommandDispatcher
    {
        private readonly IServiceProvider _serviceProvider;

        public CommandDispatcher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task SendAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
            where TCommand : class, ICommand
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command), "Command cannot be null.");
            }

            using var scope = _serviceProvider.CreateScope();
            
            var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<TCommand>>();

            if (handler == null)
            {
                throw new InvalidOperationException($"No handler registered for command of type {typeof(TCommand).Name}");
            }

            await handler.HandleAsync(command, cancellationToken);
        }
    }
}
