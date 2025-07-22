using FastTechFoodsOrder.Shared.Integration.Messages;

namespace FastTechFoodsKitchen.Api.Consumers
{
    public interface IOrderUserCancelledMessageHandler
    {
        Task HandleAsync(OrderCancelledMessage orderUserCancelledMessage);
    }
}
