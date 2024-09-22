namespace Paralax.WebApi
{
    public class WebApiOptions
    {
        /// <summary>
        /// Determines if requests should bind data from the route.
        /// </summary>
        public bool BindRequestFromRoute { get; set; }

        /// <summary>
        /// Specifies if API versioning should be enabled.
        /// </summary>
        public bool EnableVersioning { get; set; } = true;

        /// <summary>
        /// Enables or disables CORS (Cross-Origin Resource Sharing) policy.
        /// </summary>
        public bool EnableCors { get; set; } = true;

        /// <summary>
        /// Specifies the allowed CORS origins.
        /// </summary>
        public string[] AllowedCorsOrigins { get; set; } = new string[] { "*" };

        /// <summary>
        /// Enables or disables detailed error messages.
        /// </summary>
        public bool EnableDetailedErrors { get; set; } = false;

        /// <summary>
        /// Enables or disables request logging.
        /// </summary>
        public bool EnableRequestLogging { get; set; } = true;

        /// <summary>
        /// Defines the default route template for the Web API.
        /// </summary>
        public string DefaultRouteTemplate { get; set; } = "api/{controller}/{action}/{id?}";

        /// <summary>
        /// Enables or disables Swagger for API documentation.
        /// </summary>
        public bool EnableSwagger { get; set; } = true;

        /// <summary>
        /// Specifies if authentication should be required for all endpoints by default.
        /// </summary>
        public bool RequireAuthentication { get; set; } = true;

        /// <summary>
        /// Defines the supported API versions.
        /// </summary>
        public string[] SupportedApiVersions { get; set; } = new[] { "1.0", "2.0" };

        /// <summary>
        /// Enables or disables HTTPS redirection.
        /// </summary>
        public bool UseHttpsRedirection { get; set; } = true;

        /// <summary>
        /// Enables or disables response caching.
        /// </summary>
        public bool EnableResponseCaching { get; set; } = false;
    }
}
