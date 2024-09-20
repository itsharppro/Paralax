namespace Paralax.CQRS.Queries
{
    public interface IQuery
    {
    }
    public interface IQuery<TResult> : IQuery
    {
    }
}
