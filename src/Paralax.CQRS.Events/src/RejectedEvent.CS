using System.Text.Json.Serialization;

namespace Paralax.CQRS.Events
{
    public class RejectedEvent : IRejectedEvent
    {
        public string Reason { get; }
        public string Code { get; }

        [JsonConstructor]
        public RejectedEvent(string reason, string code)
        {
            Reason = reason;
            Code = code;
        }

        public static IRejectedEvent For(string name)
        {
            return new RejectedEvent($"There was an error when executing: {name}", $"{name}_error");
        }

        public static IRejectedEvent Create(string reason, string code)
        {
            return new RejectedEvent(reason, code);
        }
    }
}
