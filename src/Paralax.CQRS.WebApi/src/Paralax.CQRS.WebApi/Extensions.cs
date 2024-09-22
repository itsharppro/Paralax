using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Paralax.CQRS.Commands;
using Paralax.CQRS.Queries;
using Paralax.WebApi;
using Paralax.WebApi.CQRS;
using Paralax.WebApi.CQRS.Builders;
using Paralax.WebApi.CQRS.Middlewares;

namespace Paralax.CQRS.WebApi
{
    public static class Extensions
    {
        // Register InMemoryDispatcher as a singleton service
        public static IServiceCollection AddInMemoryDispatcher(this IServiceCollection services)
        {
            services.AddSingleton<IDispatcher, InMemoryDispatcher>();
            return services;
        }
        
        // Middleware to set up Dispatcher endpoints
        public static IApplicationBuilder UseDispatcherEndpoints(this IApplicationBuilder app,
            Action<IDispatcherEndpointsBuilder> builder, bool useAuthorization = true,
            Action<IApplicationBuilder> middleware = null)
        {
            var endpointDefinitions = app.ApplicationServices.GetRequiredService<WebApiEndpointDefinitions>();
            app.UseRouting();
            if (useAuthorization)
            {
                app.UseAuthorization();
            }

            middleware?.Invoke(app);

            app.UseEndpoints(router => builder(new DispatcherEndpointsBuilder(
                new EndpointsBuilder(router, endpointDefinitions))));

            return app;
        }

        // Setup dispatch for the IDispatcherEndpointsBuilder
        public static IDispatcherEndpointsBuilder Dispatch(this IEndpointsBuilder endpoints,
            Func<IDispatcherEndpointsBuilder, IDispatcherEndpointsBuilder> builder)
            => builder(new DispatcherEndpointsBuilder(endpoints));

        // Middleware to expose public contracts (for a specific type)
        public static IApplicationBuilder UsePublicContracts<T>(this IApplicationBuilder app,
            string endpoint = "/_contracts")
            => app.UsePublicContracts(endpoint, typeof(T));

        // Middleware to expose public contracts (with attribute filtering)
        public static IApplicationBuilder UsePublicContracts(this IApplicationBuilder app,
            bool attributeRequired, string endpoint = "/_contracts")
            => app.UsePublicContracts(endpoint, null, attributeRequired);

        // Middleware to expose public contracts (general version with optional attribute type)
        public static IApplicationBuilder UsePublicContracts(this IApplicationBuilder app,
            string endpoint = "/_contracts", Type attributeType = null, bool attributeRequired = true)
            => app.UseMiddleware<PublicContractsMiddleware>(string.IsNullOrWhiteSpace(endpoint) ? "/_contracts" :
                endpoint.StartsWith("/") ? endpoint : $"/{endpoint}", attributeType ?? typeof(PublicContractAttribute),
                attributeRequired);

        // Extension method to send commands via HttpContext
        public static Task SendAsync<T>(this HttpContext context, T command) where T : class, ICommand
            => context.RequestServices.GetRequiredService<ICommandDispatcher>().SendAsync(command);

        // Extension method to execute a query via HttpContext
        public static Task<TResult> QueryAsync<TResult>(this HttpContext context, IQuery<TResult> query)
            => context.RequestServices.GetRequiredService<IQueryDispatcher>().QueryAsync(query);

        // Extension method for query execution with a generic type
        public static Task<TResult> QueryAsync<TQuery, TResult>(this HttpContext context, TQuery query)
            where TQuery : class, IQuery<TResult>
            => context.RequestServices.GetRequiredService<IQueryDispatcher>().QueryAsync<TQuery, TResult>(query);
    }
}
