using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace Paralax.Auth.Services
{
    internal sealed class InMemoryAccessTokenService : IAccessTokenService
    {
        private readonly IMemoryCache _cache;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly TimeSpan _expires;

        public InMemoryAccessTokenService(IMemoryCache cache,
            IHttpContextAccessor httpContextAccessor,
            JwtOptions jwtOptions)
        {
            _cache = cache;
            _httpContextAccessor = httpContextAccessor;
            _expires = jwtOptions.Expiry ?? TimeSpan.FromMinutes(jwtOptions.ExpiryMinutes);
        }

        public Task<bool> IsCurrentActiveToken()
            => IsActiveAsync(GetCurrentToken());

        public Task DeactivateCurrentAsync()
            => DeactivateAsync(GetCurrentToken());

        public Task<bool> IsActiveAsync(string token)
        {
            var isRevoked = _cache.Get<string>(GetCacheKey(token)) == "revoked";
            return Task.FromResult(!isRevoked);
        }

        public Task DeactivateAsync(string token)
        {
            _cache.Set(GetCacheKey(token), "revoked", new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _expires
            });

            return Task.CompletedTask;
        }

        private string GetCurrentToken()
        {
            var authorizationHeader = _httpContextAccessor
                .HttpContext.Request.Headers["Authorization"];

            return authorizationHeader == StringValues.Empty
            ? string.Empty
            : authorizationHeader.Single().Split(' ').Last();
        }

        private static string GetCacheKey(string token) => $"blacklisted-tokens:{token}";
    }
}
