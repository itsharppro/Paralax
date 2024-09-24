using Microsoft.Extensions.DependencyInjection;
using Paralax.Common;

namespace Paralax.gRPC.Protobuf
{
    public static class Extensions
    {
        public static IServiceCollection AddCommonProtobufServices(this IServiceCollection services)
        {
            services.AddSingleton<Metadata>();
            services.AddSingleton<Error>();
            services.AddSingleton<Status>();
            return services;
        }
    }
}
