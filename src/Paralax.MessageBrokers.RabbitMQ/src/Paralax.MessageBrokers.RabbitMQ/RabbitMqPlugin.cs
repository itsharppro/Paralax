using System;
using System.Threading.Tasks;
using Paralax.MessageBrokers.RabbitMQ.Plugins;
using RabbitMQ.Client.Events;

namespace Paralax.MessageBrokers.RabbitMQ
{
    public abstract class RabbitMqPlugin : IRabbitMqPlugin, IRabbitMqPluginAccessor
    {
        private Func<object, object, BasicDeliverEventArgs, Task> _successor;

        /// <summary>
        /// Abstract method for handling a RabbitMQ message. Each plugin must implement this method.
        /// </summary>
        /// <param name="message">The message being processed.</param>
        /// <param name="correlationContext">The correlation context, typically used for tracking.</param>
        /// <param name="args">RabbitMQ delivery arguments (e.g., delivery tags, message headers).</param>
        public abstract Task HandleAsync(object message, object correlationContext, BasicDeliverEventArgs args);

        /// <summary>
        /// Invokes the next plugin in the pipeline.
        /// </summary>
        /// <param name="message">The message being processed.</param>
        /// <param name="correlationContext">The correlation context, typically used for tracking.</param>
        /// <param name="args">RabbitMQ delivery arguments (e.g., delivery tags, message headers).</param>
        /// <returns>A task representing the asynchronous operation of the next plugin.</returns>
        public Task Next(object message, object correlationContext, BasicDeliverEventArgs args)
            => _successor?.Invoke(message, correlationContext, args) ?? Task.CompletedTask;

        /// <summary>
        /// Sets the successor plugin in the pipeline. This method is called by the pipeline to chain plugins together.
        /// </summary>
        /// <param name="successor">A delegate representing the next plugin's message handler.</param>
        void IRabbitMqPluginAccessor.SetSuccessor(Func<object, object, BasicDeliverEventArgs, Task> successor)
            => _successor = successor;
    }
}
