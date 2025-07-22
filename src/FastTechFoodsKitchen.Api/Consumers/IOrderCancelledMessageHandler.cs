using FastTechFoodsOrder.Shared.Integration.Messages;

namespace FastTechFoodsKitchen.Api.Consumers
{
    public interface IOrderCancelledMessageHandler
    {
        Task HandleAsync(OrderCancelledMessage orderCancelledMessage);
    }
}
