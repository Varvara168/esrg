using MySqlConnector;
using System;
using System.Windows;

namespace ChudoObuv.Logic
{
    /// <summary> 
    /// Окна сообщений с заголовком и пиктограммой нужного типа. 
    /// </summary> 
    public static class Messages
    {
        private const string AppName = "Чудо Обувь";

        public static void ShowError(string text)
        {
            MessageBox.Show(text, $"{AppName} — Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static void ShowWarning(string text)
        {
            MessageBox.Show(text, $"{AppName} — Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        public static void ShowInfo(string text)
        {
            MessageBox.Show(text, $"{AppName} — Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary> 
        /// Подтверждение необратимой операции. Возвращает true, если пользователь нажал «Да». 
        /// </summary> 
        public static bool Confirm(string text)
        {
            MessageBoxResult result = MessageBox.Show(text, $"{AppName} — Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
            return result == MessageBoxResult.Yes;
        }

        public static void ShowDatabaseError(Exception exception)
        {
            if (exception is MySqlException)
            {
                ShowError("Не удалось выполнить операцию с базой данных.\n" +
                          $"Причина: {exception.Message}\n\n" +
                          "Проверьте, что сервер MySQL запущен, а в файле Data\\Database.cs указаны верные сервер, пользователь и пароль.");
            }
            else
            {
                ShowError(exception.Message);
            }
        }
    }
}
