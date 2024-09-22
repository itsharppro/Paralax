using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Formatters;
using Moq;
using Xunit;
using Paralax.WebApi.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

public class JsonInputFormatterTests
{
    private readonly JsonInputFormatter _formatter;

    public JsonInputFormatterTests()
    {
        _formatter = new JsonInputFormatter();
    }

    [Fact]
    public void CanRead_ShouldAlwaysReturnTrue()
    {
        // Arrange
        var context = CreateInputFormatterContext(typeof(object));

        // Act
        var result = _formatter.CanRead(context);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ReadAsync_ShouldDeserializeValidJson()
    {
        // Arrange
        var modelType = typeof(TestModel);
        var json = "{\"Id\":1,\"Name\":\"Test\"}";
        var context = CreateInputFormatterContext(modelType, json);

        // Act
        var result = await _formatter.ReadAsync(context);

        // Assert
        var model = Assert.IsType<TestModel>(result.Model);
        Assert.Equal(1, model.Id);
        Assert.Equal("Test", model.Name);
    }

    [Fact]
    public async Task ReadAsync_ShouldReturnEmptyObjectForEmptyJson()
    {
        // Arrange
        var modelType = typeof(TestModel);
        var context = CreateInputFormatterContext(modelType, string.Empty);

        // Act
        var result = await _formatter.ReadAsync(context);

        // Assert
        var model = Assert.IsType<TestModel>(result.Model);
        Assert.NotNull(model);
    }

    [Fact]
    public async Task ReadAsync_ShouldThrowForInvalidJson()
    {
        // Arrange
        var modelType = typeof(TestModel);
        var json = "Invalid JSON}";  // This is malformed JSON
        var context = CreateInputFormatterContext(modelType, json);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await _formatter.ReadAsync(context));
    }


    [Fact]
    public async Task ReadAsync_ShouldBeThreadSafe()
    {
        // Arrange
        var modelType = typeof(TestModel);
        var json = "{\"Id\":1,\"Name\":\"Test\"}";
        var context1 = CreateInputFormatterContext(modelType, json);
        var context2 = CreateInputFormatterContext(modelType, json);

        // Act
        var task1 = _formatter.ReadAsync(context1);
        var task2 = _formatter.ReadAsync(context2);

        await Task.WhenAll(task1, task2);

        // Assert
        var result1 = await task1;
        var result2 = await task2;

        var model1 = result1.Model as TestModel;
        var model2 = result2.Model as TestModel;

        Assert.NotNull(model1);
        Assert.NotNull(model2);
        Assert.Equal(1, model1.Id);
        Assert.Equal("Test", model1.Name);
    }

    private InputFormatterContext CreateInputFormatterContext(Type modelType, string json = "")
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(json));

        var modelState = new ModelStateDictionary();
        var provider = new Mock<IModelMetadataProvider>();
        var metadata = new Mock<ModelMetadata>(ModelMetadataIdentity.ForType(modelType));

        var context = new InputFormatterContext(
            httpContext,
            nameof(modelType),
            modelState,
            metadata.Object,
            (stream, encoding) => new StreamReader(stream, encoding)
        );

        return context;
    }

    public class TestModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
