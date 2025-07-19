namespace FastTechFoodsKitchen.Application.DTOs
{
    public class UpdateOrderStatusDto
    {
        public string Status { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}
