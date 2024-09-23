using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Primitives;

namespace Paralax.Auth.Distributed
{
    internal sealed class DistributedAccessTokenService : IAccessTokenService
    {
        private readonly IDistributedCache _cache;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly TimeSpan _expires;

        public DistributedAccessTokenService(IDistributedCache cache, IHttpContextAccessor httpContextAccessor,
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

        public async Task<bool> IsActiveAsync(string token)
        {
            var cachedToken = await _cache.GetStringAsync(GetKey(token));
            return string.IsNullOrWhiteSpace(cachedToken);
        }

        public Task DeactivateAsync(string token)
        {
            // Set the token in the distributed cache as "revoked"
            return _cache.SetStringAsync(GetKey(token), "revoked", new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _expires
            });
        }

        private string GetCurrentToken()
        {
            var authorizationHeader = _httpContextAccessor.HttpContext.Request.Headers["Authorization"];

            if (StringValues.IsNullOrEmpty(authorizationHeader))
            {
                return string.Empty;
            }

            return authorizationHeader.ToString().Split(' ').Last();
        }

        private static string GetKey(string token) => $"blacklisted-tokens:{token}";
    }
}
