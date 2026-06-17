# PruebaTecnica - Webhook API

API de recepción de webhooks de envíos, diseñada con principios SOLID, inyección de dependencias, concurrencia (procesamiento en background mediante RabbitMQ) y limitación de tasa (rate limiting).

## Requisitos Previos
- .NET SDK 8.0 o superior.
- **RabbitMQ**: Debes tener una instancia de RabbitMQ corriendo localmente.

### Iniciar RabbitMQ con Docker
Para levantar una instancia local rápidamente con la interfaz de administración:
```bash
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```
*(El panel de administración estará disponible en `http://localhost:15672` con usuario/contraseña: `guest`/`guest`)*

## Ejecución
1. Navega a la carpeta del proyecto.
2. Ejecuta `dotnet run`.
3. La API estará expuesta en `http://localhost:<puerto>` o `https://localhost:<puerto>`.

## Endpoint Principal
**POST** `/api/v1/webhooks/shipments`

**Headers requeridos:**
`X-Webhook-Token`: `static-demo-token-12345`

**Payload de ejemplo:**
```json
{
  "eventId": "EVT-1001",
  "trackingNumber": "TRK123456789",
  "status": "InTransit",
  "timestamp": "2023-10-27T10:00:00Z",
  "providerId": "PROV-01"
}
```

## Simulación y Pruebas
Ejecuta el script `simulate_requests.sh` para probar la concurrencia, el límite de tasa de 10 peticiones por minuto, y observar cómo los mensajes viajan a través de RabbitMQ hacia el Worker en segundo plano.

## ToDos (Futuras Mejoras)
- **Autenticación**: Reemplazar el token estático por validación de firmas digitales HMAC.
- **Persistencia**: Añadir un servicio de base de datos para registrar permanentemente el estado de los envíos procesados.
