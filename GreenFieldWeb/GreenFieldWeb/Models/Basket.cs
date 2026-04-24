namespace GreenFieldWeb.Models
{
    public class Basket
    {
        public int BasketId { get; set; } // Primary Key for the Basket table
        public string UserId { get; set; } // Links Each Basket to a User 
        public bool Status { get; set; }    // Indicates if the Basket is Active (true) or has been Checked Out (false)
        public DateTime BasketCreatedAt { get; set; } = DateTime.UtcNow;// Timestamp for when the Basket was created

        public ICollection<BasketProducts>? BasketProducts { get; set; }// Navigation property to access the products in the basket, can be null if the basket is empty
    }
}
