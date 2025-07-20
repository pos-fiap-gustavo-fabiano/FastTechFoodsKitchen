using FastTechFoodsKitchen.Application.DTOs.Analytics;

namespace FastTechFoodsKitchen.Application.Interfaces;

public interface IAnalyticsService
{
    Task<DashboardAnalyticsDto> GetDashboardAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null, string? status = null);
    Task<List<OrderStatusDistributionDto>> GetOrdersByStatusAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<List<ProductSalesDto>> GetTopProductsAsync(int top = 5, DateTime? startDate = null, DateTime? endDate = null);
}
