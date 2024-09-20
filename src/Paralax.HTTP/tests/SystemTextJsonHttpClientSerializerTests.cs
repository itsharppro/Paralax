using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Paralax.HTTP.Tests
{
    public class SystemTextJsonHttpClientSerializerTests
    {
        private readonly SystemTextJsonHttpClientSerializer _serializer;

        public SystemTextJsonHttpClientSerializerTests()
        {
            _serializer = new SystemTextJsonHttpClientSerializer();
        }

        [Fact]
        public void Serialize_ShouldReturnSerializedJsonString()
        {
            // Arrange
            var testObject = new TestObject { Id = 1, Name = "Test" };

            // Act
            var result = _serializer.Serialize(testObject);

            // Assert
            result.Should().Be("{\"id\":1,\"name\":\"Test\"}");
        }

        [Fact]
        public async Task DeserializeAsync_ShouldReturnObjectFromJsonStream()
        {
            // Arrange
            var json = "{\"id\":1,\"name\":\"Test\"}";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

            // Act
            var result = await _serializer.DeserializeAsync<TestObject>(stream);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.Name.Should().Be("Test");
        }

        [Fact]
        public void Constructor_ShouldSetDefaultJsonSerializerOptions_BehaviorTest()
        {
            // Arrange
            var testObject = new TestObject { Id = 1, Name = "Test" };

            // Act
            var serialized = _serializer.Serialize(testObject);

            // Deserialize to check behavior matches expected defaults
            var deserializedObject = JsonSerializer.Deserialize<TestObject>(serialized, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
                NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString,
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
            });

            // Assert
            deserializedObject.Should().NotBeNull();
            deserializedObject.Id.Should().Be(1);
            deserializedObject.Name.Should().Be("Test");
        }

        private class TestObject
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
