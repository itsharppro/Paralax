using Microsoft.Extensions.DependencyInjection;

namespace Paralax.Auth.Distributed
{
    public static class Extensions
    {
        private const string RegistryName = "auth.distributed";

        public static IParalaxBuilder AddDistributedAccessTokenValidator(this IParalaxBuilder builder)
        {
            if (!builder.TryRegister(RegistryName))
            {
                return builder;
            }

            builder.Services.AddSingleton<IAccessTokenService, DistributedAccessTokenService>();

            return builder;
        }
    }
}
