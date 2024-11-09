using Microsoft.Extensions.DependencyInjection;
using Paralax.Common;

namespace Paralax.gRPC.Protobuf
{
    public static class Extensions
    {
        public static IServiceCollection AddCommonProtobufServices(this IServiceCollection services)
        {
            services.AddScoped<Metadata>();
            services.AddScoped<Error>();
            services.AddScoped<Status>();
            return services;
        }
    }
}
