namespace GreenFieldWeb.Models
{
    public class Producers
    {
        public int ProducersId { get; set; }// Primary Key for the Producers table
        public string UserId { get; set; }// Links Each Producer to a User, allowing us to associate producer accounts with their respective user profiles
        public string ProducerName { get; set; }// The name of the producer, which can be a farm, artisan, or any entity that produces goods for sale on the platform
        public string Description { get ; set; }// A brief description of the producer, providing potential customers with information about the producer's background, values, and the types of products they offer
        public string BusinessLocation { get; set; }// The physical location of the producer's business, which can help customers understand where their products are coming from and may also be used for logistical purposes such as delivery or pickup arrangements
        public string ContactEmail { get; set; }// The email address for contacting the producer, allowing customers to reach out with inquiries, orders, or feedback

        public ICollection<Products>? Products { get; set; }// Navigation property to access the products offered by the producer, can be null if the producer has not listed any products yet

    }
}
