namespace ChudoObuv.Logic
{
    /// <summary>
    /// Алгоритмы расчета показателей товара.
    /// </summary>
    public static class PriceCalculator
    {
        public const decimal DiscountPercent = 25m;
        public const int ManyQuantityThreshold = 5;
        public const int LowStockThreshold = 3;
        /// <summary>
        /// Начало предыдущего календарного месяца относительно даты расчета.
        /// </summary>
        public static DateTime GetPreviousMonthStart(DateTime calculationDate)
        {
            return GetCurrentMonthStart(calculationDate).AddMonths(-1);
        }
        public static DateTime GetCurrentMonthStart(DateTime calculationDate)
        {
            return new DateTime(calculationDate.Year, calculationDate.Month, 1);
        }
        /// <summary>
        /// Цена со скидкой 25%, если в предыдущем календарном месяце заказов товара не было.
        /// </summary>
        public static decimal CalculateDiscountedPrice(decimal price, bool hasOrdersInPreviousMonth)
        {
            if (hasOrdersInPreviousMonth)
            {
                return price;
            }
            return Math.Round(price * (100m - DiscountPercent) / 100m, 2);
        }
        /// <summary>
        /// «много», если пар больше пяти, иначе «мало».
        /// </summary>
        public static string GetQuantityLabel(int totalQuantity)
        {
            return totalQuantity > ManyQuantityThreshold ? "много" : "мало";
        }
        public static bool IsLowStock(int totalQuantity)
        {
            return totalQuantity <= LowStockThreshold;
        }
    }
}