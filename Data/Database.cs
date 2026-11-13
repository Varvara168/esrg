using MySqlConnector;

namespace ChudoObuv.Data
{
    /// <summary> 
    /// Подключение к базе данных MySQL. 
    /// </summary> 
    public static class Database
    {
        // Перед запуском укажите свои сервер, имя базы, пользователя и пароль MySQL 
        private const string ConnectionString =
            "Server=85.236.189.123;Port=13306;Database=isp-42_sibyakovaa_vyu_wanderfull_shoes;User ID=isp-42_sibyakovaa_vyu;Password=isp-42_sibyakovaa_vyu;CharSet=utf8mb4;"; 


        public static MySqlConnection OpenConnection()
        {
            MySqlConnection connection = new MySqlConnection(ConnectionString);
            connection.Open();
            return connection;
        }
    }
}
