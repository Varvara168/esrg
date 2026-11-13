namespace ChudoObuv.Models
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public int ClientId { get; set; }
        public string ClientFullName { get; set; }
        public int ItemsCount { get; set; }
        public decimal TotalSum { get; set; }
    }
}