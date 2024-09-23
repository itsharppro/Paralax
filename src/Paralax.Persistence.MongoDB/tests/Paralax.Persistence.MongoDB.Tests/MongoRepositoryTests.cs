using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Paralax.Persistence.MongoDB.Repositories;
using Paralax.CQRS.Queries;
using Paralax.Types;
using MongoDB.Driver.Linq;
using MongoDB.Driver;
using MockQueryable.Moq;
using MockQueryable;

namespace Paralax.Persistence.MongoDB.Tests.Repositories
{
    public class MongoRepositoryTests
    {
        private readonly Mock<IMongoDbCollection<TestEntity>> _collectionMock;
        private readonly MongoRepository<TestEntity, Guid> _repository;

        public MongoRepositoryTests()
        {
            _collectionMock = new Mock<IMongoDbCollection<TestEntity>>();
            _repository = new MongoRepository<TestEntity, Guid>(_collectionMock.Object);
        }

        [Fact]
        public async Task GetAsync_Should_ReturnEntity_When_FoundById()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var expectedEntity = new TestEntity { Id = entityId };

            _collectionMock
                .Setup(x => x.FindAsync(It.IsAny<Expression<Func<TestEntity, bool>>>()))
                .ReturnsAsync(expectedEntity);

            // Act
            var result = await _repository.GetAsync(entityId);

            // Assert
            Assert.Equal(expectedEntity, result);
            _collectionMock.Verify(x => x.FindAsync(It.IsAny<Expression<Func<TestEntity, bool>>>()), Times.Once);
        }

        [Fact]
        public async Task GetAsync_Should_ReturnEntity_When_FoundByPredicate()
        {
            // Arrange
            var expectedEntity = new TestEntity { Id = Guid.NewGuid() };

            _collectionMock
                .Setup(x => x.FindAsync(It.IsAny<Expression<Func<TestEntity, bool>>>()))
                .ReturnsAsync(expectedEntity);

            // Act
            var result = await _repository.GetAsync(e => e.Id == expectedEntity.Id);

            // Assert
            Assert.Equal(expectedEntity, result);
            _collectionMock.Verify(x => x.FindAsync(It.IsAny<Expression<Func<TestEntity, bool>>>()), Times.Once);
        }

        [Fact]
        public async Task FindAsync_Should_ReturnListOfEntities_When_MatchingPredicate()
        {
            // Arrange
            var expectedEntities = new List<TestEntity>
            {
                new TestEntity { Id = Guid.NewGuid() },
                new TestEntity { Id = Guid.NewGuid() }
            };

            _collectionMock
                .Setup(x => x.ToListAsync(It.IsAny<Expression<Func<TestEntity, bool>>>()))
                .ReturnsAsync(expectedEntities);

            // Act
            var result = await _repository.FindAsync(e => e.Id != Guid.Empty);

            // Assert
            Assert.Equal(expectedEntities, result);
            _collectionMock.Verify(x => x.ToListAsync(It.IsAny<Expression<Func<TestEntity, bool>>>()), Times.Once);
        }

        [Fact]
        public async Task AddAsync_Should_CallAddOnCollection()
        {
            // Arrange
            var newEntity = new TestEntity { Id = Guid.NewGuid() };

            // Act
            await _repository.AddAsync(newEntity);

            // Assert
            _collectionMock.Verify(x => x.AddAsync(newEntity), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_Should_UpdateEntityById()
        {
            // Arrange
            var updatedEntity = new TestEntity { Id = Guid.NewGuid() };

            // Act
            await _repository.UpdateAsync(updatedEntity);

            // Assert
            _collectionMock.Verify(x => x.UpdateAsync(updatedEntity, It.IsAny<Expression<Func<TestEntity, bool>>>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WithPredicate_Should_UpdateEntity()
        {
            // Arrange
            var updatedEntity = new TestEntity { Id = Guid.NewGuid() };

            // Act
            await _repository.UpdateAsync(updatedEntity, e => e.Id == updatedEntity.Id);

            // Assert
            _collectionMock.Verify(x => x.UpdateAsync(updatedEntity, It.IsAny<Expression<Func<TestEntity, bool>>>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_Should_DeleteEntityById()
        {
            // Arrange
            var entityId = Guid.NewGuid();

            // Act
            await _repository.DeleteAsync(entityId);

            // Assert
            _collectionMock.Verify(x => x.DeleteAsync(It.IsAny<Expression<Func<TestEntity, bool>>>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WithPredicate_Should_DeleteEntity()
        {
            // Arrange
            var entityId = Guid.NewGuid();

            // Act
            await _repository.DeleteAsync(e => e.Id == entityId);

            // Assert
            _collectionMock.Verify(x => x.DeleteAsync(It.IsAny<Expression<Func<TestEntity, bool>>>()), Times.Once);
        }

        [Fact]
        public async Task ExistsAsync_Should_ReturnTrue_IfEntityExists()
        {
            // Arrange
            _collectionMock
                .Setup(x => x.ExistsAsync(It.IsAny<Expression<Func<TestEntity, bool>>>()))
                .ReturnsAsync(true);

            // Act
            var result = await _repository.ExistsAsync(e => e.Id != Guid.Empty);

            // Assert
            Assert.True(result);
            _collectionMock.Verify(x => x.ExistsAsync(It.IsAny<Expression<Func<TestEntity, bool>>>()), Times.Once);
        }

        // [Fact]
        // public async Task BrowseAsync_Should_ReturnPagedResult_When_Called()
        // {
        //     // Arrange
        //     var expectedEntities = new List<TestEntity>
        //     {
        //         new TestEntity { Id = Guid.NewGuid() },
        //         new TestEntity { Id = Guid.NewGuid() }
        //     };

        //     // Use MockQueryable to mock AsQueryable (IQueryable)
        //     var mockQueryable = expectedEntities.AsQueryable().BuildMock();

        //     _collectionMock
        //         .Setup(x => x.AsQueryable()) 
        //         .Returns(mockQueryable);

        //     var pagedQueryMock = new Mock<IPagedQuery>();

        //     // Act
        //     var result = await _repository.BrowseAsync(p => p.Id != Guid.Empty, pagedQueryMock.Object);

        //     // Assert
        //     Assert.NotNull(result);
        //     Assert.Equal(expectedEntities.Count, result.Items.Count());
        //     _collectionMock.Verify(x => x.AsQueryable(), Times.Once);
        // }
    }

    // Example entity for testing purposes
    public class TestEntity : IIdentifiable<Guid>
    {
        public Guid Id { get; set; }
    }
}
