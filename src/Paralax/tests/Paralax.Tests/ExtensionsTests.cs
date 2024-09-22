using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Paralax;
using Paralax.Core;
using Paralax.Types;

public class ExtensionsTests
{
    [Fact]
    public void AddParalax_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection();

        var inMemoryConfig = new Dictionary<string, string>
        {
            {"app:DisplayBanner", "false"},
            {"app:Name", "TestApp"},
            {"app:DisplayVersion", "true"},
            {"app:Version", "1.0"}
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemoryConfig)
            .Build();

        // Act
        var builder = services.AddParalax(sectionName: "app", configuration: configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var resolvedOptions = serviceProvider.GetService<AppOptions>();
        var serviceId = serviceProvider.GetService<IServiceId>();

        Assert.NotNull(resolvedOptions); // Ensure options are not null
        Assert.Equal("TestApp", resolvedOptions?.Name); // Ensure Name is correct
    }

    [Fact]
    public void AddParalax_ShouldPrintFigletText_WhenDisplayBannerIsTrue()
    {
        // Arrange
        var services = new ServiceCollection();

        var inMemoryConfig = new Dictionary<string, string>
        {
            {"app:DisplayBanner", "true"},
            {"app:Name", "TestApp"},
            {"app:DisplayVersion", "true"},
            {"app:Version", "1.0"}
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemoryConfig)
            .Build();

        using (var consoleOutput = new ConsoleOutput())
        {
            // Act
            services.AddParalax(sectionName: "app", configuration: configuration);
            var output = consoleOutput.GetOutput();

            // Assert
            Assert.Contains("#######", output); // Check if "TestApp" is in the ASCII art output
        }
    }

    [Fact]
public void UseParalax_ShouldRunStartupInitializer()
{
    // Arrange
    var services = new ServiceCollection();
    
    var initializerMock = new Mock<IStartupInitializer>();
    initializerMock.Setup(i => i.InitializeAsync()).Returns(Task.CompletedTask);
    
    services.AddSingleton<IStartupInitializer>(initializerMock.Object);

    var serviceProvider = services.BuildServiceProvider();
    var appMock = new Mock<IApplicationBuilder>();
    appMock.Setup(x => x.ApplicationServices).Returns(serviceProvider);

    // Act
    appMock.Object.UseParalax();

    // Assert
    initializerMock.Verify(i => i.InitializeAsync(), Times.Once); // Ensure that InitializeAsync was called
}



    [Fact]
    public void GetOptions_ShouldReturnCorrectOptionsFromConfiguration()
    {
        // Arrange
        var inMemoryConfig = new Dictionary<string, string>
        {
            {"app:Name", "TestApp"},
            {"app:Version", "1.0"}
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemoryConfig)
            .Build();

        // Act
        var result = configuration.GetOptions<AppOptions>("app");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TestApp", result?.Name); // Check the Name value
        Assert.Equal("1.0", result?.Version);  // Check the Version value
    }

    [Fact]
    public void Underscore_ShouldConvertToUnderscoreCase()
    {
        // Arrange
        var input = "HelloWorld";

        // Act
        var result = input.Underscore();

        // Assert
        Assert.Equal("hello_world", result);
    }

    // Mock classes to help with testing

    public class MockStartupInitializer : IStartupInitializer
    {
        public bool Initialized { get; private set; }
        private readonly List<IInitializer> _initializers = new();

        public Task InitializeAsync()
        {
            Initialized = true;
            foreach (var initializer in _initializers)
            {
                initializer.InitializeAsync().GetAwaiter().GetResult();
            }
            return Task.CompletedTask;
        }

        public void AddInitializer(IInitializer initializer)
        {
            _initializers.Add(initializer);
        }
    }

    public class MockServiceScopeFactory : IServiceScopeFactory
    {
        public IServiceScope CreateScope()
        {
            return new MockServiceScope();
        }
    }

    public class MockServiceScope : IServiceScope
    {
        public IServiceProvider ServiceProvider => new Mock<IServiceProvider>().Object;

        public void Dispose()
        {
            // No need to dispose in mock
        }
    }
}
