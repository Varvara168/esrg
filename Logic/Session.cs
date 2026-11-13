using ChudoObuv.Models;

namespace ChudoObuv.Logic
{
    /// <summary> 
    /// Текущий пользователь и формируемый (еще не подтвержденный) заказ. 
    /// </summary> 
    public static class Session
    {
        public static User CurrentUser { get; private set; }

        /// <summary> 
        /// Клиент, для которого формируется заказ: сам пользователь или клиент, выбранный менеджером. 
        /// </summary> 
        public static User OrderClient { get; private set; }

        public static List<OrderItem> Cart { get; } = new List<OrderItem>();

        public static bool IsAuthorized => CurrentUser != null;

        public static void Login(User user)
        {
            CurrentUser = user;
            OrderClient = user;
            Cart.Clear();
        }

        public static void Logout()
        {
            CurrentUser = null;
            OrderClient = null;
            Cart.Clear();
        }

        public static void StartOrderFor(User client)
        {
            OrderClient = client;
            Cart.Clear();
        }

        public static void CancelOrder()
        {
            Cart.Clear();
            OrderClient = CurrentUser;
        }

        /// <summary> 
        /// Сколько пар данной товарной позиции уже лежит в формируемом заказе. 
        /// </summary> 
        public static int GetQuantityInCart(int stockItemId)
        {
            return Cart.Where(item => item.StockItemId == stockItemId).Sum(item => item.Quantity);
        }

        public static void AddToCart(OrderItem newItem)
        {
            OrderItem existing = Cart.FirstOrDefault(item => item.StockItemId == newItem.StockItemId);
            if (existing != null)
            {
                existing.Quantity += newItem.Quantity;
            }
            else
            {
                Cart.Add(newItem);
            }
        } 
    }
}