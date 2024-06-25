namespace Kolos1Poprawa.Models
{
    public class ClientDTO
    {
        public int id { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string adress { get; set; }
        public IEnumerable<Rentals> Rentals { get; set; } = new List<Rentals>();
    }

    public class Rentals
    {
        public string vin { get; set; }
        public string color { get; set; }
        public string model { get; set; }
        public DateTime dateFrom { get; set; }
        public DateTime dateTo { get; set; }
        public double totalPrice { get; set; }
    }
}