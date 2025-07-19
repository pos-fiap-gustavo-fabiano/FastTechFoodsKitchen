namespace FastTechFoodsKitchen.Application.DTOs.External
{
    public class ExternalOrderDto
    {
        public string Id { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }
        // Adicione outras propriedades conforme necessário
    }
}
