using FastTechFoodsKitchen.Application.Interfaces;
using FastTechFoodsOrder.Shared.Integration.Messages;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace FastTechFoodsKitchen.Api.Consumers
{
    public class OrderUserCancelledMessageHandler : IOrderUserCancelledMessageHandler
    {
        private readonly IKitchenOrderService _kitchenOrderService;
        private readonly ILogger<OrderUserCancelledMessageHandler> _logger;
        private static readonly ActivitySource ActivitySource = new("FastTechFoodsKitchen.OrderUserCancelledMessageHandler");

        public OrderUserCancelledMessageHandler(
            IKitchenOrderService kitchenOrderService,
            ILogger<OrderUserCancelledMessageHandler> logger)
        {
            _kitchenOrderService = kitchenOrderService;
            _logger = logger;
        }

        public async Task HandleAsync(OrderCancelledMessage orderUserCancelledMessage)
        {
            using var activity = ActivitySource.StartActivity("ProcessOrderUserCancelled");
            activity?.SetTag("order.id", orderUserCancelledMessage.OrderId);
            activity?.SetTag("operation", "order.user.cancelled");

            try
            {
                _logger.LogInformation("Processing OrderUserCancelled for Order {OrderId}", orderUserCancelledMessage.OrderId);

                // Cancelar o pedido na cozinha
                await _kitchenOrderService.CancelOrderAsync(
                    orderUserCancelledMessage.OrderId, 
                    "Sistema", 
                    "Cancelado pelo usuário",
                    "Cancelamento originado do sistema de pedidos");

                _logger.LogInformation("OrderUserCancelled processed successfully for Order {OrderId}", orderUserCancelledMessage.OrderId);
                
                activity?.SetStatus(ActivityStatusCode.Ok);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing OrderUserCancelled for Order {OrderId}", orderUserCancelledMessage.OrderId);
                
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                throw;
            }
        }
    }
}
