using ChudoObuv.Data;
using ChudoObuv.Logic;
using ChudoObuv.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ChudoObuv.Pages
{
    public partial class OrdersPage : Page
    {
        public OrdersPage()
        {
            InitializeComponent();
        }

        // Список перечитывается при каждом открытии страницы (после добавления, изменения, удаления) 
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ClientComboBox.ItemsSource == null)
                {
                    ClientComboBox.ItemsSource = DataService.GetClients();
                }
                OrdersDataGrid.ItemsSource = DataService.GetOrders();
            }
            catch (Exception ex)
            {
                Messages.ShowDatabaseError(ex);
            }
        }

        private Order GetSelectedOrder()
        {
            if (OrdersDataGrid.SelectedItem is Order order)
            {
                return order;
            }
            Messages.ShowWarning("Сначала выберите заказ в списке (щелкните по строке).");
            return null;
        }

        private void AddOrderButton_Click(object sender, RoutedEventArgs e)
        {
            if (ClientComboBox.SelectedItem is not User client)
            {
                Messages.ShowWarning("Выберите клиента, для которого оформляется заказ.");
                ClientComboBox.Focus();
                return;
            }
            if (Session.Cart.Count > 0 && !Messages.Confirm("Ранее начатый неподтвержденный заказ будет отменен. Продолжить?"))
            {
                return;
            }
            Session.StartOrderFor(client);
            NavigationService.Navigate(new CatalogPage());
        }

        private void CatalogButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new CatalogPage());
        }

        private void OpenOrderButton_Click(object sender, RoutedEventArgs e)
        {
            Order order = GetSelectedOrder();
            if (order != null)
            {
                NavigationService.Navigate(new OrderPage(order));
            }
        }

        private void OrdersDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (OrdersDataGrid.SelectedItem is Order order)
            {
                NavigationService.Navigate(new OrderPage(order));
            }
        }

        private void DeleteOrderButton_Click(object sender, RoutedEventArgs e)
        {
            Order order = GetSelectedOrder();
            if (order == null)
            {
                return;
            }

            if (!Messages.Confirm($"Удалить заказ № {order.Id} от {order.OrderDate:dd.MM.yyyy} ({order.ClientFullName})?\n" +
                                  "Товары вернутся на склад. Действие нельзя отменить."))
            {
                return;
            }

            try
            {
                DataService.DeleteOrder(order.Id);
                OrdersDataGrid.ItemsSource = DataService.GetOrders();
                Messages.ShowInfo($"Заказ № {order.Id} удален.");
            }
            catch (Exception ex)
            {
                Messages.ShowDatabaseError(ex);
            }
        }
    }
}
