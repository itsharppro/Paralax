using System;
using System.Text;
using System.Threading.Tasks;
using OpenTelemetry.Trace;
using RabbitMQ.Client.Events;
using OpenTracing;
using OpenTracing.Tag;
using Paralax.MessageBrokers.RabbitMQ;

namespace Paralax.Tracing.Jaeger.RabbitMQ.Plugins
{
    internal sealed class JaegerPlugin : RabbitMqPlugin
    {
        private readonly ITracer _tracer;
        private readonly string _spanContextHeader;

        public JaegerPlugin(ITracer tracer, RabbitMqOptions options)
        {
            _tracer = tracer ?? throw new ArgumentNullException(nameof(tracer));
            _spanContextHeader = options.GetSpanContextHeader() ?? "span-context";
        }

        public override async Task HandleAsync(object message, object correlationContext, BasicDeliverEventArgs args)
        {
            var messageName = message.GetType().Name.Underscore(); // Converts message name to underscored format
            var messageId = args.BasicProperties.MessageId ?? string.Empty;
            var spanContext = ExtractSpanContext(args); // Extracts span context from headers

            using var scope = BuildScope(messageName, spanContext);
            var span = scope.Span;
            span.Log($"Started processing a message: '{messageName}' [id: '{messageId}'].");

            try
            {
                await Next(message, correlationContext, args); // Call the next plugin in the pipeline
            }
            catch (Exception ex)
            {
                span.SetTag(Tags.Error, true); // Mark the span as errored
                span.Log($"Exception: {ex.Message}"); // Log the exception message
            }

            span.Log($"Finished processing a message: '{messageName}' [id: '{messageId}'].");
        }

        private string ExtractSpanContext(BasicDeliverEventArgs args)
        {
            if (args.BasicProperties.Headers is { } &&
                args.BasicProperties.Headers.TryGetValue(_spanContextHeader, out var spanContextHeader) &&
                spanContextHeader is byte[] spanContextBytes)
            {
                return Encoding.UTF8.GetString(spanContextBytes);
            }

            return string.Empty; // No span context found
        }

        private IScope BuildScope(string messageName, string serializedSpanContext)
        {
            var spanBuilder = _tracer
                .BuildSpan($"processing-{messageName}")
                .WithTag("message-type", messageName);

            if (string.IsNullOrEmpty(serializedSpanContext))
            {
                return spanBuilder.StartActive(true);
            }

            var spanContext = SpanContext.ContextFromString(serializedSpanContext);

            return spanBuilder
                .AddReference(References.FollowsFrom, spanContext)
                .StartActive(true);
        }
    }
}
