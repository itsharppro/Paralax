using System;
using System.Collections.Generic;
using Xunit;

namespace Paralax.CQRS.Logging.Tests
{
    public class HandlerLogTemplateTests
    {
        [Fact]
        public void GetBeforeTemplate_ShouldReturnCorrectTemplate_WhenBeforeTemplateIsDefined()
        {
            // Arrange
            var message = new { Id = 1, Name = "TestMessage" };
            var template = new HandlerLogTemplate
            {
                Before = "Handling message: {0}"
            };

            // Act
            var result = template.GetBeforeTemplate(message);

            // Assert
            Assert.Equal("Handling message: {\"Id\":1,\"Name\":\"TestMessage\"}", result);
        }

        [Fact]
        public void GetBeforeTemplate_ShouldReturnDefaultTemplate_WhenBeforeTemplateIsNull()
        {
            // Arrange
            var message = new { Id = 1, Name = "TestMessage" };
            var template = new HandlerLogTemplate();

            // Act
            var result = template.GetBeforeTemplate(message);

            // Assert
            Assert.Equal($"Starting to handle message of type {message.GetType().Name}.", result);
        }

        [Fact]
        public void GetAfterTemplate_ShouldReturnCorrectTemplate_WhenAfterTemplateIsDefined()
        {
            // Arrange
            var message = new { Id = 1, Name = "TestMessage" };
            var template = new HandlerLogTemplate
            {
                After = "Completed processing message: {0}"
            };

            // Act
            var result = template.GetAfterTemplate(message);

            // Assert
            Assert.Equal("Completed processing message: {\"Id\":1,\"Name\":\"TestMessage\"}", result);
        }

        [Fact]
        public void GetAfterTemplate_ShouldReturnDefaultTemplate_WhenAfterTemplateIsNull()
        {
            // Arrange
            var message = new { Id = 1, Name = "TestMessage" };
            var template = new HandlerLogTemplate();

            // Act
            var result = template.GetAfterTemplate(message);

            // Assert
            Assert.Equal($"Completed handling message of type {message.GetType().Name}.", result);
        }

        [Fact]
        public void GetExceptionTemplate_ShouldReturnCorrectTemplate_ForSpecificException()
        {
            // Arrange
            var exception = new InvalidOperationException();
            var template = new HandlerLogTemplate
            {
                OnError = new Dictionary<Type, string>
                {
                    { typeof(InvalidOperationException), "Operation failed with InvalidOperationException." }
                }
            };

            // Act
            var result = template.GetExceptionTemplate(exception);

            // Assert
            Assert.Equal("Operation failed with InvalidOperationException.", result);
        }

        [Fact]
        public void GetExceptionTemplate_ShouldReturnDefaultTemplate_WhenExceptionTypeIsNotMapped()
        {
            // Arrange
            var exception = new ArgumentNullException();
            var template = new HandlerLogTemplate
            {
                OnError = new Dictionary<Type, string>
                {
                    { typeof(InvalidOperationException), "Operation failed with InvalidOperationException." }
                }
            };

            // Act
            var result = template.GetExceptionTemplate(exception);

            // Assert
            Assert.Equal("An unexpected error occurred.", result);
        }

        [Fact]
        public void GetExceptionTemplate_ShouldReturnNull_WhenOnErrorIsNull()
        {
            // Arrange
            var exception = new InvalidOperationException();
            var template = new HandlerLogTemplate
            {
                OnError = null
            };

            // Act
            var result = template.GetExceptionTemplate(exception);

            // Assert
            Assert.Null(result);
        }
    }
}
