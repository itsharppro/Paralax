namespace Paralax.MessageBrokers
{
    public interface IBusSubscriber
    {
        IBusSubscriber Subscribe<T>(Func<IServiceProvider, T, object, Task> handle) where T : class;

        IBusSubscriber SubscribeToBroker<T>(Func<IServiceProvider, T, object, Task> handle, string brokerName) where T : class;
    }
}
