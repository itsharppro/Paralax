using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Paralax.Persistence.Postgres.Builders;
using Paralax.Persistence.Postgres.Factories;
using Paralax.Persistence.Postgres.Initializers;
using Paralax.Persistence.Postgres.Repositories;
using Paralax.Types;

namespace Paralax.Persistence.Postgres
{
    public static class Extensions
    {
        private const string SectionName = "postgres";
        private const string RegistryName = "persistence.postgres";

        public static IParalaxBuilder AddPostgres(this IParalaxBuilder builder, string sectionName = SectionName)
        {
            var options = builder.GetOptions<PostgresDbOptions>(sectionName);
            return builder.AddPostgres(options);
        }

        public static IParalaxBuilder AddPostgres(this IParalaxBuilder builder, Func<PostgresOptionsBuilder, PostgresOptionsBuilder> buildOptions)
        {
            var options = buildOptions(new PostgresOptionsBuilder()).Build();
            return builder.AddPostgres(options);
        }

        public static IParalaxBuilder AddPostgres(this IParalaxBuilder builder, PostgresDbOptions options)
        {
            if (!builder.TryRegister(RegistryName))
            {
                return builder;
            }

            builder.Services.AddSingleton(options);
            builder.Services.AddDbContext<PostgresDbContext>(opt =>
            {
                opt.UseNpgsql(options.ConnectionString);
                if (options.EnableLogging)
                {
                    opt.EnableSensitiveDataLogging();
                }
            });

            builder.Services.AddScoped<PostgresDbContextFactory>();
            builder.Services.AddTransient<PostgresDbInitializer>();
            builder.Services.AddTransient<IPostgresDbInitializer>(provider => provider.GetRequiredService<PostgresDbInitializer>());


            builder.AddInitializer<PostgresDbInitializer>();

            return builder;
        }

        public static IParalaxBuilder AddPostgres<TContext>(this IParalaxBuilder builder, PostgresDbOptions options)
            where TContext : PostgresDbContext
        {
            if (!builder.TryRegister(RegistryName))
            {
                return builder;
            }

            builder.Services.AddSingleton(options);
            // Here we register TContext, so the DI container builds DbContextOptions<TContext>.
            builder.Services.AddDbContext<TContext>(opt =>
            {
                opt.UseNpgsql(options.ConnectionString);
                if (options.EnableLogging)
                {
                    opt.EnableSensitiveDataLogging();
                }
            });

            builder.Services.AddScoped<PostgresDbContextFactory>();
            builder.Services.AddTransient<PostgresDbInitializer>();
            builder.Services.AddTransient<IPostgresDbInitializer>(provider => provider.GetRequiredService<PostgresDbInitializer>());
            builder.AddInitializer<PostgresDbInitializer>();

            return builder;
        }

        public static IParalaxBuilder AddPostgresRepository<TEntity, TIdentifiable>(this IParalaxBuilder builder)
            where TEntity : class, IIdentifiable<TIdentifiable>
        {
            builder.Services.AddTransient<IPostgresRepository<TEntity, TIdentifiable>, PostgresRepository<TEntity, TIdentifiable>>();
            return builder;
        }
    }
}
