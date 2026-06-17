# PruebaTecnica Codebase

This document provides a high-level overview of the `PruebaTecnica` repository structure, architecture, and key components to help AI agents and developers quickly understand the codebase.

## 1. Architecture Overview

The application follows a clean, modular structure within a single ASP.NET Core project, adhering strictly to **SOLID principles** and **Dependency Injection**. It handles incoming webhook payloads, validates rate limits per provider, enforces authentication, and queues messages for asynchronous processing via **RabbitMQ**.

### Key Flows
1. **Request Intake**: `POST /api/v1/webhooks/shipments`
2. **Pre-Processing (Filters)**:
   - `WebhookAuthenticationAttribute`: Validates static token (`X-Webhook-Token`).
   - `ProviderRateLimitAttribute`: Validates 10 requests/minute limit per `ProviderId`.
3. **Queueing**: The Controller calls `IWebhookQueueService` to publish the payload to RabbitMQ and immediately returns `202 Accepted`.
4. **Background Processing**:
   - `WebhookProcessingWorker` listens to RabbitMQ.
   - Upon receiving a message, it delegates business logic to `IShipmentProcessorService`.
   - The processor simulates a 2-second delay.

## 2. Directory Structure

```text
PruebaTecnica/
├── Controllers/
│   └── WebhooksController.cs         # API entry points
├── Filters/
│   ├── ProviderRateLimitAttribute.cs # Rate limiting filter
│   └── WebhookAuthenticationAttribute.cs # Auth filter
├── Interfaces/
│   ├── IRateLimitingService.cs       # Rate limiter interface
│   ├── IShipmentProcessorService.cs  # Business logic interface
│   └── IWebhookQueueService.cs       # Queue publisher interface
├── Middlewares/
│   └── GlobalExceptionMiddleware.cs  # Unified error handling
├── Models/
│   ├── ShipmentWebhookPayload.cs     # Incoming request model
│   └── WebhookAcceptedResponse.cs    # 202 Response model
├── Services/
│   ├── RateLimitingService.cs        # In-Memory Cache rate limiting
│   ├── ShipmentProcessorService.cs   # Core business logic processing
│   └── WebhookQueueService.cs        # RabbitMQ publishing service
├── Workers/
│   └── WebhookProcessingWorker.cs    # RabbitMQ background consumer
├── simulate_requests.sh              # E2E test script
├── Program.cs                        # DI setup and middleware pipeline
└── appsettings.json                  # Configuration (RabbitMQ, Limits, Token)
```

## 3. Core Components

### `WebhooksController.cs`
Handles the REST API layer. It has no business logic. It relies on Action Filters for validation and delegates to `IWebhookQueueService` for enqueueing.

### `WebhookQueueService.cs` (RabbitMQ Publisher)
Responsible solely for connecting to RabbitMQ using the `IConnectionFactory` and publishing the `ShipmentWebhookPayload` as a JSON byte array to the `shipments_queue`.

### `WebhookProcessingWorker.cs` (RabbitMQ Consumer)
An `IHostedService` (`BackgroundService`). Connects to RabbitMQ, sets up an `AsyncEventingBasicConsumer`, and listens for messages on `shipments_queue`. It acknowledges (`BasicAckAsync`) successful processing or rejects (`BasicNackAsync`) on failures. Delegates the actual work to `IShipmentProcessorService`.

### `ShipmentProcessorService.cs` (Business Logic)
Contains the actual domain logic for shipments. Currently, it logs the data and introduces a simulated `Task.Delay(2000)`.

### `RateLimitingService.cs` & `ProviderRateLimitAttribute.cs`
Uses a fixed-window algorithm backed by `IMemoryCache`. The attribute runs *after* Model Binding so it can inspect the `ShipmentWebhookPayload.ProviderId` directly.

## 4. Testing (`PruebaTecnica.Tests`)
Unit tests are written using **xUnit** and **Moq**. They cover Controllers, Services, Filters, and the Worker initialization logic. They ensure boundaries and interfaces are respected without requiring a live RabbitMQ server.
