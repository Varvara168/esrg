using System.IO;
using ChudoObuv.Logic;
namespace ChudoObuv.Models
{
    /// <summary>
    /// Товар (модель обуви) для вывода в каталоге.
    /// </summary>
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Subcategory { get; set; }
        public string Manufacturer { get; set; }
        public string Description { get; set; }
        public string Composition { get; set; }
        public decimal Price { get; set; }
        public string ImageFile { get; set; }
        public int TotalQuantity { get; set; }
        public bool HasOrdersInPreviousMonth { get; set; }
        public string Title => $"{Manufacturer} | {Name}";
        public decimal DiscountedPrice => PriceCalculator.CalculateDiscountedPrice(Price,
        HasOrdersInPreviousMonth);
        public bool HasDiscount => DiscountedPrice < Price;
        public string QuantityLabel => PriceCalculator.GetQuantityLabel(TotalQuantity);
        public bool IsLowStock => PriceCalculator.IsLowStock(TotalQuantity);
        /// <summary>
        /// Путь к изображению товара; если файла нет — картинка-заглушка.
        /// </summary>
        public string ImagePath
        {
            get
            {
                string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
                if (!string.IsNullOrWhiteSpace(ImageFile))
                {
                    string path = Path.Combine(folder, "Images", ImageFile);

                    if (File.Exists(path))
                    {

                        return path;
                    }
                }
                return Path.Combine(folder, "picture.png");
            }
        }
    }
}