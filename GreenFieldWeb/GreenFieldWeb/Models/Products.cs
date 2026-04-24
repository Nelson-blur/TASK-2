namespace GreenFieldWeb.Models
{
    public class Products// This class represents the products that producers can list on the platform. It includes properties such as product name, price, stock, description, and other relevant information. It also has navigation properties to link to the Producers class and the OrderProducts and BasketProducts classes for managing orders and baskets.
    {
        public int ProductsId { get; set; }// Primary Key for the Products table
        public int ProducersId { get; set; }// Foreign Key linking to the Producers table, indicating which producer listed the product
        public string ProductName { get; set; }// The name of the product, which should be descriptive and help customers understand what the product is
        public decimal Price { get; set; }// The price of the product, which should be a positive decimal value representing the cost of one unit of the product
        public int Stock {  get; set; }// The available stock for the product, which should be a non-negative integer indicating how many units of the product are currently available for purchase
        public string Description { get; set; }// A detailed description of the product, providing customers with information about the product's features, benefits, and any other relevant details that can help them make informed purchasing decisions
        public DateTime CreatedAt { get; set; }// The date and time when the product was created, which can be used for tracking and sorting products based on their creation date
        public DateTime UpdatedAt { get; set; }// The date and time when the product was last updated, which can be used for tracking changes to the product information and ensuring that customers see the most up-to-date details
        public bool IsAvailable { get; set; }// A boolean value indicating whether the product is currently available for purchase, which can be used to manage product listings and prevent customers from ordering products that are out of stock or temporarily unavailable
        public string AllergenInformation { get; set; }// Information about any allergens that may be present in the product, which is important for customers with allergies or dietary restrictions to make informed purchasing decisions
        public string FarmingMethod { get; set; }// Information about the farming method used to produce the product, which can be important for customers who are interested in sustainable or organic products and want to know more about how the product was produced
        public string? ImageUrl { get; set; }// An optional URL for an image of the product, which can help customers visualize the product and make it more appealing when browsing the product listings
        public Producers Producers { get; set; } // Navigation property to access the producer details, cannot be null because a product must always be associated with a producer

        public ICollection<OrderProducts>? OrderProducts { get; set; }// Navigation property to access the orders that include this product, can be null if the product has not been included in any orders yet
        public ICollection<BasketProducts>? BasketProducts { get; set; }// Navigation property to access the baskets that include this product, can be null if the product has not been added to any baskets yet

    }
}
