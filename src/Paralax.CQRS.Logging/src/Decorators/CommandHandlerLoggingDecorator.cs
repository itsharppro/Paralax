using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Paralax.Core;
using Paralax.CQRS.Commands;
using SmartFormat;

namespace Paralax.CQRS.Logging.Decorators
{
    [Decorator]
    internal sealed class CommandHandlerLoggingDecorator<TCommand> : ICommandHandler<TCommand>
        where TCommand : class, ICommand
    {
        private readonly ICommandHandler<TCommand> _handler;
        private readonly ILogger<CommandHandlerLoggingDecorator<TCommand>> _logger;
        private readonly IMessageToLogTemplateMapper _mapper;

        public CommandHandlerLoggingDecorator(ICommandHandler<TCommand> handler,
            ILogger<CommandHandlerLoggingDecorator<TCommand>> logger, IServiceProvider serviceProvider)
        {
            _handler = handler;
            _logger = logger;
            _mapper = serviceProvider.GetService<IMessageToLogTemplateMapper>() ?? new EmptyMessageToLogTemplateMapper();
        }

        public async Task HandleAsync(TCommand command, CancellationToken cancellationToken = default)
        {
            var template = _mapper.Map(command);

            if (template is null)
            {
                await _handler.HandleAsync(command, cancellationToken);
                return;
            }

            try
            {
                Log(command, template.Before);
                await _handler.HandleAsync(command, cancellationToken);
                Log(command, template.After);
            }
            catch (Exception ex)
            {
                var exceptionTemplate = template.GetExceptionTemplate(ex);
                Log(command, exceptionTemplate, isError: true);
                throw;
            }
        }

        private void Log(TCommand command, string message, bool isError = false)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            var formattedMessage = Smart.Format(message, command);

            if (isError)
            {
                _logger.LogError(formattedMessage);
            }
            else
            {
                _logger.LogInformation(formattedMessage);
            }
        }

        private class EmptyMessageToLogTemplateMapper : IMessageToLogTemplateMapper
        {
            public HandlerLogTemplate Map<TMessage>(TMessage message) where TMessage : class => null;
        }
    }
}
