using Microsoft.Identity.Client;

namespace GreenFieldWeb.Models
{
    public class Orders
    {
        public int OrdersId { get; set; } // Primary Key for the Orders table
        public string UserId { get; set; } // Links Each Order to a User
        public DateOnly OrderDate { get; set; } // Timestamp for when the Order was placed
        public string OrderStatus { get; set; }// Indicates the current status of the order (e.g., "Pending", "Shipped", "Delivered", "Cancelled")
        public string DeliveryMethod { get; set; }// Indicates the method of delivery chosen for the order (e.g., "Standard Shipping", "Express Shipping", "In-Store Pickup")
        public DateOnly? DeliveryDate { get; set; }// Optional timestamp for when the order is expected to be delivered, can be null if the delivery date is not yet determined
        public string? DeliveryAddress { get; set; }// Optional field for the delivery address, can be null if the order is for in-store pickup or if the delivery address is not provided
        public decimal DeliveryFee { get; set; }// The fee associated with the chosen delivery method, can be zero for free delivery options
        public decimal DiscountApplied { get; set; }// The amount of any discount applied to the order, can be zero if no discounts are applied
        public decimal TotalAmount { get; set; }// The total amount for the order, including the cost of products, delivery fee, and after applying any discounts

        public ICollection<OrderProducts>? OrderProducts { get; set; }// Navigation property to access the products included in the order, can be null if the order has no products (though in practice, an order should typically have at least one product)
    }
}
