using System;
using System.Threading.Tasks;

namespace Paralax.MessageBrokers.RabbitMQ.Subscribers
{
    internal class MessageSubscriber : IMessageSubscriber
    {
        public MessageSubscriberAction Action { get; }
        public Type Type { get; }
        public Func<IServiceProvider, object, object, Task> Handle { get; }

        private MessageSubscriber(MessageSubscriberAction action, Type type,
            Func<IServiceProvider, object, object, Task> handle = null)
        {
            Action = action;
            Type = type;
            Handle = handle;
        }

        public static MessageSubscriber Subscribe(Type type, Func<IServiceProvider, object, object, Task> handle)
        {
            if (handle == null)
            {
                throw new ArgumentNullException(nameof(handle), "Message handler cannot be null when subscribing.");
            }

            return new MessageSubscriber(MessageSubscriberAction.Subscribe, type, handle);
        }

        public static MessageSubscriber Unsubscribe(Type type)
        {
            return new MessageSubscriber(MessageSubscriberAction.Unsubscribe, type);
        }
    }
}
