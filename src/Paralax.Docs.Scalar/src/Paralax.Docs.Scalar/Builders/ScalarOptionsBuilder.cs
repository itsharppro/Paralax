namespace Paralax.Docs.Scalar.Builders
{
    internal sealed class ScalarOptionsBuilder : IScalarOptionsBuilder
    {
        private readonly ScalarOptions _options = new();

        public IScalarOptionsBuilder Enable(bool enabled)
        {
            _options.Enabled = enabled;
            return this;
        }

        public IScalarOptionsBuilder WithName(string name)
        {
            _options.Name = name;
            return this;
        }

        public IScalarOptionsBuilder WithTitle(string title)
        {
            _options.Title = title;
            return this;
        }

        public IScalarOptionsBuilder WithVersion(string version)
        {
            _options.Version = version;
            return this;
        }

        public IScalarOptionsBuilder WithRoutePrefix(string routePrefix)
        {
            _options.RoutePrefix = routePrefix;
            return this;
        }

        public IScalarOptionsBuilder IncludeSecurity(bool includeSecurity)
        {
            _options.IncludeSecurity = includeSecurity;
            return this;
        }

        public ScalarOptions Build() => _options;
    }
}
