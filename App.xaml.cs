using ChudoObuv.Logic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Threading;

namespace ChudoObuv
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Русский формат чисел, цен и дат во всем приложении (в том числе в привязках XAML) 
            CultureInfo russian = new CultureInfo("ru-RU");
            CultureInfo.DefaultThreadCurrentCulture = russian;
            CultureInfo.DefaultThreadCurrentUICulture = russian;
            Thread.CurrentThread.CurrentCulture = russian;
            Thread.CurrentThread.CurrentUICulture = russian;
            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement),
                new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(russian.IetfLanguageTag)));

            base.OnStartup(e);
        }

        // Последняя защита от аварийного завершения: любая необработанная ошибка показывается пользователю
        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Messages.ShowError($"Произошла непредвиденная ошибка: {e.Exception.Message}\nПовторите действие. Если ошибка повторяется, перезапустите приложение.");
            e.Handled = true;
        }
    }
}
