using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;

namespace Paralax.Auth
{
    /// <summary>
    /// This class bypasses the usual authentication process, returning a successful authentication result for any request.
    /// This is useful when authentication is disabled, allowing the request to be processed without enforcing security checks.
    /// </summary>
    internal sealed class DisabledAuthenticationPolicyEvaluator : IPolicyEvaluator
    {
        /// <summary>
        /// Simulates the authentication process and returns a successful result.
        /// </summary>
        /// <param name="policy">Authorization policy to be evaluated.</param>
        /// <param name="context">The current HTTP context.</param>
        /// <returns>A successful authentication result.</returns>
        public Task<AuthenticateResult> AuthenticateAsync(AuthorizationPolicy policy, HttpContext context)
        {
            // Creating an authentication ticket with an empty claims principal and properties
            var authenticationTicket = new AuthenticationTicket(new ClaimsPrincipal(),
                new AuthenticationProperties(), JwtBearerDefaults.AuthenticationScheme);
            
            // Returning a successful authentication result
            return Task.FromResult(AuthenticateResult.Success(authenticationTicket));
        }

        /// <summary>
        /// Simulates the authorization process and returns a successful result.
        /// </summary>
        /// <param name="policy">Authorization policy to be evaluated.</param>
        /// <param name="authenticationResult">The result of the authentication process.</param>
        /// <param name="context">The current HTTP context.</param>
        /// <param name="resource">An optional resource object.</param>
        /// <returns>A successful policy authorization result.</returns>
        public Task<PolicyAuthorizationResult> AuthorizeAsync(AuthorizationPolicy policy,
            AuthenticateResult authenticationResult, HttpContext context, object resource)
        {
            // Returning a successful authorization result
            return Task.FromResult(PolicyAuthorizationResult.Success());
        }
    }
}
