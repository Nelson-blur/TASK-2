namespace GreenFieldWeb.Models
{
    public class Products
    {
        public int ProductsId { get; set; }
        public int ProducersId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Stock {  get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsAvailable { get; set; }
        public string AllergenInformation { get; set; }
        public string FarmingMethod { get; set; }
        public string? ImageUrl { get; set; }
        public Producers Producers { get; set; } 

        public ICollection<OrderProducts>? OrderProducts { get; set; }
        public ICollection<BasketProducts>? BasketProducts { get; set; }

    }
}
