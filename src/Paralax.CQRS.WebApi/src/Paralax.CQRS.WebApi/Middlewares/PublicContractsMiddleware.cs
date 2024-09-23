using Microsoft.AspNetCore.Http;
using Paralax.CQRS.Commands;
using Paralax.CQRS.Events;
using NetJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Paralax.WebApi.CQRS.Middlewares
{
    public class PublicContractsMiddleware
    {
        private const string ContentType = "application/json";
        private readonly RequestDelegate _next;
        private readonly string _endpoint;
        private readonly bool _attributeRequired;

        private static readonly ContractTypes Contracts = new();
        private static int _initialized;
        private static string _serializedContracts = "{}";

        public PublicContractsMiddleware(RequestDelegate next, string endpoint, Type attributeType,
            bool attributeRequired)
        {
            _next = next;
            _endpoint = endpoint;
            _attributeRequired = attributeRequired;

            if (_initialized == 1)
            {
                return;
            }

            Load(attributeType);
        }

        public Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path != _endpoint)
            {
                return _next(context);
            }

            context.Response.ContentType = ContentType;
            context.Response.WriteAsync(_serializedContracts);

            return Task.CompletedTask;
        }

        private void Load(Type attributeType)
        {
            if (Interlocked.Exchange(ref _initialized, 1) == 1)
            {
                return;
            }

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var contracts = assemblies.SelectMany(a => a.GetTypes())
                .Where(t => (!_attributeRequired || t.GetCustomAttribute(attributeType) is not null) && !t.IsInterface)
                .ToArray();

            foreach (var command in contracts.Where(t => typeof(ICommand).IsAssignableFrom(t)))
            {
                var instance = command.GetDefaultInstance();
                var name = instance.GetType().Name;

                if (Contracts.Commands.ContainsKey(name))
                {
                    throw new InvalidOperationException($"Command: '{name}' already exists.");
                }

                Contracts.Commands[name] = instance;
            }

            foreach (var @event in contracts.Where(t => typeof(IEvent).IsAssignableFrom(t) &&
                                                        t != typeof(RejectedEvent)))
            {
                var instance = @event.GetDefaultInstance();
                var name = instance.GetType().Name;

                if (Contracts.Events.ContainsKey(name))
                {
                    throw new InvalidOperationException($"Event: '{name}' already exists.");
                }

                Contracts.Events[name] = instance;
            }

            _serializedContracts = NetJSON.NetJSON.Serialize(Contracts);
        }

        private class ContractTypes
        {
            public Dictionary<string, object> Commands { get; } = new();
            public Dictionary<string, object> Events { get; } = new();
        }
    }

    public static class TypeExtensions
    {
        public static object GetDefaultInstance(this Type type)
        {
            return Activator.CreateInstance(type);
        }
    }
}
