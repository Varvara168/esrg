using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ChudoObuv.Data;
using ChudoObuv.Logic;
using ChudoObuv.Models;

namespace ChudoObuv.Pages
{
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
            Loaded += (sender, e) => LoginTextBox.Focus();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            TryLogin();
        }

        private void LoginTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TryLogin();
            }
        }

        private void TryLogin()
        {
            string login = LoginTextBox.Text.Trim();
            if (login.Length == 0)
            {
                Messages.ShowWarning("Введите логин. Логин выдается администратором и указан в списке пользователей системы.");
                LoginTextBox.Focus();
                return;
            }

            User user;
            try
            {
                user = DataService.FindUserByLogin(login);
            }
            catch (Exception ex)
            {
                Messages.ShowDatabaseError(ex);
                return;
            }

            if (user == null)
            {
                Messages.ShowError($"Пользователь с логином «{login}» не найден.\nПроверьте раскладку клавиатуры и правильность ввода логина или войдите как гость.");
                LoginTextBox.SelectAll();
                LoginTextBox.Focus();
                return;
            }

            Session.Login(user);
            OpenCatalog();
        }

        private void GuestButton_Click(object sender, RoutedEventArgs e)
        {
            Session.Logout();
            OpenCatalog();
        }

        private void OpenCatalog()
        {
            NavigationService.Navigate(new CatalogPage());
            ((MainWindow)Application.Current.MainWindow).ClearHistory();
        }
    }
}
