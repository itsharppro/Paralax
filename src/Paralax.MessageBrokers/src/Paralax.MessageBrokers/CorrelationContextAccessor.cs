namespace Paralax.MessageBrokers
{
    public class CorrelationContextAccessor : ICorrelationContextAccessor
    {
        private static readonly AsyncLocal<CorrelationContextHolder> Holder = new();

        public object? CorrelationContext
        {
            get => Holder.Value?.Context;
            set
            {
                if (value == null)
                {
                    Holder.Value = null;
                }
                else
                {
                    Holder.Value = new CorrelationContextHolder { Context = value };
                }
            }
        }

        private class CorrelationContextHolder
        {
            public object? Context;
        }
    }
}
