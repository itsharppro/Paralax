namespace Paralax.MessageBrokers
{
    public class MessagePropertiesAccessor : IMessagePropertiesAccessor
    {
        private static readonly AsyncLocal<MessageContextHolder> Holder = new();

        public IMessageProperties? MessageProperties
        {
            get => Holder.Value?.Properties;
            set
            {
                if (value == null)
                {
                    Holder.Value = null; 
                }
                else
                {
                    Holder.Value = new MessageContextHolder { Properties = value };
                }
            }
        }

        private class MessageContextHolder
        {
            public IMessageProperties? Properties;
        }
    }
}
