using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ChudoObuv.Data;
using ChudoObuv.Logic;
using ChudoObuv.Models;

namespace ChudoObuv.Pages
{
    public partial class OrderPage : Page
    {
        private readonly Order order;
        private List<OrderItem> items = new List<OrderItem>();

        public OrderPage(Order order)
        {
            InitializeComponent();
            this.order = order;
            Title = $"Состав заказа № {order.Id}";
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            bool isAdmin = Session.CurrentUser.IsAdmin;
            OrderDateText.Visibility = isAdmin ? Visibility.Collapsed : Visibility.Visible;
            OrderDatePicker.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            SaveDateButton.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            DeleteColumn.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;

            HeaderText.Text = $"Заказ № {order.Id}. Клиент: {order.ClientFullName}";
            OrderDateText.Text = order.OrderDate.ToString("dd.MM.yyyy");
            OrderDatePicker.SelectedDate = order.OrderDate;
            LoadItems();
        }

        private void LoadItems()
        {
            try
            {
                items = DataService.GetOrderItems(order.Id);
            }
            catch (Exception ex)
            {
                Messages.ShowDatabaseError(ex);
                items = new List<OrderItem>();
            }
            ItemsDataGrid.ItemsSource = items;
            TotalText.Text = $"Итоговая сумма по заказу: {items.Sum(item => item.Sum):N2} ₽";
        }

        private void SaveDateButton_Click(object sender, RoutedEventArgs e)
        {
            // Дату, введенную с клавиатуры, разбираем сами: DatePicker применяет ее только при потере фокуса
            if (!DateTime.TryParseExact(OrderDatePicker.Text.Trim(), "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime newDate))
            {
                Messages.ShowWarning("Дата заказа указана неверно. Выберите ее в календаре или введите в формате ДД.ММ.ГГГГ, например 15.04.2026.");
                OrderDatePicker.Focus();
                return;
            }
            if (newDate.Date > DateTime.Today)
            {
                Messages.ShowWarning("Дата заказа не может быть позже сегодняшней. Выберите другую дату.");
                return;
            }

            try
            {
                DataService.UpdateOrderDate(order.Id, newDate);
                order.OrderDate = newDate;
                Messages.ShowInfo($"Дата заказа № {order.Id} изменена на {newDate:dd.MM.yyyy}.");
            }
            catch (Exception ex)
            {
                Messages.ShowDatabaseError(ex);
            }
        }

        private void DeleteItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).DataContext is not OrderItem item)
            {
                return;
            }
            if (items.Count == 1)
            {
                Messages.ShowWarning("Это последняя позиция заказа. Пустой заказ хранить нельзя — удалите весь заказ на странице «Список заказов».");
                return;
            }
            if (!Messages.Confirm($"Удалить позицию «{item.Position}» из заказа № {order.Id}?\nТовар вернется на склад. Действие нельзя отменить."))
            {
                return;
            }

            try
            {
                DataService.DeleteOrderItem(item.Id);
                LoadItems();
            }
            catch (Exception ex)
            {
                Messages.ShowDatabaseError(ex);
            }
        }
    }
}
