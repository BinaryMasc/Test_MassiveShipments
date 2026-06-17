using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PruebaTecnica.Interfaces;
using PruebaTecnica.Models;

namespace PruebaTecnica.Filters;

[AttributeUsage(AttributeTargets.Method)]
public class ProviderRateLimitAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var rateLimitingService = (IRateLimitingService?)context.HttpContext.RequestServices.GetService(typeof(IRateLimitingService));

        if (rateLimitingService == null)
        {
            context.Result = new StatusCodeResult(500);
            return;
        }

        // Search for the ShipmentWebhookPayload in the action arguments
        ShipmentWebhookPayload? payload = null;
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is ShipmentWebhookPayload p)
            {
                payload = p;
                break;
            }
        }

        if (payload != null && !string.IsNullOrWhiteSpace(payload.ProviderId))
        {
            bool isAllowed = rateLimitingService.IsRequestAllowed(payload.ProviderId);
            
            if (!isAllowed)
            {
                // Rate limit exceeded for this ProviderId
                context.Result = new StatusCodeResult(429); // Too Many Requests
                return;
            }
        }

        base.OnActionExecuting(context);
    }
}
