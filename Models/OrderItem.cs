namespace ChudoObuv.Models
{
    /// <summary> 
    /// Строка состава заказа (сохраненная в БД или в оформляемом заказе). 
    /// </summary> 
    public class OrderItem
    {
        public int Id { get; set; }
        public int StockItemId { get; set; }
        public string ProductName { get; set; }
        public string Manufacturer { get; set; }
        public decimal Size { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        public string Position => $"{ProductName}, {Manufacturer}, размер {Size:0.#}";

        public string SizeText => Size.ToString("0.#");

        public decimal Sum => UnitPrice * Quantity;
    }
}