namespace PruebaTecnica.Models;

public class ShipmentWebhookPayload
{
    public string EventId { get; set; } = string.Empty;
    public string TrackingNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string ProviderId { get; set; } = string.Empty;
}
