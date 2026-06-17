using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PruebaTecnica.Interfaces;
using PruebaTecnica.Middlewares;
using PruebaTecnica.Services;
using PruebaTecnica.Workers;
using RabbitMQ.Client;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Memory Cache for Rate Limiting
builder.Services.AddMemoryCache();

// Register RabbitMQ Connection Factory
builder.Services.AddSingleton<IConnectionFactory>(sp => 
{
    var config = builder.Configuration;
    return new ConnectionFactory
    {
        HostName = config["RabbitMQ:HostName"] ?? "localhost",
        UserName = config["RabbitMQ:UserName"] ?? "guest",
        Password = config["RabbitMQ:Password"] ?? "guest",
        Port = int.TryParse(config["RabbitMQ:Port"], out var port) ? port : 5672
    };
});

// Register application services
builder.Services.AddSingleton<IWebhookQueueService, WebhookQueueService>();
builder.Services.AddSingleton<IRateLimitingService, RateLimitingService>();
builder.Services.AddSingleton<IShipmentProcessorService, ShipmentProcessorService>();

// Register background workers
builder.Services.AddHostedService<WebhookProcessingWorker>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Use Global Exception Handler
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
