using ChudoObuv.Data;
using ChudoObuv.Logic;
using ChudoObuv.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace ChudoObuv.Pages
{
    public partial class ProductPage : Page
    {
        private readonly Product product;
        private List<StockItem> stockItems = new List<StockItem>();

        public ProductPage(Product product)
        {
            InitializeComponent();
            this.product = product;
            Title = $"Просмотр товара: {product.Name}";
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ProductImage.Source = new BitmapImage(new Uri(product.ImagePath));
            NameText.Text = product.Name;
            ManufacturerRun.Text = product.Manufacturer;
            CategoryRun.Text = $"{product.Category} / {product.Subcategory}";
            DiscountedPriceText.Text = $"{product.DiscountedPrice:N2} ₽";
            OldPriceText.Text = product.HasDiscount ? $"{product.Price:N2} ₽" : "";
            CompositionRun.Text = product.Composition;
            DescriptionRun.Text = product.Description;
            LoadSizes();
        }

        /// <summary> 
        /// Загружает актуальные остатки из БД и учитывает то, что уже добавлено в заказ. 
        /// </summary> 
        private void LoadSizes()
        {
            try
            {
                stockItems = DataService.GetStockItems(product.Id);
            }
            catch (Exception ex)
            {
                Messages.ShowDatabaseError(ex);
                stockItems = new List<StockItem>();
            }

            SizeRangeRun.Text = stockItems.Count > 0 ? string.Join(", ", stockItems.Select(item => item.SizeText)) : "нет данных";

            List<StockItem> available = stockItems
                .Where(item => item.Quantity - Session.GetQuantityInCart(item.Id) > 0)
                .ToList();
            AvailableSizesRun.Text = available.Count > 0 ? string.Join(", ", available.Select(item => item.SizeText)) : "нет в наличии";

            SizeComboBox.ItemsSource = available;
            SizeComboBox.SelectedIndex = available.Count > 0 ? 0 : -1;
            UpdateAvailableText();
        }

        private int GetAvailableQuantity(StockItem stockItem)
        {
            return stockItem.Quantity - Session.GetQuantityInCart(stockItem.Id);
        }

        private void SizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateAvailableText();
        }

        private void UpdateAvailableText()
        {
            AvailableText.Text = SizeComboBox.SelectedItem is StockItem stockItem
                ? $"доступно: {GetAvailableQuantity(stockItem)} пар"
                : "";
        }

        // В поле количества можно вводить только цифры 
        private void QuantityTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (SizeComboBox.SelectedItem is not StockItem stockItem)
            {
                Messages.ShowWarning("Выберите размер обуви. Если список размеров пуст — товара нет в наличии.");
                return;
            }

            int available = GetAvailableQuantity(stockItem);
            if (!int.TryParse(QuantityTextBox.Text, out int quantity) || quantity <= 0)
            {
                Messages.ShowWarning("Количество указано неверно. Введите целое число пар больше нуля.");
                QuantityTextBox.Focus();
                return;
            }
            if (quantity > available)
            {
                Messages.ShowWarning($"Для размера {stockItem.SizeText} доступно только {available} пар. Уменьшите количество.");
                QuantityTextBox.Focus();
                return;
            }

            Session.AddToCart(new OrderItem
            {
                StockItemId = stockItem.Id,
                ProductName = product.Name,
                Manufacturer = product.Manufacturer,
                Size = stockItem.Size,
                Quantity = quantity,
                UnitPrice = product.DiscountedPrice
            });

            Messages.ShowInfo($"Товар «{product.Name}», размер {stockItem.SizeText}, {quantity} пар добавлен в заказ.\n" +
                              "Добавьте другие товары или перейдите к оформлению заказа.");
            QuantityTextBox.Text = "1";
            LoadSizes();
        }

        private void CartButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new CartPage());
        }
    }
}
