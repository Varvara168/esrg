using System.Windows;
using System.Windows.Controls;
using ChudoObuv.Data;
using ChudoObuv.Logic;
using ChudoObuv.Models;
namespace ChudoObuv.Pages
{
    public partial class CartPage : Page
    {
        public CartPage()
        {
            InitializeComponent();
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshCart();
        }
        private void RefreshCart()
        {
            ClientText.Text = $"Клиент: {Session.OrderClient.FullName} Дата заказа: { DateTime.Today:dd.MM.yyyy}";
        ItemsDataGrid.ItemsSource = null;
            ItemsDataGrid.ItemsSource = Session.Cart;
            TotalText.Text = $"Итого: {Session.Cart.Sum(item => item.Sum):N2} ₽";
        }
        private void RemoveItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).DataContext is OrderItem item &&
            Messages.Confirm($"Удалить из заказа «{item.ProductName}», размер {item.SizeText}?"))
            {
                Session.Cart.Remove(item);
                RefreshCart();
            }
        }
        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new CatalogPage());
        }
        private void CancelOrderButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Messages.Confirm("Отказаться от заказа? Все выбранные товары будут удалены из заказа. Действие нельзя отменить."))
            {
                return;
            }
            Session.CancelOrder();
            GoHome();
        }
        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (Session.Cart.Count == 0)
            {
            
            Messages.ShowWarning("В заказе нет товаров. Выберите товар в каталоге, укажите размер и количество и нажмите «Добавить в заказ».");
            return;
            }
            if (!Messages.Confirm($"Подтвердить заказ на сумму {Session.Cart.Sum(item => item.Sum):N2} ₽ для клиента { Session.OrderClient.FullName}?"))
        {
                return;
            }
            try
            {
                int orderId = DataService.CreateOrder(Session.OrderClient.Id, Session.Cart);
                Messages.ShowInfo($"Заказ No {orderId} оформлен {DateTime.Today:dd.MM.yyyy}.");
            }
            catch (Exception ex)
            {
                Messages.ShowDatabaseError(ex);
                return;
            }
            Session.CancelOrder();
            GoHome();
        }
        /// <summary>
        /// Менеджер и администратор возвращаются к списку заказов, клиент — в каталог.
        /// </summary>
        private void GoHome()
        {
            if (Session.CurrentUser.IsManagerOrAdmin)
            {
                NavigationService.Navigate(new OrdersPage());
            }
            else
            {
                NavigationService.Navigate(new CatalogPage());
            }
        ((MainWindow)Application.Current.MainWindow).ClearHistory();
        }
    }
}