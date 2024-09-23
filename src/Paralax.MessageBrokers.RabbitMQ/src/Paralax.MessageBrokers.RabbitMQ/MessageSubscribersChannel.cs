using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using Paralax.MessageBrokers.RabbitMQ.Subscribers;

namespace Paralax.MessageBrokers.RabbitMQ
{
    internal class MessageSubscribersChannel
    {
        private readonly Channel<IMessageSubscriber> _channel = Channel.CreateUnbounded<IMessageSubscriber>();

        public ChannelReader<IMessageSubscriber> Reader => _channel.Reader;
        public ChannelWriter<IMessageSubscriber> Writer => _channel.Writer;
    }
}