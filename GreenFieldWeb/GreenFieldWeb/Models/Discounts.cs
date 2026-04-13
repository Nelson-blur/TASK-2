namespace GreenFieldWeb.Models
{
    public class Discounts
    {
        public int DiscountsId { get; set; }
        public int DiscountName {  get; set; }
        public int DiscountCode { get; set; }
        public int DiscountPercentage { get; set; }
        public bool IsActive { get; set; }
        
        public ICollection<Products>? Products { get; set; }
        public ICollection<Orders>? Orders { get; set; }
    }
}
