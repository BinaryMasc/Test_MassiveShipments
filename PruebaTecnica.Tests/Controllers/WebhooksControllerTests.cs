using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PruebaTecnica.Controllers;
using PruebaTecnica.Interfaces;
using PruebaTecnica.Models;
using Xunit;

namespace PruebaTecnica.Tests.Controllers;

public class WebhooksControllerTests
{
    [Fact]
    public async Task ReceiveShipmentWebhook_ValidPayload_ReturnsAccepted()
    {
        // Arrange
        var mockQueueService = new Mock<IWebhookQueueService>();
        mockQueueService.Setup(q => q.EnqueueAsync(It.IsAny<ShipmentWebhookPayload>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        var controller = new WebhooksController(mockQueueService.Object);
        var payload = new ShipmentWebhookPayload
        {
            EventId = "1",
            ProviderId = "P1",
            Status = "Shipped",
            TrackingNumber = "T1"
        };

        // Act
        var result = await controller.ReceiveShipmentWebhook(payload);

        // Assert
        var acceptedResult = Assert.IsType<AcceptedResult>(result);
        var response = Assert.IsType<WebhookAcceptedResponse>(acceptedResult.Value);
        Assert.NotEqual(Guid.Empty, response.ReceiptId);
        
        mockQueueService.Verify(q => q.EnqueueAsync(payload, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task ReceiveShipmentWebhook_NullPayload_ReturnsBadRequest()
    {
        // Arrange
        var mockQueueService = new Mock<IWebhookQueueService>();
        var controller = new WebhooksController(mockQueueService.Object);

        // Act
        var result = await controller.ReceiveShipmentWebhook(null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Payload cannot be null.", badRequestResult.Value);
        
        mockQueueService.Verify(q => q.EnqueueAsync(It.IsAny<ShipmentWebhookPayload>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
