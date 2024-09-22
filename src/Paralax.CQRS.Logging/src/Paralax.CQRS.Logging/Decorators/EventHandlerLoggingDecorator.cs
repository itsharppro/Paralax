using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Paralax.Core;
using Paralax.CQRS.Events;
using SmartFormat;

namespace Paralax.CQRS.Logging.Decorators
{
    [Decorator]
    internal sealed class EventHandlerLoggingDecorator<TEvent> : IEventHandler<TEvent>
        where TEvent : class, IEvent
    {
        private readonly IEventHandler<TEvent> _handler;
        private readonly ILogger<EventHandlerLoggingDecorator<TEvent>> _logger;
        private readonly IMessageToLogTemplateMapper _mapper;

        public EventHandlerLoggingDecorator(IEventHandler<TEvent> handler,
            ILogger<EventHandlerLoggingDecorator<TEvent>> logger, IServiceProvider serviceProvider)
        {
            _handler = handler;
            _logger = logger;
            _mapper = serviceProvider.GetService<IMessageToLogTemplateMapper>() ?? new EmptyMessageToLogTemplateMapper();
        }

        public async Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default)
        {
            var template = _mapper.Map(@event);

            if (template is null)
            {
                await _handler.HandleAsync(@event, cancellationToken);
                return;
            }

            try
            {
                Log(@event, template.Before);
                await _handler.HandleAsync(@event, cancellationToken);
                Log(@event, template.After);
            }
            catch (Exception ex)
            {
                var exceptionTemplate = template.GetExceptionTemplate(ex);
                Log(@event, exceptionTemplate, isError: true);
                throw;
            }
        }

        private void Log(TEvent @event, string message, bool isError = false)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            var formattedMessage = Smart.Format(message, @event);

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
