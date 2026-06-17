using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using PruebaTecnica.Interfaces;
using PruebaTecnica.Models;

namespace PruebaTecnica.Workers;

public class WebhookProcessingWorker : BackgroundService
{
    private readonly IConnectionFactory _connectionFactory;
    private readonly IShipmentProcessorService _processorService;
    private readonly ILogger<WebhookProcessingWorker> _logger;
    private const string QueueName = "shipments_queue";
    private IConnection? _connection;
    private IChannel? _channel;

    public WebhookProcessingWorker(
        IConnectionFactory connectionFactory, 
        IShipmentProcessorService processorService, 
        ILogger<WebhookProcessingWorker> logger)
    {
        _connectionFactory = connectionFactory;
        _processorService = processorService;
        _logger = logger;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        try 
        {
            _connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
            _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

            await _channel.QueueDeclareAsync(
                queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken);

            _logger.LogInformation("Webhook Processing Worker connected to RabbitMQ and listening on {QueueName}.", QueueName);
            
            await base.StartAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start Webhook Processing Worker. Is RabbitMQ running?");
            throw;
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_channel == null) return;

        var consumer = new AsyncEventingBasicConsumer(_channel);
        
        consumer.ReceivedAsync += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);
                var payload = JsonSerializer.Deserialize<ShipmentWebhookPayload>(json);

                if (payload != null)
                {
                    await _processorService.ProcessShipmentWebhookAsync(payload, stoppingToken);
                }

                await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message from RabbitMQ.");
                // Reject message and requeue
                await _channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true, cancellationToken: stoppingToken);
            }
        };

        await _channel.BasicConsumeAsync(queue: QueueName, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);

        // Keep the worker alive
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Webhook Processing Worker is stopping.");
        
        if (_channel != null)
        {
            await _channel.CloseAsync(cancellationToken);
            _channel.Dispose();
        }
        
        if (_connection != null)
        {
            await _connection.CloseAsync(cancellationToken);
            _connection.Dispose();
        }

        await base.StopAsync(cancellationToken);
    }
}
