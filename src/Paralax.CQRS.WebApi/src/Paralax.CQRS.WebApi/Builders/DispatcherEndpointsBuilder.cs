using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NetJSON;
using Paralax.CQRS.Commands;
using Paralax.CQRS.Queries;

namespace Paralax.WebApi.CQRS.Builders
{
    public class DispatcherEndpointsBuilder : IDispatcherEndpointsBuilder
    {
        private readonly IEndpointsBuilder _builder;

        public DispatcherEndpointsBuilder(IEndpointsBuilder builder)
        {
            _builder = builder;
        }

        public IDispatcherEndpointsBuilder Get(string path, Func<HttpContext, Task> context = null,
            Action<IEndpointConventionBuilder> endpoint = null, bool auth = false, string roles = null,
            params string[] policies)
        {
            _builder.Get(path, context, endpoint, auth, roles, policies);
            return this;
        }

        public IDispatcherEndpointsBuilder Get<TQuery, TResult>(string path,
            Func<TQuery, HttpContext, Task> beforeDispatch = null,
            Func<TQuery, TResult, HttpContext, Task> afterDispatch = null,
            Action<IEndpointConventionBuilder> endpoint = null, bool auth = false, string roles = null,
            params string[] policies) where TQuery : class, IQuery<TResult>
        {
            _builder.Get<TQuery, TResult>(path, async (query, ctx) =>
            {
                if (beforeDispatch is not null)
                {
                    await beforeDispatch(query, ctx);
                }

                var dispatcher = ctx.RequestServices.GetRequiredService<IQueryDispatcher>();
                var result = await dispatcher.QueryAsync<TQuery, TResult>(query);
                if (afterDispatch is null)
                {
                    if (result is null)
                    {
                        ctx.Response.StatusCode = 404;
                        return;
                    }

                    await WriteJsonAsync(ctx.Response, result);
                    return;
                }

                await afterDispatch(query, result, ctx);
            }, endpoint, auth, roles, policies);

            return this;
        }

        public IDispatcherEndpointsBuilder Post(string path, Func<HttpContext, Task> context = null,
            Action<IEndpointConventionBuilder> endpoint = null, bool auth = false, string roles = null,
            params string[] policies)
        {
            _builder.Post(path, context, endpoint, auth, roles, policies);
            return this;
        }

        public IDispatcherEndpointsBuilder Post<T>(string path, Func<T, HttpContext, Task> beforeDispatch = null,
            Func<T, HttpContext, Task> afterDispatch = null, Action<IEndpointConventionBuilder> endpoint = null,
            bool auth = false, string roles = null, params string[] policies)
            where T : class, ICommand
        {
            _builder.Post<T>(path, (cmd, ctx) => BuildCommandContext(cmd, ctx, beforeDispatch, afterDispatch),
                endpoint, auth, roles, policies);

            return this;
        }

        public IDispatcherEndpointsBuilder Put(string path, Func<HttpContext, Task> context = null,
            Action<IEndpointConventionBuilder> endpoint = null, bool auth = false, string roles = null,
            params string[] policies)
        {
            _builder.Put(path, context, endpoint, auth, roles, policies);
            return this;
        }

        public IDispatcherEndpointsBuilder Put<T>(string path, Func<T, HttpContext, Task> beforeDispatch = null,
            Func<T, HttpContext, Task> afterDispatch = null, Action<IEndpointConventionBuilder> endpoint = null,
            bool auth = false, string roles = null, params string[] policies)
            where T : class, ICommand
        {
            _builder.Put<T>(path, (cmd, ctx) => BuildCommandContext(cmd, ctx, beforeDispatch, afterDispatch),
                endpoint, auth, roles, policies);
            return this;
        }

        public IDispatcherEndpointsBuilder Delete(string path, Func<HttpContext, Task> context = null,
            Action<IEndpointConventionBuilder> endpoint = null, bool auth = false, string roles = null,
            params string[] policies)
        {
            _builder.Delete(path, context, endpoint, auth, roles, policies);
            return this;
        }

        public IDispatcherEndpointsBuilder Delete<T>(string path, Func<T, HttpContext, Task> beforeDispatch = null,
            Func<T, HttpContext, Task> afterDispatch = null, Action<IEndpointConventionBuilder> endpoint = null,
            bool auth = false, string roles = null, params string[] policies)
            where T : class, ICommand
        {
            _builder.Delete<T>(path, (cmd, ctx) => BuildCommandContext(cmd, ctx, beforeDispatch, afterDispatch),
                endpoint, auth, roles, policies);
            return this;
        }

        private static async Task BuildCommandContext<T>(T command, HttpContext context,
            Func<T, HttpContext, Task> beforeDispatch = null,
            Func<T, HttpContext, Task> afterDispatch = null) where T : class, ICommand
        {
            if (beforeDispatch is not null)
            {
                await beforeDispatch(command, context);
            }

            var dispatcher = context.RequestServices.GetRequiredService<ICommandDispatcher>();
            await dispatcher.SendAsync(command);

            if (afterDispatch is null)
            {
                context.Response.StatusCode = 200;
                return;
            }

            await afterDispatch(command, context);
        }

        private static async Task WriteJsonAsync(HttpResponse response, object result)
        {
            NetJSON.NetJSON.DateFormat = NetJSON.NetJSONDateFormat.ISO;
            NetJSON.NetJSON.SkipDefaultValue = false;

            response.ContentType = "application/json";

            var json = NetJSON.NetJSON.Serialize(result);
            
            await response.WriteAsync(json);
        }


        private static async Task<T> ReadJsonAsync<T>(HttpContext context) where T : class
        {
            var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
            return NetJSON.NetJSON.Deserialize<T>(body);
        }
    }
}
