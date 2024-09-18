using Microsoft.Extensions.DependencyInjection;

namespace Paralax.Security
{
    public static class Extensions
    {
        public static IParalaxBuilder AddSecurity(this IParalaxBuilder builder)
        {
            builder.Services
                .AddSingleton<IEncryptor, Encryptor>()
                .AddSingleton<IHasher, Hasher>()
                .AddSingleton<ISigner, Signer>();

            return builder;
        }
    }
}
