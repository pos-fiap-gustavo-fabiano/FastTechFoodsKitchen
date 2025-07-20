using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FastTechFoodsKitchen.Domain.Entities
{
    public class Order
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        public string OrderId { get; set; } = string.Empty; // ID do pedido original
        public string CustomerId { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public decimal Total { get; set; }
        public decimal UnitPrice { get; set; } // Preço unitário do produto, se necessário
        public string Status { get; set; } = string.Empty;
        public List<KitchenOrderItem> Items { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string DeliveryMethod { get; set; }

    }

    public class KitchenOrderItem
    {
        public string ProductId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; } 
    }
}
