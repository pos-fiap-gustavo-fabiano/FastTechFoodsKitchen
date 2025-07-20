using FastTechFoodsKitchen.Application.Interfaces;
using MongoDB.Driver;

namespace FastTechFoodsKitchen.Infrastructure.Context;

public class MongoDbContext : IApplicationDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(ApplicationDbContext applicationDbContext)
    {
        // Reutilizar a conexão existente do ApplicationDbContext
        var mongoClient = new MongoClient(Environment.GetEnvironmentVariable("CONNECTION_STRING_DATABASE") ?? "mongodb://localhost:27017");
        _database = mongoClient.GetDatabase("FastTechFoodsKitchen");
    }

    public IMongoCollection<T> GetCollection<T>(string name) =>
        _database.GetCollection<T>(name);
}
