namespace LibraryApiClient.Models
{
    public class Order
    {
        public Guid OrderId { get; set; }
        public List<OrderLine> OrderLines { get; set; } = new();
    }
}
