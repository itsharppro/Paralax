using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Paralax.CQRS.Logging
{
    public sealed class HandlerLogTemplate
    {
        public string? Before { get; set; }
        public string? After { get; set; }
        public IReadOnlyDictionary<Type, string>? OnError { get; set; }

        public string? GetExceptionTemplate(Exception ex)
        {
            var exceptionType = ex.GetType();
            if (OnError == null)
            {
                return null;
            }

            return OnError.TryGetValue(exceptionType, out var template)
                ? template
                : "An unexpected error occurred.";
        }

        public string GetBeforeTemplate<TMessage>(TMessage message)
        {
            var messageType = message?.GetType().Name ?? "UnknownMessage";
            return Before != null
                ? SmartFormat(Before, message)
                : $"Starting to handle message of type {messageType}.";
        }

        public string GetAfterTemplate<TMessage>(TMessage message)
        {
            var messageType = message?.GetType().Name ?? "UnknownMessage";
            return After != null
                ? SmartFormat(After, message)
                : $"Completed handling message of type {messageType}.";
        }

        private string SmartFormat<TMessage>(string template, TMessage message)
        {
            // Serialize anonymous types or complex objects as JSON to make them human-readable
            return string.Format(template, JsonSerializer.Serialize(message));
        }
    }
}
