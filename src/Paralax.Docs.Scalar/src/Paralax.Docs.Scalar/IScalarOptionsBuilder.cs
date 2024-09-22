namespace Paralax.Docs.Scalar
{
    public interface IScalarOptionsBuilder
    {
        IScalarOptionsBuilder Enable(bool enabled);
        IScalarOptionsBuilder WithName(string name);
        IScalarOptionsBuilder WithTitle(string title);
        IScalarOptionsBuilder WithVersion(string version);
        IScalarOptionsBuilder WithRoutePrefix(string routePrefix);
        IScalarOptionsBuilder IncludeSecurity(bool includeSecurity);
        ScalarOptions Build();
    }
}
