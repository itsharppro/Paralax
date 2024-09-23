using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Paralax.CQRS.Queries;

namespace Paralax.Persistence.MongoDB
{
    public static class Pagination
    {
        /// <summary>
        /// Paginates a MongoDB queryable collection asynchronously based on the page and results per page.
        /// </summary>
        public static async Task<PagedResult<T>> PaginateAsync<T>(this IMongoQueryable<T> collection, IPagedQuery query)
            => await collection.PaginateAsync(query.OrderBy, query.SortOrder, query.Page, query.Results);

        /// <summary>
        /// Paginates a MongoDB queryable collection asynchronously with sorting.
        /// </summary>
        public static async Task<PagedResult<T>> PaginateAsync<T>(this IMongoQueryable<T> collection, string orderBy, string sortOrder, int page = 1, int resultsPerPage = 10)
        {
            if (page <= 0) page = 1;
            if (resultsPerPage <= 0) resultsPerPage = 10;

            var isEmpty = await collection.AnyAsync() == false;
            if (isEmpty)
            {
                return PagedResult<T>.Empty;
            }

            var totalResults = await collection.CountAsync();
            var totalPages = (int)Math.Ceiling((decimal)totalResults / resultsPerPage);

            List<T> data;
            if (string.IsNullOrWhiteSpace(orderBy))
            {
                data = await collection.Limit(page, resultsPerPage).ToListAsync();
                return PagedResult<T>.Create(data, page, resultsPerPage, totalPages, totalResults);
            }

            // Apply sorting by orderBy and sortOrder
            if (sortOrder?.ToLowerInvariant() == "asc")
            {
                data = await collection.OrderBy(ToLambda<T>(orderBy)).Limit(page, resultsPerPage).ToListAsync();
            }
            else
            {
                data = await collection.OrderByDescending(ToLambda<T>(orderBy)).Limit(page, resultsPerPage).ToListAsync();
            }

            return PagedResult<T>.Create(data, page, resultsPerPage, totalPages, totalResults);
        }

        /// <summary>
        /// Limits the results based on the page number and results per page.
        /// </summary>
        public static IMongoQueryable<T> Limit<T>(this IMongoQueryable<T> collection, IPagedQuery query)
            => collection.Limit(query.Page, query.Results);

        /// <summary>
        /// Limits the query to the specified page and results per page.
        /// </summary>
        public static IMongoQueryable<T> Limit<T>(this IMongoQueryable<T> collection, int page = 1, int resultsPerPage = 10)
        {
            if (page <= 0) page = 1;
            if (resultsPerPage <= 0) resultsPerPage = 10;

            var skip = (page - 1) * resultsPerPage;
            return collection.Skip(skip).Take(resultsPerPage);
        }

        /// <summary>
        /// Converts a property name into a lambda expression for ordering.
        /// </summary>
        private static Expression<Func<T, object>> ToLambda<T>(string propertyName)
        {
            var parameter = Expression.Parameter(typeof(T));
            var property = Expression.Property(parameter, propertyName);
            var propAsObject = Expression.Convert(property, typeof(object));

            return Expression.Lambda<Func<T, object>>(propAsObject, parameter);
        }
    }
}
