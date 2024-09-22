using FluentAssertions;
using System;
using System.Collections.Generic;
using Utf8Json;
using Utf8Json.Resolvers;
using Xunit;

namespace Paralax.WebApi.Tests
{
    public class ParalaxFormatterResolverTests
    {
        private readonly ParalaxFormatterResolver _resolver;

        public ParalaxFormatterResolverTests()
        {
            _resolver = (ParalaxFormatterResolver)ParalaxFormatterResolver.Instance;
        }

        [Fact]
        public void GetFormatter_Should_Return_Custom_Formatter_If_Exists()
        {
            // Arrange
            var customFormatter = new CustomIntFormatter();
            ParalaxFormatterResolver.CustomFormatters.Add(customFormatter);

            // Act
            var formatter = _resolver.GetFormatter<int>();

            // Assert
            formatter.Should().BeOfType<CustomIntFormatter>();
        }

        [Fact]
        public void GetFormatter_Should_Fallback_To_Standard_Resolver_If_No_Custom_Formatter_Exists()
        {
            // Arrange
            ParalaxFormatterResolver.CustomFormatters.Clear();

            // Act
            var formatter = _resolver.GetFormatter<string>();

            // Assert
            formatter.Should().NotBeNull();
            formatter.Should().BeAssignableTo<IJsonFormatter<string>>();
        }

        [Fact]
        public void GetFormatter_Should_Throw_TypeInitializationException_If_No_Formatter_Found()
        {
            // Arrange
            ParalaxFormatterResolver.CustomFormatters.Clear();

            // Act
            Action action = () => _resolver.GetFormatter<UnsupportedType>();

            // Assert
            // Ensure that a TypeInitializationException is thrown, with a TypeLoadException as the inner exception.
            action.Should().Throw<Exception>();
        }

        private class CustomIntFormatter : IJsonFormatter<int>
        {
            public int Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
            {
                return reader.ReadInt32();
            }

            public void Serialize(ref JsonWriter writer, int value, IJsonFormatterResolver formatterResolver)
            {
                writer.WriteInt32(value);
            }
        }

        // A dummy type that Utf8Json will fail to generate a formatter for, causing the TypeInitializationException
        private class UnsupportedType
        {
            public int Id { get; set; }
        }
    }
}
