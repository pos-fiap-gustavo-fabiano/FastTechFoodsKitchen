using FastTechFoodsKitchen.Application.Interfaces;
using FastTechFoodsOrder.Shared.Integration.Messages;
using Microsoft.Extensions.Logging;

namespace FastTechFoodsKitchen.Api.Consumers
{
    public class OrderCreatedMessageHandler : IOrderCreatedMessageHandler
    {
        private readonly ILogger<OrderCreatedMessageHandler> _logger;
        private readonly IKitchenOrderService _kitchenOrderService;

        public OrderCreatedMessageHandler(
            ILogger<OrderCreatedMessageHandler> logger,
            IKitchenOrderService kitchenOrderService)
        {
            _logger = logger;
            _kitchenOrderService = kitchenOrderService;
        }

        public async Task HandleAsync(OrderCreatedMessage message)
        {
            _logger.LogInformation("Handling OrderCreatedMessage for Order {OrderId}", message.OrderId);

            try
            {
                await _kitchenOrderService.ProcessOrderCreatedAsync(message);
                _logger.LogInformation("OrderCreatedMessage successfully handled for Order {OrderId}", message.OrderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling OrderCreatedMessage for Order {OrderId}", message.OrderId);
                throw; // Re-throw para que o consumer possa fazer o nack
            }
        }
    }
}
