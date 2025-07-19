using FastTechFoodsKitchen.Application.Interfaces;
using FastTechFoodsKitchen.Domain.Entities;
using FastTechFoodsKitchen.Infrastructure.Context;
using MongoDB.Driver;

namespace FastTechFoodsKitchen.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly IMongoCollection<Order> _orders;

        public OrderRepository(ApplicationDbContext context)
        {
            _orders = context.GetCollection<Order>("orders");
        }

        public async Task<Order> CreateAsync(Order order)
        {
            await _orders.InsertOneAsync(order);
            return order;
        }

        public async Task<Order?> GetByIdAsync(string id)
        {
            return await _orders.Find(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Order?> GetByOrderIdAsync(string orderId)
        {
            return await _orders.Find(x => x.OrderId == orderId).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            return await _orders.Find(_ => true).SortByDescending(x => x.CreatedAt).ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetByStatusAsync(string status)
        {
            return await _orders.Find(x => x.Status == status).SortByDescending(x => x.CreatedAt).ToListAsync();
        }

        public async Task<Order> UpdateAsync(Order order)
        {
            order.UpdatedAt = DateTime.UtcNow;
            
            var result = await _orders.ReplaceOneAsync(x => x.Id == order.Id, order);
            
            if (result.MatchedCount == 0)
                throw new InvalidOperationException($"Order with Id {order.Id} not found");
                
            return order;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _orders.DeleteOneAsync(x => x.Id == id);
            return result.DeletedCount > 0;
        }

        public async Task<IEnumerable<Order>> GetByCustomerIdAsync(string customerId)
        {
            return await _orders.Find(x => x.CustomerId == customerId).ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _orders.Find(x => x.OrderDate >= startDate && x.OrderDate <= endDate).ToListAsync();
        }
    }
}
