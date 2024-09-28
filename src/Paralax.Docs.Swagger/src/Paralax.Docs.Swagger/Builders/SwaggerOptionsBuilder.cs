namespace Paralax.Docs.Swagger.Builders
{
    public class SwaggerOptionsBuilder : ISwaggerOptionsBuilder
    {
        private string _title;
        private string _version;
        private string _routePrefix;

        private bool _enabled;
        private bool _reDocEnabled;
        private string _name = string.Empty;
        private bool _includeSecurity;
        private bool _serializeAsOpenApiV2;

        public ISwaggerOptionsBuilder Enable(bool enabled)
        {
            _enabled = enabled;
            return this;
        }

        public ISwaggerOptionsBuilder ReDocEnable(bool reDocEnabled)
        {
            _reDocEnabled = reDocEnabled;
            return this;
        }

        public ISwaggerOptionsBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public ISwaggerOptionsBuilder WithTitle(string title)
        {
            _title = title;
            return this;
        }

        public ISwaggerOptionsBuilder WithVersion(string version)
        {
            _version = version;
            return this;
        }

        public ISwaggerOptionsBuilder WithRoutePrefix(string routePrefix)
        {
            _routePrefix = routePrefix;
            return this;
        }

        public ISwaggerOptionsBuilder IncludeSecurity(bool includeSecurity)
        {
            _includeSecurity = includeSecurity;
            return this;
        }

        public ISwaggerOptionsBuilder SerializeAsOpenApiV2(bool serializeAsOpenApiV2)
        {
            _serializeAsOpenApiV2 = serializeAsOpenApiV2;
            return this;
        }

        public SwaggerOptions Build()
        {
            if (string.IsNullOrWhiteSpace(_title) ||
                string.IsNullOrWhiteSpace(_version) ||
                string.IsNullOrWhiteSpace(_routePrefix))
            {
                throw new InvalidOperationException("All required properties must be set (Title, Version, RoutePrefix).");
            }

            var options = new SwaggerOptions(_title, _version, _routePrefix)
            {
                Enabled = _enabled,
                ReDocEnabled = _reDocEnabled,
                Name = _name,
                IncludeSecurity = _includeSecurity,
                SerializeAsOpenApiV2 = _serializeAsOpenApiV2
            };

            return options;
        }
    }
}
