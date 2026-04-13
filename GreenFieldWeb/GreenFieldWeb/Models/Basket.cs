namespace GreenFieldWeb.Models
{
    public class Basket
    {
        public int BasketId { get; set; }
        public string UserId { get; set; } // Links Each Basket to a User 
        public bool Status { get; set; }    
        public DateTime BasketCreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<BasketProducts>? BasketProducts { get; set; }
    }
}
