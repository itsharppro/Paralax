using System;
using System.Collections.Generic;
using Paralax.Common;
using Paralax.CQRS.Queries;

namespace Paralax.gRPC.Protobuf.Utilities
{
    public static class PaginationUtility
    {
        public static Pagination CreatePagination<T>(PagedResult<T> pagedResult)
        {
            return new Pagination
            {
                PageNumber = pagedResult.CurrentPage,
                PageSize = pagedResult.ResultsPerPage,
                TotalItems = pagedResult.TotalResults,
                TotalPages = pagedResult.TotalPages,
                IsFirstPage = pagedResult.CurrentPage == 1,
                IsLastPage = pagedResult.CurrentPage == pagedResult.TotalPages,
                HasNextPage = pagedResult.CurrentPage < pagedResult.TotalPages,
                HasPreviousPage = pagedResult.CurrentPage > 1,
                ItemsCount = pagedResult.Items.Count() // Updated to use ItemCount
            };
        }
    }
}
