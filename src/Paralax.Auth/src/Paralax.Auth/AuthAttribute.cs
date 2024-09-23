using Microsoft.AspNetCore.Authorization;

namespace Paralax.Auth
{
    public class AuthAttribute : AuthorizeAttribute
    {
        public AuthAttribute(string scheme, string policy = "") : base(policy)
        {
            AuthenticationSchemes = scheme;
        }  
    }
}