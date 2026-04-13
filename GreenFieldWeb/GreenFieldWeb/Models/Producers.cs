namespace GreenFieldWeb.Models
{
    public class Producers
    {
        public int ProducersId { get; set; }
        public string UserId { get; set; }
        public string ProducerName { get; set; }
        public string Description { get ; set; }
        public string BusinessLocation { get; set; }
        public string ContactEmail { get; set; }

        public ICollection<Products>? Products { get; set; }
   
    }
}
