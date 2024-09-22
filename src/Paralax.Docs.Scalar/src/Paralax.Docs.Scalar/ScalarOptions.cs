namespace Paralax.Docs.Scalar
{
    public class ScalarOptions
    {
        public bool Enabled { get; set; } = true;
        public string Name { get; set; } = "scalar";
        public string Title { get; set; } = "API Documentation";
        public string Version { get; set; } = "v1";
        public string RoutePrefix { get; set; } = "scalar";
        public bool IncludeSecurity { get; set; } = false;
    }
}
