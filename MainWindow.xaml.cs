using ChudoObuv.Pages;
using System.Net.NetworkInformation;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ChudoObuv.Logic; 
using ChudoObuv.Pages;

namespace ChudoObuv
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(new LoginPage());
        }

        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            // Заголовок окна и шапки берется из заголовка открытой страницы 
            if (e.Content is Page page)
            {
                Title = $"Чудо Обувь — {page.Title}";
                PageTitleText.Text = page.Title;
            }

            BackButton.Visibility = MainFrame.CanGoBack ? Visibility.Visible : Visibility.Collapsed;
            UpdateUserInfo();
        }

        private void UpdateUserInfo()
        {
            bool isLoginPage = MainFrame.Content is LoginPage;
            if (Session.IsAuthorized)
            {
                UserNameText.Text = Session.CurrentUser.FullName;
                UserRoleText.Text = Session.CurrentUser.Role;
               
                LogoutButton.Content = "Выйти";
            }
            else
            {
                UserNameText.Text = isLoginPage ? "" : "Гость";
                UserRoleText.Text = "";
                LogoutButton.Content = "Войти";
            }
            LogoutButton.Visibility = isLoginPage ? Visibility.Collapsed : Visibility.Visible;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.CanGoBack)
            {
                MainFrame.GoBack();
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            if (Session.Cart.Count > 0 && !Messages.Confirm("Неподтвержденный заказ будет отменен. Выйти из системы?"))
            {
                return;
            }
            Session.Logout();
            MainFrame.Navigate(new LoginPage());
            ClearHistory();
        }

        /// <summary> 
        /// Очищает историю переходов, чтобы кнопкой «Назад» нельзя было вернуться в чужой сеанс. 
        /// </summary> 
        public void ClearHistory()
        {
            // Навигация выполняется асинхронно, поэтому очищаем журнал после завершения перехода 
            Dispatcher.BeginInvoke(new Action(() =>
            {
                while (MainFrame.CanGoBack)
                {
                    MainFrame.RemoveBackEntry();
                }
                BackButton.Visibility = Visibility.Collapsed;
                UpdateUserInfo();
            }), System.Windows.Threading.DispatcherPriority.ContextIdle);
        }
    }
}