namespace Kolos1Poprawa.Models
{
    public class PostClientDTO
    {
        public ClientToPost client { get; set; }
        public int carId { get; set; }
        public DateTime dateFrom { get; set; }
        public DateTime dateTo { get; set; }
    }

    public class ClientToPost
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string adress { get; set; }
    }
}