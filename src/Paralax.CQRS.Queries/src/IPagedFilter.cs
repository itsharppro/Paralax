using System.Collections.Generic;
using Paralax.CQRS.Queries;

namespace ConveParalaxy.CQRS.Queries;

public interface IPagedFilter<TResult, in TQuery> where TQuery : IQuery
{
    PagedResult<TResult> Filter(IEnumerable<TResult> values, TQuery query);
}