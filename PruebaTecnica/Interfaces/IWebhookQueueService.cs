using System.Threading;
using System.Threading.Tasks;
using PruebaTecnica.Models;

namespace PruebaTecnica.Interfaces;

public interface IWebhookQueueService
{
    /// <summary>
    /// Enqueues a webhook payload for background processing.
    /// </summary>
    /// <param name="payload">The webhook payload received.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A ValueTask representing the asynchronous operation.</returns>
    ValueTask EnqueueAsync(ShipmentWebhookPayload payload, CancellationToken cancellationToken = default);
}
