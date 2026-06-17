using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PruebaTecnica.Filters;
using PruebaTecnica.Interfaces;
using PruebaTecnica.Models;

namespace PruebaTecnica.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class WebhooksController : ControllerBase
{
    private readonly IWebhookQueueService _queueService;

    public WebhooksController(IWebhookQueueService queueService)
    {
        _queueService = queueService;
    }

    [HttpPost("shipments")]
    [WebhookAuthentication]
    [ProviderRateLimit]
    public async Task<IActionResult> ReceiveShipmentWebhook([FromBody] ShipmentWebhookPayload payload)
    {
        if (payload == null)
        {
            return BadRequest("Payload cannot be null.");
        }

        // Generate a unique receipt identifier
        var receiptId = Guid.NewGuid();

        // Send to queue asynchronously. 
        // Notice we do NOT wait for it to be fully processed by business logic, 
        // we just wait for it to be accepted into the internal channel buffer.
        await _queueService.EnqueueAsync(payload);

        // Return 202 Accepted immediately
        return Accepted(new WebhookAcceptedResponse
        {
            ReceiptId = receiptId,
            Message = "Webhook payload accepted for processing."
        });
    }
}
