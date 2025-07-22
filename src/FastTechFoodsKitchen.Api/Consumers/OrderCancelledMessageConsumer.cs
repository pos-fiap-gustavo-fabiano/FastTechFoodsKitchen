using System.Text;
using System.Text.Json;
using FastTechFoodsOrder.Shared.Integration.Messages;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FastTechFoodsKitchen.Api.Consumers
{
    public class OrderCancelledMessageConsumer : BackgroundService
    {
        private readonly IConnection _connection;
        private readonly ILogger<OrderCancelledMessageConsumer> _logger;
        private readonly IServiceProvider _serviceProvider;
        private IChannel? _channel;

        public OrderCancelledMessageConsumer(
            IConnection connection,
            ILogger<OrderCancelledMessageConsumer> logger,
            IServiceProvider serviceProvider)
        {
            _connection = connection;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("OrderCancelledMessageConsumer started");

            _channel = await _connection.CreateChannelAsync();

            // Declara a fila
            await _channel.QueueDeclareAsync(
                queue: "order.cancelled.queue",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            // Configura o consumer
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    _logger.LogInformation("Received OrderCancelledMessage: {Message}", message);

                    var orderCancelledMessage = JsonSerializer.Deserialize<OrderCancelledMessage>(message, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (orderCancelledMessage != null)
                    {
                        await ProcessOrderCancelledMessageAsync(orderCancelledMessage);
                        await _channel.BasicAckAsync(ea.DeliveryTag, false);
                        _logger.LogInformation("OrderCancelledMessage processed successfully for Order {OrderId}", orderCancelledMessage.OrderId);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to deserialize OrderCancelledMessage");
                        await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing OrderCancelledMessage");
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
                }
            };

            // Inicia o consumo
            await _channel.BasicConsumeAsync(
                queue: "order.cancelled.queue",
                autoAck: false,
                consumer: consumer);

            _logger.LogInformation("OrderCancelledMessageConsumer is listening for messages on queue: order.cancelled.queue");

            // Mantém o serviço rodando
            try
            {
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("OrderCancelledMessageConsumer stopping...");
            }
        }

        private async Task ProcessOrderCancelledMessageAsync(OrderCancelledMessage orderCancelledMessage)
        {
            using var scope = _serviceProvider.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<IOrderCancelledMessageHandler>();
            await handler.HandleAsync(orderCancelledMessage);
        }

        public override void Dispose()
        {
            if (_channel != null)
            {
                _channel.CloseAsync().GetAwaiter().GetResult();
                _channel.DisposeAsync().GetAwaiter().GetResult();
            }
            
            base.Dispose();
        }
    }
}
