using FastTechFoodsKitchen.Domain.Entities;

namespace FastTechFoodsKitchen.Application.Interfaces
{
    public interface IOrderRepository
    {
        Task<Order> CreateAsync(Order order);
        Task<Order?> GetByIdAsync(string id);
        Task<Order?> GetByOrderIdAsync(string orderId);
        Task<IEnumerable<Order>> GetAllAsync();
        Task<IEnumerable<Order>> GetByStatusAsync(string status);
        Task<Order> UpdateAsync(Order order);
        Task<bool> DeleteAsync(string id);
        Task<IEnumerable<Order>> GetByCustomerIdAsync(string customerId);
        Task<IEnumerable<Order>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}
