namespace GreenFieldWeb.Models
{
    public class BasketProducts
    {
        public int BasketProductsId { get; set; }// Primary Key for the BasketProducts table
        public int BasketId { get; set; }// Foreign Key linking to the Basket table
        public int ProductsId { get; set; }// Foreign Key linking to the Products table
        public int Quantity {  get; set; }// Quantity of the product added to the basket
        public Products Products { get; set; }  // Navigation property to access the product details, cannot be null because a basket product must always be associated with a product  
        public Basket Basket { get; set; }// Navigation property to access the basket details, cannot be null because a basket product must always be associated with a basket
    }
}
