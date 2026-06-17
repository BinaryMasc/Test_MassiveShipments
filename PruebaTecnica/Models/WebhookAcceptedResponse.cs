namespace PruebaTecnica.Models;

public class WebhookAcceptedResponse
{
    public Guid ReceiptId { get; set; }
    public string Message { get; set; } = "Webhook payload accepted for processing.";
}
