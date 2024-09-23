namespace Paralax.Auth
{
    public class JwtAuthAttribute : AuthAttribute
    {
        public const string AuthenticationScheme = "Bearer";
        
        public JwtAuthAttribute(string policy = "") : base(AuthenticationScheme, policy)
        {
        }
    }
}
