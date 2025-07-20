using FastTechFoodsKitchen.Application.Interfaces;
using FastTechFoodsKitchen.Domain.Entities;
using FastTechFoodsKitchen.Infrastructure.Context;
using MongoDB.Driver;

namespace FastTechFoodsKitchen.Infrastructure.Repositories;

public class AnalyticsRepository : IAnalyticsRepository
{
    private readonly IMongoCollection<Order> _ordersCollection;

    public AnalyticsRepository(ApplicationDbContext context)
    {
        _ordersCollection = context.GetCollection<Order>("orders");
    }

    public async Task<List<Order>> GetOrdersAsync(DateTime? startDate = null, DateTime? endDate = null, string? status = null)
    {
        var filter = BuildOrderFilter(startDate, endDate, status);
        return await _ordersCollection.Find(filter).ToListAsync();
    }

    public async Task<int> GetTotalOrdersCountAsync(DateTime? startDate = null, DateTime? endDate = null, string? status = null)
    {
        var filter = BuildOrderFilter(startDate, endDate, status);
        return (int)await _ordersCollection.CountDocumentsAsync(filter);
    }

    public async Task<decimal> GetTotalRevenueAsync(DateTime? startDate = null, DateTime? endDate = null, string? status = null)
    {
        var filter = BuildOrderFilter(startDate, endDate, status);
        var orders = await _ordersCollection.Find(filter).ToListAsync();
        return orders.Sum(o => o.Total);
    }

    public async Task<Dictionary<string, int>> GetOrdersByStatusAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var filter = BuildOrderFilter(startDate, endDate);
        var orders = await _ordersCollection.Find(filter).ToListAsync();
        
        return orders
            .GroupBy(o => o.Status)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task<List<Order>> GetOrdersWithItemsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var filter = BuildOrderFilter(startDate, endDate);
        return await _ordersCollection.Find(filter).ToListAsync();
    }

    private FilterDefinition<Order> BuildOrderFilter(DateTime? startDate = null, DateTime? endDate = null, string? status = null)
    {
        var builder = Builders<Order>.Filter;
        var filters = new List<FilterDefinition<Order>>();

        if (startDate.HasValue)
            filters.Add(builder.Gte(o => o.OrderDate, startDate.Value));

        if (endDate.HasValue)
            filters.Add(builder.Lte(o => o.OrderDate, endDate.Value));

        if (!string.IsNullOrEmpty(status))
            filters.Add(builder.Eq(o => o.Status, status));

        return filters.Count > 0 ? builder.And(filters) : builder.Empty;
    }
}
