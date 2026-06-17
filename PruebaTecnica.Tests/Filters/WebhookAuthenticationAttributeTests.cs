using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Moq;
using PruebaTecnica.Filters;
using Xunit;

namespace PruebaTecnica.Tests.Filters;

public class WebhookAuthenticationAttributeTests
{
    private AuthorizationFilterContext CreateContext(string tokenValue = null)
    {
        var httpContext = new DefaultHttpContext();
        if (tokenValue != null)
        {
            httpContext.Request.Headers["X-Webhook-Token"] = tokenValue;
        }

        var inMemorySettings = new Dictionary<string, string> {
            {"WebhookSettings:Token", "valid-token"}
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();

        var serviceProvider = new Mock<System.IServiceProvider>();
        serviceProvider.Setup(x => x.GetService(typeof(IConfiguration))).Returns(configuration);
        httpContext.RequestServices = serviceProvider.Object;

        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            new ActionDescriptor()
        );

        return new AuthorizationFilterContext(actionContext, new List<IFilterMetadata>());
    }

    [Fact]
    public void OnAuthorization_ValidToken_DoesNotSetResult()
    {
        // Arrange
        var filter = new WebhookAuthenticationAttribute();
        var context = CreateContext("valid-token");

        // Act
        filter.OnAuthorization(context);

        // Assert
        Assert.Null(context.Result); // Processing continues
    }

    [Fact]
    public void OnAuthorization_InvalidToken_SetsUnauthorizedResult()
    {
        // Arrange
        var filter = new WebhookAuthenticationAttribute();
        var context = CreateContext("invalid-token");

        // Act
        filter.OnAuthorization(context);

        // Assert
        Assert.IsType<UnauthorizedResult>(context.Result);
    }

    [Fact]
    public void OnAuthorization_MissingToken_SetsUnauthorizedResult()
    {
        // Arrange
        var filter = new WebhookAuthenticationAttribute();
        var context = CreateContext(); // No token provided

        // Act
        filter.OnAuthorization(context);

        // Assert
        Assert.IsType<UnauthorizedResult>(context.Result);
    }
}
