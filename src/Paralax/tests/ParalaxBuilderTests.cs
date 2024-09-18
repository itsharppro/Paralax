using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using Paralax;
using Paralax.Types;

namespace Paralax.Tests
{
    public class ParalaxBuilderTests
    {
        [Fact]
        public void Should_Create_ParalaxBuilder()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = new Mock<IConfiguration>();

            // Act
            var builder = ParalaxBuilder.Create(services, configuration.Object);

            // Assert
            Assert.NotNull(builder);
            Assert.Same(services, builder.Services);
        }

        [Fact]
        public void Should_Register_Only_Once()
        {
            // Arrange
            var services = new ServiceCollection();
            var builder = ParalaxBuilder.Create(services);

            // Act
            var firstTry = builder.TryRegister("test-service");
            var secondTry = builder.TryRegister("test-service");

            // Assert
            Assert.True(firstTry);
            Assert.False(secondTry); 
        }

        [Fact]
        public void Should_Add_Build_Action_And_Execute()
        {
            // Arrange
            var services = new ServiceCollection();
            var builder = ParalaxBuilder.Create(services);
            var actionExecuted = false;

            // Act
            builder.AddBuildAction(provider =>
            {
                actionExecuted = true;
            });

            builder.Build(); 

            // Assert
            Assert.True(actionExecuted); 
        }

        [Fact]
        public void Should_Add_Initializer_And_Execute()
        {
            // Arrange
            var services = new ServiceCollection();
            var builder = ParalaxBuilder.Create(services);
            var initializerMock = new Mock<IInitializer>();

            // Act
            builder.AddInitializer(initializerMock.Object);
            var serviceProvider = builder.Build();

            // Assert
            initializerMock.Verify(i => i.InitializeAsync(), Times.Once); 
        }

       [Fact]
        public void Should_Add_Generic_Initializer_And_Execute()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<MockInitializer>(); // Add MockInitializer as a Singleton
            var builder = ParalaxBuilder.Create(services);

            // Act
            builder.AddInitializer<MockInitializer>();
            var serviceProvider = builder.Build();

            // Assert
            var initializer = serviceProvider.GetRequiredService<MockInitializer>();
            Assert.True(initializer.Initialized); // Ensure the initializer was executed
        }

    }

    // Mock initializer for testing
    public class MockInitializer : IInitializer
    {
        public bool Initialized { get; private set; }

        public Task InitializeAsync()
        {
            Initialized = true;
            return Task.CompletedTask;
        }
    }
}
