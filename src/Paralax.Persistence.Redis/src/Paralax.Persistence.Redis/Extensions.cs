using System;
using Microsoft.Extensions.DependencyInjection;
using Paralax.Persistence.Redis.Builders;
using StackExchange.Redis;

namespace Paralax.Persistence.Redis
{
    public static class Extensions
    {
        private const string SectionName = "redis";
        private const string RegistryName = "persistence.redis";

        public static IParalaxBuilder AddRedis(this IParalaxBuilder builder, string sectionName = SectionName)
        {
            if (string.IsNullOrWhiteSpace(sectionName))
            {
                sectionName = SectionName;
            }

            var options = builder.GetOptions<RedisOptions>(sectionName);
            return builder.AddRedis(options);
        }

        public static IParalaxBuilder AddRedis(this IParalaxBuilder builder,
            Func<IRedisOptionsBuilder, IRedisOptionsBuilder> buildOptions)
        {
            var options = buildOptions(new RedisOptionsBuilder()).Build();
            return builder.AddRedis(options);
        }

        public static IParalaxBuilder AddRedis(this IParalaxBuilder builder, RedisOptions options)
        {
            if (!builder.TryRegister(RegistryName))
            {
                return builder;
            }

            builder.Services
                .AddSingleton(options)
                .AddSingleton<IConnectionMultiplexer>(sp => ConnectionMultiplexer.Connect(options.ConnectionString))
                .AddTransient(sp => sp.GetRequiredService<IConnectionMultiplexer>().GetDatabase(options.Database))
                .AddStackExchangeRedisCache(o =>
                {
                    o.Configuration = options.ConnectionString;
                    o.InstanceName = options.Instance;
                });

            return builder;
        }
    }
}