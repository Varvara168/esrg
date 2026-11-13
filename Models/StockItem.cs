namespace ChudoObuv.Models
{
    /// <summary> 
    /// Товарная позиция: модель + размер с доступным количеством. 
    /// </summary> 
    public class StockItem
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public decimal Size { get; set; }
        public int Quantity { get; set; }

        public string SizeText => Size.ToString("0.#");
    }
}