using System;
using System.Collections.Generic;
using System.Linq;
using Paralax.CQRS.Queries;
using Xunit;

namespace Paralax.CQRS.Queries.Tests
{
    public class PagedResultTests
    {
        [Fact]
        public void PagedResult_Create_Should_Return_Correct_Result()
        {
            // Arrange
            var items = new List<string> { "Item1", "Item2", "Item3" };
            int currentPage = 1;
            int resultsPerPage = 10;
            int totalPages = 2;
            long totalResults = 15;

            // Act
            var pagedResult = PagedResult<string>.Create(items, currentPage, resultsPerPage, totalPages, totalResults);

            // Assert
            Assert.Equal(items, pagedResult.Items);
            Assert.Equal(currentPage, pagedResult.CurrentPage);
            Assert.Equal(resultsPerPage, pagedResult.ResultsPerPage);
            Assert.Equal(totalPages, pagedResult.TotalPages);
            Assert.Equal(totalResults, pagedResult.TotalResults);
            Assert.True(pagedResult.IsNotEmpty);
            Assert.False(pagedResult.IsEmpty);
        }

        [Fact]
        public void PagedResult_Empty_Should_Return_Empty_Result()
        {
            // Act
            var emptyResult = PagedResult<string>.Empty;

            // Assert
            Assert.Empty(emptyResult.Items);
            Assert.Equal(0, emptyResult.CurrentPage); // Ensure CurrentPage is 1
            Assert.Equal(0, emptyResult.ResultsPerPage);
            Assert.Equal(0, emptyResult.TotalPages);
            Assert.Equal(0, emptyResult.TotalResults);
            Assert.True(emptyResult.IsEmpty);
            Assert.False(emptyResult.IsNotEmpty);
        }

        [Fact]
        public void PagedResult_Map_Should_Transform_The_Items()
        {
            // Arrange
            var items = new List<string> { "Item1", "Item2", "Item3" };
            var pagedResult = PagedResult<string>.Create(items, 1, 10, 1, 3);

            // Act
            var mappedResult = pagedResult.Map(item => item.ToUpper());

            // Assert
            var expectedItems = new List<string> { "ITEM1", "ITEM2", "ITEM3" };
            Assert.Equal(expectedItems, mappedResult.Items);
            Assert.Equal(pagedResult.CurrentPage, mappedResult.CurrentPage);
            Assert.Equal(pagedResult.ResultsPerPage, mappedResult.ResultsPerPage);
            Assert.Equal(pagedResult.TotalPages, mappedResult.TotalPages);
            Assert.Equal(pagedResult.TotalResults, mappedResult.TotalResults);
        }

        [Fact]
        public void PagedResult_From_Should_Create_From_Existing_Base()
        {
            // Arrange
            var items = new List<string> { "Item1", "Item2" };
            var pagedResultBase = PagedResult<string>.Create(items, 1, 10, 1, 2);

            // Act
            var newItems = new List<string> { "NewItem1", "NewItem2" };
            var newPagedResult = PagedResult<string>.From(pagedResultBase, newItems);

            // Assert
            Assert.Equal(newItems, newPagedResult.Items);
            Assert.Equal(pagedResultBase.CurrentPage, newPagedResult.CurrentPage);
            Assert.Equal(pagedResultBase.ResultsPerPage, newPagedResult.ResultsPerPage);
            Assert.Equal(pagedResultBase.TotalPages, newPagedResult.TotalPages);
            Assert.Equal(pagedResultBase.TotalResults, newPagedResult.TotalResults);
        }

        [Fact]
        public void PagedResult_With_Null_Items_Should_Handle_Empty_Items()
        {
            // Act
            var pagedResult = PagedResult<string>.Create(null, 1, 10, 1, 0);

            // Assert
            Assert.NotNull(pagedResult.Items);
            Assert.Empty(pagedResult.Items);
            Assert.True(pagedResult.IsEmpty);
        }
    }
}
