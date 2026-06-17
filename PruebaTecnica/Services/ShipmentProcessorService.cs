using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PruebaTecnica.Interfaces;
using PruebaTecnica.Models;

namespace PruebaTecnica.Services;

public class ShipmentProcessorService : IShipmentProcessorService
{
    private readonly ILogger<ShipmentProcessorService> _logger;

    public ShipmentProcessorService(ILogger<ShipmentProcessorService> logger)
    {
        _logger = logger;
    }

    public async Task ProcessShipmentWebhookAsync(ShipmentWebhookPayload payload, CancellationToken cancellationToken = default)
    {
        if (payload == null)
        {
            throw new ArgumentNullException(nameof(payload));
        }

        _logger.LogInformation(
            "Processing Webhook - Provider: {ProviderId}, EventId: {EventId}, Status: {Status}, TrackingNumber: {TrackingNumber}",
            payload.ProviderId, payload.EventId, payload.Status, payload.TrackingNumber);

        // Simulate business logic processing delay
        await Task.Delay(2000, cancellationToken);

        _logger.LogInformation("Successfully processed Webhook EventId: {EventId}", payload.EventId);
    }
}
