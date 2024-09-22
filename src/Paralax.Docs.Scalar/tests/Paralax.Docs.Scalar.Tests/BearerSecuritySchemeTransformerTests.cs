using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.DependencyInjection; // For IServiceProvider
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer; // For IAuthenticationHandler

public class BearerSecuritySchemeTransformerTests
{
    private readonly Mock<IAuthenticationSchemeProvider> _authenticationSchemeProviderMock;
    private readonly BearerSecuritySchemeTransformer _transformer;

    // Mock AuthenticationHandler for testing
    public class TestAuthenticationHandler : IAuthenticationHandler
    {
        public Task<AuthenticateResult> AuthenticateAsync() => Task.FromResult(AuthenticateResult.NoResult());
        public Task ChallengeAsync(AuthenticationProperties properties) => Task.CompletedTask;
        public Task ForbidAsync(AuthenticationProperties properties) => Task.CompletedTask;
        public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context) => Task.CompletedTask;
    }

    public BearerSecuritySchemeTransformerTests()
    {
        _authenticationSchemeProviderMock = new Mock<IAuthenticationSchemeProvider>();
        _transformer = new BearerSecuritySchemeTransformer(_authenticationSchemeProviderMock.Object);
    }

    private OpenApiDocumentTransformerContext CreateTransformerContext()
    {
        // Create a mock service provider if needed
        var serviceProviderMock = new Mock<IServiceProvider>();

        // Initialize the OpenApiDocumentTransformerContext with required members
        return new OpenApiDocumentTransformerContext
        {
            DocumentName = "TestDocument",
            DescriptionGroups = new List<ApiDescriptionGroup>(),
            ApplicationServices = serviceProviderMock.Object
        };
    }

    [Fact]
    public async Task TransformAsync_Should_Add_BearerSecurityScheme_When_BearerScheme_Exists()
    {
        // Arrange
        var authenticationSchemes = new List<AuthenticationScheme>
        {
            new AuthenticationScheme("Bearer", "Bearer", typeof(TestAuthenticationHandler)) // Corrected handler type
        };
        _authenticationSchemeProviderMock.Setup(x => x.GetAllSchemesAsync())
                                         .ReturnsAsync(authenticationSchemes);

        var document = new OpenApiDocument
        {
            Components = new OpenApiComponents(),
            Paths = new OpenApiPaths()
        };
        var context = CreateTransformerContext(); // Use the method to create a valid context

        // Act
        await _transformer.TransformAsync(document, context, CancellationToken.None);

        // Assert
        document.Components.SecuritySchemes.Should().ContainKey("Bearer");
        var bearerScheme = document.Components.SecuritySchemes["Bearer"];
        bearerScheme.Should().NotBeNull();
        bearerScheme.Type.Should().Be(SecuritySchemeType.Http);
        bearerScheme.Scheme.Should().Be("bearer");
        bearerScheme.In.Should().Be(ParameterLocation.Header);
        bearerScheme.BearerFormat.Should().Be("JWT");
    }

    [Fact]
    public async Task TransformAsync_Should_Add_SecurityRequirement_To_All_Operations_When_BearerScheme_Exists()
    {
        // Arrange
        var authenticationSchemes = new List<AuthenticationScheme>
        {
            new AuthenticationScheme("Bearer", "Bearer", typeof(TestAuthenticationHandler)) // Corrected handler type
        };
        _authenticationSchemeProviderMock.Setup(x => x.GetAllSchemesAsync())
                                         .ReturnsAsync(authenticationSchemes);

        var document = new OpenApiDocument
        {
            Components = new OpenApiComponents(),
            Paths = new OpenApiPaths
            {
                ["/test"] = new OpenApiPathItem
                {
                    Operations = new Dictionary<OperationType, OpenApiOperation>
                    {
                        [OperationType.Get] = new OpenApiOperation(),
                        [OperationType.Post] = new OpenApiOperation()
                    }
                }
            }
        };
        var context = CreateTransformerContext(); // Use the method to create a valid context

        // Act
        await _transformer.TransformAsync(document, context, CancellationToken.None);

        // Assert
        foreach (var operation in document.Paths.Values.SelectMany(p => p.Operations.Values))
        {
            operation.Security.Should().NotBeNull();
            operation.Security.Should().ContainSingle();
            var securityRequirement = operation.Security.First();
            securityRequirement.Should().ContainKey(new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Id = "Bearer", Type = ReferenceType.SecurityScheme }
            });
        }
    }

    [Fact]
    public async Task TransformAsync_Should_Not_Add_BearerSecurityScheme_When_BearerScheme_Does_Not_Exist()
    {
        // Arrange
        var authenticationSchemes = new List<AuthenticationScheme>();
        _authenticationSchemeProviderMock.Setup(x => x.GetAllSchemesAsync())
                                         .ReturnsAsync(authenticationSchemes);

        var document = new OpenApiDocument
        {
            Components = new OpenApiComponents(),
            Paths = new OpenApiPaths()
        };
        var context = CreateTransformerContext(); // Use the method to create a valid context

        // Act
        await _transformer.TransformAsync(document, context, CancellationToken.None);

        // Assert
        document.Components.SecuritySchemes.Should().BeEmpty();
    }
}
