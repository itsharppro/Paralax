using Microsoft.Extensions.DependencyInjection;
using Paralax.Security;
using Paralax.Security.Core;
using Paralax;

namespace Paralax.Security
{
    public static class SecurityExtensions
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
