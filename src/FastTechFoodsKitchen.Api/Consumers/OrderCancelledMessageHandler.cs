using System.Diagnostics;
using FastTechFoodsKitchen.Application.Interfaces;
using FastTechFoodsOrder.Shared.Integration.Messages;

namespace FastTechFoodsKitchen.Api.Consumers
{
    public class OrderCancelledMessageHandler : IOrderCancelledMessageHandler
    {
        private readonly IKitchenOrderService _kitchenOrderService;
        private readonly ILogger<OrderCancelledMessageHandler> _logger;
        private static readonly ActivitySource ActivitySource = new("FastTechFoodsKitchen.OrderCancelledMessageHandler");

        public OrderCancelledMessageHandler(
            IKitchenOrderService kitchenOrderService,
            ILogger<OrderCancelledMessageHandler> logger)
        {
            _kitchenOrderService = kitchenOrderService;
            _logger = logger;
        }

        public async Task HandleAsync(OrderCancelledMessage orderCancelledMessage)
        {
            using var activity = ActivitySource.StartActivity("ProcessOrderCancelled");
            activity?.SetTag("order.id", orderCancelledMessage.OrderId);
            activity?.SetTag("cancellation.source", "kitchen");

            try
            {
                _logger.LogInformation("Processing OrderCancelled (Kitchen) for Order {OrderId}", orderCancelledMessage.OrderId);

                // Cancelar o pedido na cozinha (cancelamento pela própria cozinha)
                await _kitchenOrderService.CancelOrderAsync(
                    orderCancelledMessage.OrderId, 
                    orderCancelledMessage.CancelledBy ?? "System",
                    orderCancelledMessage.CancelReason ?? "Cancelled",
                    "Processed via OrderCancelledMessageHandler"
                );

                _logger.LogInformation("OrderCancelled (Kitchen) processed successfully for Order {OrderId}", orderCancelledMessage.OrderId);
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                _logger.LogError(ex, "Error processing OrderCancelled (Kitchen) for Order {OrderId}", orderCancelledMessage.OrderId);
                throw;
            }
        }
    }
}
