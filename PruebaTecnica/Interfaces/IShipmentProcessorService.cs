using System.Threading;
using System.Threading.Tasks;
using PruebaTecnica.Models;

namespace PruebaTecnica.Interfaces;

public interface IShipmentProcessorService
{
    /// <summary>
    /// Processes a shipment webhook payload (simulates business logic).
    /// </summary>
    /// <param name="payload">The webhook payload received.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    Task ProcessShipmentWebhookAsync(ShipmentWebhookPayload payload, CancellationToken cancellationToken = default);
}
