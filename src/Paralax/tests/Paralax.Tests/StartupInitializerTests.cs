using System.Threading.Tasks;
using Moq;
using Paralax.Core;
using Paralax.Types;
using Xunit;

namespace Paralax.Tests
{
    public class StartupInitializerTests
    {
        [Fact]
        public void Should_Add_Initializer_When_Valid()
        {
            // Arrange
            var startupInitializer = new StartupInitializer();
            var initializerMock = new Mock<IInitializer>();

            // Act
            startupInitializer.AddInitializer(initializerMock.Object);

            // Assert
            Assert.Contains(initializerMock.Object, startupInitializer.Initializers);
        }

        [Fact]
        public void Should_Not_Add_Initializer_When_Null()
        {
            // Arrange
            var startupInitializer = new StartupInitializer();

            // Act
            startupInitializer.AddInitializer(null);

            // Assert
            Assert.Empty(startupInitializer.Initializers);
        }

        [Fact]
        public void Should_Not_Add_Duplicate_Initializers()
        {
            // Arrange
            var startupInitializer = new StartupInitializer();
            var initializerMock = new Mock<IInitializer>();

            // Act
            startupInitializer.AddInitializer(initializerMock.Object);
            startupInitializer.AddInitializer(initializerMock.Object); 

            // Assert
            Assert.Single(startupInitializer.Initializers);
        }

        [Fact]
        public async Task Should_Initialize_All_Initializers()
        {
            // Arrange
            var startupInitializer = new StartupInitializer();
            var initializerMock1 = new Mock<IInitializer>();
            var initializerMock2 = new Mock<IInitializer>();

            initializerMock1.Setup(i => i.InitializeAsync()).Returns(Task.CompletedTask);
            initializerMock2.Setup(i => i.InitializeAsync()).Returns(Task.CompletedTask);

            startupInitializer.AddInitializer(initializerMock1.Object);
            startupInitializer.AddInitializer(initializerMock2.Object);

            // Act
            await startupInitializer.InitializeAsync();

            // Assert
            initializerMock1.Verify(i => i.InitializeAsync(), Times.Once);
            initializerMock2.Verify(i => i.InitializeAsync(), Times.Once);
        }
    }
}
