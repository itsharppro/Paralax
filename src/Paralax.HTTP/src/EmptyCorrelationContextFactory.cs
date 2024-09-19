namespace Paralax.HTTP;

internal class EmptyCorrelationContextFactory : ICorrelationContextFactory
{
    public string Create() => default;
}