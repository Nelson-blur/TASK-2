using Microsoft.Identity.Client;

namespace GreenFieldWeb.Models
{
    public class Orders
    {
        public int OrdersId { get; set; }
        public string UserId { get; set; }
        public DateOnly OrderDate { get; set; }
        public string OrderStatus { get; set; }
        public string DeliveryMethod { get; set; }
        public DateOnly? DeliveryDate { get; set; }
        public string? DeliveryAddress { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal DiscountApplied { get; set; }
        public decimal TotalAmount { get; set; }

        public ICollection<OrderProducts>? OrderProducts { get; set; }
    }
}
