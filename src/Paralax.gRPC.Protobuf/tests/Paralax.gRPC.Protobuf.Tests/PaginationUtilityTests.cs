using System.Collections.Generic;
using System.Linq;
using Paralax.Common;
using Paralax.CQRS.Queries;
using Paralax.gRPC.Protobuf.Utilities;
using Xunit;

namespace Paralax.gRPC.Tests.Utilities
{
    public class PaginationUtilityTests
    {
        [Fact]
        public void CreatePagination_ShouldSetPaginationFieldsCorrectly()
        {
            // Arrange
            var items = new List<string> { "item1", "item2", "item3" };
            var pagedResult = PagedResult<string>.Create(items, 1, 10, 1, items.Count);

            // Act
            var result = PaginationUtility.CreatePagination(pagedResult);

            // Assert
            Assert.Equal(1, result.PageNumber);
            Assert.Equal(10, result.PageSize);
            Assert.Equal(1, result.TotalPages);
            Assert.Equal(3, result.TotalItems);
            Assert.Equal(3, result.ItemsCount);
            Assert.True(result.IsFirstPage);
            Assert.True(result.IsLastPage);
            Assert.False(result.HasNextPage);
            Assert.False(result.HasPreviousPage);
        }

        [Fact]
        public void CreatePagination_ShouldSetCorrectPaginationForMultiplePages()
        {
            // Arrange
            var items = Enumerable.Range(1, 50).Select(x => $"Item{x}").ToList();
            var pagedResult = PagedResult<string>.Create(items, 2, 25, 3, 75); // Testing page 2 of 3

            // Act
            var result = PaginationUtility.CreatePagination(pagedResult);

            // Assert
            Assert.Equal(2, result.PageNumber);
            Assert.Equal(25, result.PageSize);
            Assert.Equal(3, result.TotalPages);
            Assert.Equal(75, result.TotalItems);
            Assert.Equal(50, result.ItemsCount); // Only checking 50 here because we're passing that many items in
            Assert.False(result.IsFirstPage);
            Assert.False(result.IsLastPage);
            Assert.True(result.HasNextPage);
            Assert.True(result.HasPreviousPage);
        }

        [Fact]
        public void CreatePagination_ShouldReturnTrueForIsLastPage_WhenLastPage()
        {
            // Arrange
            var items = Enumerable.Range(1, 25).Select(x => $"Item{x}").ToList();
            var pagedResult = PagedResult<string>.Create(items, 3, 25, 3, 75); // Testing last page (page 3)

            // Act
            var result = PaginationUtility.CreatePagination(pagedResult);

            // Assert
            Assert.True(result.IsLastPage);
            Assert.False(result.HasNextPage);
            Assert.True(result.HasPreviousPage);
        }

        [Fact]
        public void CreatePagination_ShouldReturnTrueForIsFirstPage_WhenFirstPage()
        {
            // Arrange
            var items = Enumerable.Range(1, 25).Select(x => $"Item{x}").ToList();
            var pagedResult = PagedResult<string>.Create(items, 1, 25, 3, 75); // Testing first page (page 1)

            // Act
            var result = PaginationUtility.CreatePagination(pagedResult);

            // Assert
            Assert.True(result.IsFirstPage);
            Assert.True(result.HasNextPage);
            Assert.False(result.HasPreviousPage);
        }

        [Fact]
        public void CreatePagination_ShouldReturnNoNextOrPreviousPage_WhenOnlyOnePage()
        {
            // Arrange
            var items = Enumerable.Range(1, 10).Select(x => $"Item{x}").ToList();
            var pagedResult = PagedResult<string>.Create(items, 1, 10, 1, 10); // Only one page of results

            // Act
            var result = PaginationUtility.CreatePagination(pagedResult);

            // Assert
            Assert.True(result.IsFirstPage);
            Assert.True(result.IsLastPage);
            Assert.False(result.HasNextPage);
            Assert.False(result.HasPreviousPage);
        }
    }
}
