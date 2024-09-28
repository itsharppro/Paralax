namespace Paralax.Docs.Swagger
{
    public class SwaggerOptions
    {
        public bool Enabled { get; set; }
        public bool ReDocEnabled { get; set; }
        public string Name { get; set; } = string.Empty;
        
        public string Title { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string RoutePrefix { get; set; } = string.Empty;


        public bool IncludeSecurity { get; set; }
        public bool SerializeAsOpenApiV2 { get; set; }
        public SwaggerOptions() { }

        // Constructor for required properties
        public SwaggerOptions(string title, string version, string routePrefix)
        {
            Title = title ?? throw new ArgumentNullException(nameof(title));
            Version = version ?? throw new ArgumentNullException(nameof(version));
            RoutePrefix = routePrefix ?? throw new ArgumentNullException(nameof(routePrefix));
        }
    }
}
