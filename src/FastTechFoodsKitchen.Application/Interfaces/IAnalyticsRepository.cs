using FastTechFoodsKitchen.Domain.Entities;

namespace FastTechFoodsKitchen.Application.Interfaces;

public interface IAnalyticsRepository
{
    Task<List<Order>> GetOrdersAsync(DateTime? startDate = null, DateTime? endDate = null, string? status = null);
    Task<int> GetTotalOrdersCountAsync(DateTime? startDate = null, DateTime? endDate = null, string? status = null);
    Task<decimal> GetTotalRevenueAsync(DateTime? startDate = null, DateTime? endDate = null, string? status = null);
    Task<Dictionary<string, int>> GetOrdersByStatusAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<List<Order>> GetOrdersWithItemsAsync(DateTime? startDate = null, DateTime? endDate = null);
}
