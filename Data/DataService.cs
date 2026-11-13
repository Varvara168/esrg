using ChudoObuv.Logic;
using ChudoObuv.Models;
using MySqlConnector;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace ChudoObuv.Data
{
    /// <summary>
    /// Все запросы приложения к базе данных.
    /// </summary>
    public static class DataService
    {
        // ---------------------------- Пользователи ----------------------------
        public static User FindUserByLogin(string login)
        {
            const string sql = @"
                SELECT u.user_id, u.last_name, u.first_name, u.patronymic, u.login, r.role_name
                FROM users u
                JOIN roles r ON r.role_id = u.role_id
                WHERE u.login = @login";
            using MySqlConnection connection = Database.OpenConnection();
            using MySqlCommand command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@login", login);
            using MySqlDataReader reader = command.ExecuteReader();
            return reader.Read() ? ReadUser(reader) : null;
        }
        public static List<User> GetClients()
        {
            const string sql = @"
                SELECT u.user_id, u.last_name, u.first_name, u.patronymic, u.login, r.role_name
                FROM users u
                JOIN roles r ON r.role_id = u.role_id
                WHERE r.role_name = @role
                ORDER BY u.last_name, u.first_name";
            List<User> clients = new List<User>();
            using MySqlConnection connection = Database.OpenConnection();
            using MySqlCommand command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@role", User.ClientRole);
            using MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                clients.Add(ReadUser(reader));
            }
            return clients;
        }
        private static User ReadUser(MySqlDataReader reader)
        {
            return new User
            {
                Id = reader.GetInt32("user_id"),
                LastName = reader.GetString("last_name"),
                FirstName = reader.GetString("first_name"),
                Patronymic = reader.IsDBNull(reader.GetOrdinal("patronymic")) ? "" :
            reader.GetString("patronymic"),
                Login = reader.GetString("login"),
                Role = reader.GetString("role_name")
            };
        }
        // ------------------------------- Товары -------------------------------
        public static List<Product> GetProducts()
        {
            const string sql = @"
                SELECT p.product_id, p.product_name, c.category_name, s.subcategory_name,
m.manufacturer_name,
                    p.description, p.composition, p.price, p.image_file,
                    COALESCE((SELECT SUM(si.quantity) FROM stock_items si WHERE si.product_id =

p.product_id), 0) AS total_quantity,
                    EXISTS (SELECT 1
                        FROM order_items oi
                        JOIN orders o ON o.order_id = oi.order_id
                        JOIN stock_items si ON si.stock_item_id = oi.stock_item_id
                        WHERE si.product_id = p.product_id
                            AND o.order_date >= @previousMonthStart
                            AND o.order_date < @currentMonthStart) AS has_previous_month_orders

                FROM products p
                JOIN categories c ON c.category_id = p.category_id
                JOIN subcategories s ON s.subcategory_id = p.subcategory_id
                JOIN manufacturers m ON m.manufacturer_id = p.manufacturer_id
                    ORDER BY p.product_id";
            DateTime today = DateTime.Today;
            List<Product> products = new List<Product>();
            using MySqlConnection connection = Database.OpenConnection();
            using MySqlCommand command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@previousMonthStart",
            PriceCalculator.GetPreviousMonthStart(today));
            command.Parameters.AddWithValue("@currentMonthStart",
            PriceCalculator.GetCurrentMonthStart(today));
            using MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                products.Add(new Product
                {
                    Id = reader.GetInt32("product_id"),
                    Name = reader.GetString("product_name"),
                    Category = reader.GetString("category_name"),
                    Subcategory = reader.GetString("subcategory_name"),
                    Manufacturer = reader.GetString("manufacturer_name"),
                    Description = reader.IsDBNull(reader.GetOrdinal("description")) ? "" :

                reader.GetString("description"),
                    Composition = reader.IsDBNull(reader.GetOrdinal("composition")) ? "" :
                reader.GetString("composition"),
                    Price = reader.GetDecimal("price"),

                    ImageFile = reader.IsDBNull(reader.GetOrdinal("image_file")) ? null :

                reader.GetString("image_file"),
                    TotalQuantity = Convert.ToInt32(reader["total_quantity"]),

                    HasOrdersInPreviousMonth = Convert.ToInt32(reader["has_previous_month_orders"]) == 1

                });
            }
            return products;
        }
        public static List<string> GetCategories()
        {
            List<string> categories = new List<string>();
            using MySqlConnection connection = Database.OpenConnection();
            using MySqlCommand command = new MySqlCommand("SELECT category_name FROM categories ORDER BY category_name", connection);
        using MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                categories.Add(reader.GetString(0));
            }
            return categories;
        }
        /// <summary>
        /// Все товарные позиции модели (размерный ряд) с доступным количеством.
        /// </summary>
        public static List<StockItem> GetStockItems(int productId)
        {
            const string sql = @"
                SELECT si.stock_item_id, si.product_id, s.size_value, si.quantity
                FROM stock_items si
                JOIN sizes s ON s.size_id = si.size_id
                WHERE si.product_id = @productId
                ORDER BY s.size_value";
            List<StockItem> items = new List<StockItem>();
            using MySqlConnection connection = Database.OpenConnection();
            using MySqlCommand command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@productId", productId);
            using MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                items.Add(new StockItem
                {
                    Id = reader.GetInt32("stock_item_id"),
                    ProductId = reader.GetInt32("product_id"),
                    Size = reader.GetDecimal("size_value"),
                    Quantity = reader.GetInt32("quantity")

                });
            }
            return items;
        }
        // ------------------------------- Заказы -------------------------------
        public static List<Order> GetOrders()
        {
            const string sql = @"
                SELECT o.order_id, o.order_date, u.user_id,
                    CONCAT_WS(' ', u.last_name, u.first_name, u.patronymic) AS client_name,

                    COUNT(oi.order_item_id) AS items_count,
                    COALESCE(SUM(oi.quantity * oi.unit_price), 0) AS total_sum

                FROM orders o
                JOIN users u ON u.user_id = o.user_id
                LEFT JOIN order_items oi ON oi.order_id = o.order_id
                GROUP BY o.order_id, o.order_date, u.user_id, client_name
                ORDER BY o.order_date DESC, o.order_id DESC";
            List<Order> orders = new List<Order>();
            using MySqlConnection connection = Database.OpenConnection();
            using MySqlCommand command = new MySqlCommand(sql, connection);
            using MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                orders.Add(new Order
                {
                    Id = reader.GetInt32("order_id"),

                    OrderDate = reader.GetDateTime("order_date"),
                    ClientId = reader.GetInt32("user_id"),
                    ClientFullName = reader.GetString("client_name"),
                    ItemsCount = Convert.ToInt32(reader["items_count"]),
                    TotalSum = reader.GetDecimal("total_sum")

                });
            }
            return orders;
        }
        public static List<OrderItem> GetOrderItems(int orderId)
        {
            const string sql = @"
                SELECT oi.order_item_id, oi.stock_item_id, p.product_name, m.manufacturer_name,
                        s.size_value, oi.quantity, oi.unit_price
                FROM order_items oi
                JOIN stock_items si ON si.stock_item_id = oi.stock_item_id
                JOIN products p ON p.product_id = si.product_id
                JOIN manufacturers m ON m.manufacturer_id = p.manufacturer_id
                JOIN sizes s ON s.size_id = si.size_id
                WHERE oi.order_id = @orderId
                ORDER BY oi.order_item_id";
            List<OrderItem> items = new List<OrderItem>();
            using MySqlConnection connection = Database.OpenConnection();
            using MySqlCommand command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@orderId", orderId);
            using MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                items.Add(new OrderItem
                {
                    Id = reader.GetInt32("order_item_id"),
                    StockItemId = reader.GetInt32("stock_item_id"),
                    ProductName = reader.GetString("product_name"),
                    Manufacturer = reader.GetString("manufacturer_name"),
                    Size = reader.GetDecimal("size_value"),
                    Quantity = reader.GetInt32("quantity"),
                    UnitPrice = reader.GetDecimal("unit_price")

                });
            }
            return items;
        }
        /// <summary>
        /// Сохраняет заказ и уменьшает остатки. Возвращает номер заказа.
        /// Если какого-то размера уже не хватает — заказ не сохраняется (исключение).
        /// </summary>
        public static int CreateOrder(int clientId, List<OrderItem> items)
        {
            using MySqlConnection connection = Database.OpenConnection();
            using MySqlTransaction transaction = connection.BeginTransaction();
            
            MySqlCommand orderCommand = new MySqlCommand(
                "INSERT INTO orders (order_date, user_id) VALUES (@date, @userId)", connection,
transaction);
            orderCommand.Parameters.AddWithValue("@date", DateTime.Today);
            orderCommand.Parameters.AddWithValue("@userId", clientId);
            orderCommand.ExecuteNonQuery();
            int orderId = (int)orderCommand.LastInsertedId;
            
            foreach (OrderItem item in items)
            {
                // Остаток уменьшается только если его хватает — защита от одновременных заказов
                MySqlCommand stockCommand = new MySqlCommand(
                    "UPDATE stock_items SET quantity = quantity - @quantity WHERE stock_item_id = @id AND quantity >= @quantity",
                    connection, transaction);
                stockCommand.Parameters.AddWithValue("@quantity", item.Quantity);
                stockCommand.Parameters.AddWithValue("@id", item.StockItemId);
                if (stockCommand.ExecuteNonQuery() == 0)
                {
                    transaction.Rollback();

                    throw new InvalidOperationException(

                        $"Недостаточно товара «{item.ProductName}», размер {item.SizeText}. Обновите каталог и измените количество.");
                }
                
                MySqlCommand itemCommand = new MySqlCommand(
                    "INSERT INTO order_items (order_id, stock_item_id, quantity, unit_price) VALUES (@orderId, @stockItemId, @quantity, @price)",
                connection, transaction);
                itemCommand.Parameters.AddWithValue("@orderId", orderId);
                itemCommand.Parameters.AddWithValue("@stockItemId", item.StockItemId);
                itemCommand.Parameters.AddWithValue("@quantity", item.Quantity);
                itemCommand.Parameters.AddWithValue("@price", item.UnitPrice);
                itemCommand.ExecuteNonQuery();
            }
            transaction.Commit();
            return orderId;
        }
        /// <summary>
        /// Удаляет заказ и возвращает его товары на склад.
        /// </summary>
        public static void DeleteOrder(int orderId)
        {
            using MySqlConnection connection = Database.OpenConnection();
            using MySqlTransaction transaction = connection.BeginTransaction();
            
            MySqlCommand returnCommand = new MySqlCommand(@"
                UPDATE stock_items si
                JOIN order_items oi ON oi.stock_item_id = si.stock_item_id
                SET si.quantity = si.quantity + oi.quantity
                WHERE oi.order_id = @orderId", connection, transaction);
            returnCommand.Parameters.AddWithValue("@orderId", orderId);
            returnCommand.ExecuteNonQuery();
            
            // Строки состава удаляются каскадно (ON DELETE CASCADE)
            MySqlCommand deleteCommand = new MySqlCommand("DELETE FROM orders WHERE order_id = @orderId", connection, transaction);
            deleteCommand.Parameters.AddWithValue("@orderId", orderId);
            deleteCommand.ExecuteNonQuery();
            transaction.Commit();
        }
        /// <summary>
        /// Удаляет товарную позицию из заказа и возвращает ее количество на склад.
        /// </summary>
        
        public static void DeleteOrderItem(int orderItemId)
        {
            using MySqlConnection connection = Database.OpenConnection();
            using MySqlTransaction transaction = connection.BeginTransaction();
            
            MySqlCommand returnCommand = new MySqlCommand(@"
                UPDATE stock_items si
                JOIN order_items oi ON oi.stock_item_id = si.stock_item_id
                SET si.quantity = si.quantity + oi.quantity
                WHERE oi.order_item_id = @id", connection, transaction);
            returnCommand.Parameters.AddWithValue("@id", orderItemId);
            returnCommand.ExecuteNonQuery();
            
            MySqlCommand deleteCommand = new MySqlCommand("DELETE FROM order_items WHERE order_item_id = @id", connection, transaction);
            deleteCommand.Parameters.AddWithValue("@id", orderItemId);
            deleteCommand.ExecuteNonQuery();
            
            transaction.Commit();
        }

        public static void UpdateOrderDate(int orderId, DateTime orderDate)
        {
            using MySqlConnection connection = Database.OpenConnection();
            using MySqlCommand command = new MySqlCommand("UPDATE orders SET order_date = @date WHERE order_id = @orderId", connection);
            command.Parameters.AddWithValue("@date", orderDate.Date);
            command.Parameters.AddWithValue("@orderId", orderId);
            command.ExecuteNonQuery();
        }
    }
}