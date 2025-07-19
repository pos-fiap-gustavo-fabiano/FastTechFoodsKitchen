using FastTechFoodsOrder.Shared.Integration.Messages;

namespace FastTechFoodsKitchen.Api.Consumers
{
    public interface IOrderCreatedMessageHandler
    {
        Task HandleAsync(OrderCreatedMessage message);
    }
}
