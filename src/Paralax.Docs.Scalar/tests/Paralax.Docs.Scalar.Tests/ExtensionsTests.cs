using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Paralax.Docs.Scalar;
using Microsoft.AspNetCore.Routing;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Http;
using System;

namespace Paralax.Docs.Scalar.Tests
{
    public class ExtensionsTests
    {
        private readonly Mock<IServiceCollection> _services;
        private readonly Mock<IParalaxBuilder> _paralaxBuilder;
        private readonly ScalarOptions _options;

        public ExtensionsTests()
        {
            _services = new Mock<IServiceCollection>();
            _paralaxBuilder = new Mock<IParalaxBuilder>();
            _options = new ScalarOptions
            {
                Enabled = true,
                RoutePrefix = "docs"
            };
        }

        [Fact]
        public void AddScalarDocs_Should_Add_OpenApi_When_Enabled()
        {
            // Arrange
            _paralaxBuilder.Setup(b => b.Services).Returns(_services.Object);
            _paralaxBuilder.Setup(b => b.TryRegister(It.IsAny<string>())).Returns(true); // Ensure TryRegister returns true

            // Act
            var result = _paralaxBuilder.Object.AddScalarDocs(_options);

            // Assert
            _services.Verify(s => s.Add(It.IsAny<ServiceDescriptor>()), Times.AtLeastOnce); // Verify Add was called
            result.Should().NotBeNull();
        }

        [Fact]
        public void AddScalarDocs_Should_Not_Add_When_Disabled()
        {
            // Arrange
            _options.Enabled = false;
            _paralaxBuilder.Setup(b => b.Services).Returns(_services.Object);

            // Act
            var result = _paralaxBuilder.Object.AddScalarDocs(_options);

            // Assert
            _services.Verify(s => s.Add(It.IsAny<ServiceDescriptor>()), Times.Never); // Verify Add was not called
            result.Should().NotBeNull();
        }

    }
}
