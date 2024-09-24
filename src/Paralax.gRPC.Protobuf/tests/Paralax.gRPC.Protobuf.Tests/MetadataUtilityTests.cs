using System;
using Paralax.Common;
using Paralax.gRPC.Protobuf.Utilities;
using Xunit;

namespace Paralax.gRPC.Tests.Utilities
{
    public class MetadataUtilityTests
    {
        [Fact]
        public void CreateMetadata_ShouldGenerateNewRequestId_WhenRequestIdIsNotProvided()
        {
            // Arrange
            string serviceName = "TestService";

            // Act
            var result = MetadataUtility.CreateMetadata(serviceName);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(result.RequestId));
            Assert.True(Guid.TryParse(result.RequestId, out _)); // Check if the RequestId is a valid GUID
            Assert.Equal(serviceName, result.ServiceName);
            Assert.True(DateTime.TryParse(result.Timestamp, out _)); // Ensure the timestamp is a valid date
        }

        [Fact]
        public void CreateMetadata_ShouldUseProvidedRequestId()
        {
            // Arrange
            string serviceName = "TestService";
            string requestId = "CustomRequestId123";

            // Act
            var result = MetadataUtility.CreateMetadata(serviceName, requestId);

            // Assert
            Assert.Equal(requestId, result.RequestId); // Check if the provided requestId is used
            Assert.Equal(serviceName, result.ServiceName);
            Assert.True(DateTime.TryParse(result.Timestamp, out _)); // Ensure the timestamp is a valid date
        }

        [Fact]
        public void CreateMetadata_ShouldSetCorrectTimestampFormat()
        {
            // Arrange
            string serviceName = "TestService";

            // Act
            var result = MetadataUtility.CreateMetadata(serviceName);

            // Assert
            Assert.True(DateTime.TryParseExact(result.Timestamp, "o", null, System.Globalization.DateTimeStyles.RoundtripKind, out _)); // Validate ISO 8601 format
        }

        [Fact]
        public void CreateMetadata_ShouldSetServiceNameCorrectly()
        {
            // Arrange
            string serviceName = "MyTestService";

            // Act
            var result = MetadataUtility.CreateMetadata(serviceName);

            // Assert
            Assert.Equal(serviceName, result.ServiceName); // Ensure the service name is set correctly
        }
    }
}
