namespace Paralax.CQRS.Logging 
{
    public interface IMessageToLogTemplateMapper
    {
        HandlerLogTemplate Map<TMessage>(TMessage message) where TMessage : class;
    }
}

