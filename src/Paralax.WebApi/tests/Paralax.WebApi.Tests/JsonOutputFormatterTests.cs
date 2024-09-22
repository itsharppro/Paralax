using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Moq;
using Xunit;
using Paralax.WebApi.Formatters;

public class JsonOutputFormatterTests
{
    private readonly JsonOutputFormatter _formatter;

    public JsonOutputFormatterTests()
    {
        _formatter = new JsonOutputFormatter();
    }

    [Fact]
    public void CanWriteResult_ShouldAlwaysReturnTrue()
    {
        // Arrange
        var context = CreateOutputFormatterContext(typeof(object));

        // Act
        var result = _formatter.CanWriteResult(context);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task WriteAsync_ShouldSerializeAndWriteJson()
    {
        // Arrange
        var model = new TestModel { Id = 1, Name = "Test" };
        var context = CreateOutputFormatterContext(model);

        // Act
        await _formatter.WriteAsync(context);

        // Assert
        context.HttpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        using (var reader = new StreamReader(context.HttpContext.Response.Body, Encoding.UTF8))
        {
            var responseContent = await reader.ReadToEndAsync();
            Assert.Equal("{\"Id\":1,\"Name\":\"Test\"}", responseContent);
        }
    }

    [Fact]
    public async Task WriteAsync_ShouldHandleNullObject()
    {
        // Arrange
        var context = CreateOutputFormatterContext(null);

        // Act
        await _formatter.WriteAsync(context);

        // Assert
        Assert.Equal(0, context.HttpContext.Response.Body.Length);  // Nothing should be written
    }

    [Fact]
    public async Task WriteAsync_ShouldHandleStringDirectly()
    {
        // Arrange
        var jsonString = "{\"Id\":1,\"Name\":\"Test\"}";
        var context = CreateOutputFormatterContext(jsonString);

        // Act
        await _formatter.WriteAsync(context);

        // Assert
        context.HttpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        using (var reader = new StreamReader(context.HttpContext.Response.Body, Encoding.UTF8))
        {
            var responseContent = await reader.ReadToEndAsync();
            Assert.Equal(jsonString, responseContent);
        }
    }

    private OutputFormatterWriteContext CreateOutputFormatterContext(object model)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();

        var context = new OutputFormatterWriteContext(
            httpContext,
            (stream, encoding) => new StreamWriter(stream, encoding),
            model?.GetType() ?? typeof(object),
            model
        );

        return context;
    }

    public class TestModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
