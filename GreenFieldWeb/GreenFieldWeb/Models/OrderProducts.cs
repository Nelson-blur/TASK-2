namespace GreenFieldWeb.Models
{
    public class OrderProducts
    {
        public int OrderProductsId { get; set; }
        public int OrdersId { get; set; }
        public int ProductsId { get; set; }
        public int Quantity { get; set; }
        public Products Products { get; set; }
        public Orders Orders { get; set; }

    }
}
