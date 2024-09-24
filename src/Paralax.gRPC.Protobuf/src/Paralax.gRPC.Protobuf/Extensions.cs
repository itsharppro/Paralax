using Microsoft.Extensions.DependencyInjection;
using Paralax.Common;

public static class ProtobufServiceCollectionExtensions
{
    public static IServiceCollection AddCommonProtobufServices(this IServiceCollection services)
    {
        services.AddSingleton<Metadata>();
        services.AddSingleton<Error>();
        return services;
    }
}
