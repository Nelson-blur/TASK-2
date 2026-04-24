namespace GreenFieldWeb.Models
{
    public class OrderProducts// This class represents the many-to-many relationship between Orders and Products, allowing us to track which products are included in each order and in what quantity.
    {
        public int OrderProductsId { get; set; }// Primary Key for the OrderProducts table
        public int OrdersId { get; set; }// Foreign Key linking to the Orders table
        public int ProductsId { get; set; }// Foreign Key linking to the Products table
        public int Quantity { get; set; }// Quantity of the product included in the order
        public Products Products { get; set; }// Navigation property to access the product details, cannot be null because an order product must always be associated with a product
        public Orders Orders { get; set; }// Navigation property to access the order details, cannot be null because an order product must always be associated with an order

    }
}
