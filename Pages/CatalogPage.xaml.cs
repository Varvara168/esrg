using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ChudoObuv.Data;
using ChudoObuv.Logic;
using ChudoObuv.Models;

namespace ChudoObuv.Pages
{
    public partial class CatalogPage : Page
    {
        private const string AllCategories = "Все категории";

        private List<Product> allProducts = new List<Product>();
        private bool isInitialized;

        public CatalogPage()
        {
            InitializeComponent();
        }

        // Вызывается при каждом открытии страницы, в том числе после возврата «Назад», 
        // поэтому каталог и остатки всегда актуальны 
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            bool isAuthorized = Session.IsAuthorized;
            FilterPanel.Visibility = isAuthorized ? Visibility.Visible : Visibility.Collapsed;
            CartButton.Visibility = isAuthorized ? Visibility.Visible : Visibility.Collapsed;
            OrdersButton.Visibility = isAuthorized && Session.CurrentUser.IsManagerOrAdmin ? Visibility.Visible : Visibility.Collapsed;

            CartButton.Content = $"Оформление заказа ({Session.Cart.Sum(item => item.Quantity)})";
            OrderClientText.Text = isAuthorized ? $"Заказ оформляется для клиента: {Session.OrderClient.FullName}" : "Для оформления заказа войдите в систему.";

            try
            {
                if (!isInitialized)
                {
                    List<string> categories = DataService.GetCategories();
                    categories.Insert(0, AllCategories);
                    CategoryComboBox.ItemsSource = categories;
                    CategoryComboBox.SelectedIndex = 0;
                    isInitialized = true;
                }
                allProducts = DataService.GetProducts();
            }
            catch (Exception ex)
            {
                Messages.ShowDatabaseError(ex);
                allProducts = new List<Product>();
            }
            ApplyFilters();
        }

        private void Filter_Changed(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        /// <summary> 
        /// Поиск, фильтр и сортировка применяются вместе при любом изменении. 
        /// </summary> 
        private void ApplyFilters()
        {
            if (ProductsListView == null || SortComboBox == null)
            {
                return;
            }

            IEnumerable<Product> result = allProducts;

            if (Session.IsAuthorized)
            {
                string search = SearchTextBox.Text.Trim();
                if (search.Length > 0)
                {
                    result = result.Where(product =>
                        product.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        product.Description.Contains(search, StringComparison.OrdinalIgnoreCase));
                }

                string category = CategoryComboBox.SelectedItem as string;
                if (category != null && category != AllCategories)
                {
                    result = result.Where(product => product.Category == category);
                }

                if (SortComboBox.SelectedIndex == 1)
                {
                    result = result.OrderBy(product => product.DiscountedPrice);
                }
                else if (SortComboBox.SelectedIndex == 2)
                {
                    result = result.OrderByDescending(product => product.DiscountedPrice);
                }
            }

            List<Product> shown = result.ToList();
            ProductsListView.ItemsSource = shown;
            CountText.Text = $"Показано товаров: {shown.Count} из {allProducts.Count}";
        }

        private void ProductsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProductsListView.SelectedItem is not Product product)
            {
                return;
            }
            ProductsListView.SelectedItem = null;

            if (!Session.IsAuthorized)
            {
                Messages.ShowInfo("Оформлять заказы могут только авторизованные пользователи.\nВыйдите на страницу входа и войдите под своим логином.");
                return;
            }
            NavigationService.Navigate(new ProductPage(product));
        }

        private void CartButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new CartPage());
        }

        private void OrdersButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new OrdersPage());
        }
    }
}
