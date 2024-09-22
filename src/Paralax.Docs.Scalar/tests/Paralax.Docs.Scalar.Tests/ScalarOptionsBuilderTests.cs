using FluentAssertions;
using Xunit;
using Paralax.Docs.Scalar;
using Paralax.Docs.Scalar.Builders;

namespace Paralax.Docs.Scalar.Tests.Builders
{
    public class ScalarOptionsBuilderTests
    {
        [Fact]
        public void Enable_Should_Set_Enabled_Property()
        {
            // Arrange
            var builder = new ScalarOptionsBuilder();

            // Act
            builder.Enable(false);
            var options = builder.Build();

            // Assert
            options.Enabled.Should().BeFalse();
        }

        [Fact]
        public void WithName_Should_Set_Name_Property()
        {
            // Arrange
            var builder = new ScalarOptionsBuilder();
            var name = "CustomName";

            // Act
            builder.WithName(name);
            var options = builder.Build();

            // Assert
            options.Name.Should().Be(name);
        }

        [Fact]
        public void WithTitle_Should_Set_Title_Property()
        {
            // Arrange
            var builder = new ScalarOptionsBuilder();
            var title = "CustomTitle";

            // Act
            builder.WithTitle(title);
            var options = builder.Build();

            // Assert
            options.Title.Should().Be(title);
        }

        [Fact]
        public void WithVersion_Should_Set_Version_Property()
        {
            // Arrange
            var builder = new ScalarOptionsBuilder();
            var version = "v2";

            // Act
            builder.WithVersion(version);
            var options = builder.Build();

            // Assert
            options.Version.Should().Be(version);
        }

        [Fact]
        public void WithRoutePrefix_Should_Set_RoutePrefix_Property()
        {
            // Arrange
            var builder = new ScalarOptionsBuilder();
            var routePrefix = "docs";

            // Act
            builder.WithRoutePrefix(routePrefix);
            var options = builder.Build();

            // Assert
            options.RoutePrefix.Should().Be(routePrefix);
        }

        [Fact]
        public void IncludeSecurity_Should_Set_IncludeSecurity_Property()
        {
            // Arrange
            var builder = new ScalarOptionsBuilder();

            // Act
            builder.IncludeSecurity(true);
            var options = builder.Build();

            // Assert
            options.IncludeSecurity.Should().BeTrue();
        }

        [Fact]
        public void Build_Should_Return_ScalarOptions_With_Default_Values()
        {
            // Arrange
            var builder = new ScalarOptionsBuilder();

            // Act
            var options = builder.Build();

            // Assert
            options.Enabled.Should().BeTrue();
            options.Name.Should().Be("scalar");
            options.Title.Should().Be("API Documentation");
            options.Version.Should().Be("v1");
            options.RoutePrefix.Should().Be("scalar");
            options.IncludeSecurity.Should().BeFalse();
        }
    }
}
