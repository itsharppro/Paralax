using System;
using Paralax.gRPC;
using Paralax.gRPC.Builders;
using Xunit;

namespace Paralax.gRPC.Tests.Builders
{
    public class GrpcOptionsBuilderTests
    {
        [Fact]
        public void Build_ShouldReturnDefaultGrpcOptions()
        {
            // Arrange
            var builder = new GrpcOptionsBuilder();

            // Act
            var options = builder.Build();

            // Assert
            Assert.Equal(5001, options.Port);
            Assert.True(options.EnableReflection);
            Assert.Equal(4 * 1024 * 1024, options.MaxReceiveMessageSize);
            Assert.Equal(4 * 1024 * 1024, options.MaxSendMessageSize);
            Assert.True(options.EnableRetries);
            Assert.Equal(TimeSpan.FromSeconds(30), options.Timeout);
            Assert.Equal("DefaultService", options.ServiceName);
            Assert.Equal("1.0.0", options.ServiceVersion);
            Assert.Equal("Production", options.Environment);
        }

        [Fact]
        public void UsePort_ShouldSetPortCorrectly()
        {
            // Arrange
            var builder = new GrpcOptionsBuilder();

            // Act
            var options = builder.UsePort(8080).Build();

            // Assert
            Assert.Equal(8080, options.Port);
        }

        [Fact]
        public void EnableReflection_ShouldSetReflectionCorrectly()
        {
            // Arrange
            var builder = new GrpcOptionsBuilder();

            // Act
            var options = builder.EnableReflection(false).Build();

            // Assert
            Assert.False(options.EnableReflection);
        }

        [Fact]
        public void SetMaxReceiveMessageSize_ShouldSetCorrectSize()
        {
            // Arrange
            var builder = new GrpcOptionsBuilder();

            // Act
            var options = builder.SetMaxReceiveMessageSize(8 * 1024 * 1024).Build();

            // Assert
            Assert.Equal(8 * 1024 * 1024, options.MaxReceiveMessageSize);
        }

        [Fact]
        public void SetMaxSendMessageSize_ShouldSetCorrectSize()
        {
            // Arrange
            var builder = new GrpcOptionsBuilder();

            // Act
            var options = builder.SetMaxSendMessageSize(16 * 1024 * 1024).Build();

            // Assert
            Assert.Equal(16 * 1024 * 1024, options.MaxSendMessageSize);
        }

        [Fact]
        public void EnableRetries_ShouldSetRetriesCorrectly()
        {
            // Arrange
            var builder = new GrpcOptionsBuilder();

            // Act
            var options = builder.EnableRetries(false).Build();

            // Assert
            Assert.False(options.EnableRetries);
        }

        [Fact]
        public void SetTimeout_ShouldSetCorrectTimeout()
        {
            // Arrange
            var builder = new GrpcOptionsBuilder();

            // Act
            var options = builder.SetTimeout(TimeSpan.FromMinutes(2)).Build();

            // Assert
            Assert.Equal(TimeSpan.FromMinutes(2), options.Timeout);
        }

        [Fact]
        public void SetServiceName_ShouldSetCorrectServiceName()
        {
            // Arrange
            var builder = new GrpcOptionsBuilder();

            // Act
            var options = builder.SetServiceName("TestService").Build();

            // Assert
            Assert.Equal("TestService", options.ServiceName);
        }

        [Fact]
        public void SetServiceVersion_ShouldSetCorrectServiceVersion()
        {
            // Arrange
            var builder = new GrpcOptionsBuilder();

            // Act
            var options = builder.SetServiceVersion("2.0.0").Build();

            // Assert
            Assert.Equal("2.0.0", options.ServiceVersion);
        }

        [Fact]
        public void SetEnvironment_ShouldSetCorrectEnvironment()
        {
            // Arrange
            var builder = new GrpcOptionsBuilder();

            // Act
            var options = builder.SetEnvironment("Development").Build();

            // Assert
            Assert.Equal("Development", options.Environment);
        }
    }
}
