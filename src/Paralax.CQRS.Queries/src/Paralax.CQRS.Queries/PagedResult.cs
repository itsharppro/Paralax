using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

namespace Paralax.CQRS.Queries
{
    public class PagedResult<T> : PagedResultBase
    {
        public IEnumerable<T> Items { get; }

        public bool IsEmpty => Items is null || !Items.Any();
        public bool IsNotEmpty => !IsEmpty;

        protected PagedResult()
        {
            Items = Enumerable.Empty<T>();
        }

        protected PagedResult(IEnumerable<T> items,
            int currentPage, int resultsPerPage,
            int totalPages, long totalResults)
            : base(currentPage, resultsPerPage, totalPages, totalResults)
        {
            Items = items ?? Enumerable.Empty<T>();
        }

        public static PagedResult<T> Create(IEnumerable<T> items, 
            int currentPage, int resultsPerPage, 
            int totalPages, long totalResults)
        {
            return new PagedResult<T>(items, currentPage, resultsPerPage, totalPages, totalResults);
        }

        public static PagedResult<T> From(PagedResultBase result, IEnumerable<T> items)
        {
            return new PagedResult<T>(items, result.CurrentPage, result.ResultsPerPage, result.TotalPages, result.TotalResults);
        }

        public static PagedResult<T> Empty => new PagedResult<T>(Enumerable.Empty<T>(), 1, 0, 0, 0);

        public PagedResult<U> Map<U>(Func<T, U> map)
        {
            return PagedResult<U>.From(this, Items.Select(map));
        }
    }
}
