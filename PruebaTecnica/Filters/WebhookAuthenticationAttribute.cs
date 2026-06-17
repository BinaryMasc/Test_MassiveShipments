using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;

namespace PruebaTecnica.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class WebhookAuthenticationAttribute : Attribute, IAuthorizationFilter
{
    private const string TokenHeaderName = "X-Webhook-Token";

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var configuration = (IConfiguration?)context.HttpContext.RequestServices.GetService(typeof(IConfiguration));
        
        if (configuration == null)
        {
            context.Result = new StatusCodeResult(500);
            return;
        }

        string expectedToken = configuration.GetValue<string>("WebhookSettings:Token") ?? string.Empty;

        if (string.IsNullOrWhiteSpace(expectedToken))
        {
            // Token is not configured properly on the server
            context.Result = new StatusCodeResult(500);
            return;
        }

        if (!context.HttpContext.Request.Headers.TryGetValue(TokenHeaderName, out var providedToken))
        {
            // Token is missing
            context.Result = new UnauthorizedResult();
            return;
        }

        if (!string.Equals(providedToken, expectedToken, StringComparison.Ordinal))
        {
            // Token is invalid
            context.Result = new UnauthorizedResult();
            return;
        }
    }
}
