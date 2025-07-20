using MongoDB.Driver;

namespace FastTechFoodsKitchen.Application.Interfaces;

public interface IApplicationDbContext
{
    IMongoCollection<T> GetCollection<T>(string name);
}
