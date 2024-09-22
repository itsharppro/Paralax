using System;
using System.Collections.Generic;
using System.Reflection;
using Utf8Json;
using Utf8Json.Resolvers;

namespace Paralax.WebApi
{
    internal sealed class ParalaxFormatterResolver : IJsonFormatterResolver
    {
        public static readonly IJsonFormatterResolver Instance = new ParalaxFormatterResolver();

        // Resolvers that will be used if no custom formatter is available.
        private static readonly IJsonFormatterResolver[] Resolvers =
        {
            StandardResolver.CamelCase, // Use CamelCase as a standard fallback resolver
        };

        public static List<IJsonFormatter> CustomFormatters { get; } = new List<IJsonFormatter>();

        public IJsonFormatter<T> GetFormatter<T>()
        {
            return FormatterCache<T>.Formatter;
        }

        private static class FormatterCache<T>
        {
            public static readonly IJsonFormatter<T> Formatter;

            static FormatterCache()
            {
                foreach (var customFormatter in CustomFormatters)
                {
                    foreach (var implInterface in customFormatter.GetType().GetTypeInfo().ImplementedInterfaces)
                    {
                        var ti = implInterface.GetTypeInfo();
                        if (ti.IsGenericType && ti.GenericTypeArguments[0] == typeof(T))
                        {
                            Formatter = (IJsonFormatter<T>)customFormatter;
                            return;
                        }
                    }
                }

                foreach (var resolver in Resolvers)
                {
                    var formatter = resolver.GetFormatter<T>();
                    if (formatter != null)
                    {
                        Formatter = formatter;
                        return;
                    }
                }

                if (Formatter == null)
                {
                    throw new InvalidOperationException($"No formatter found for type {typeof(T).FullName}");
                }
            }
        }
    }
}
