namespace ChudoObuv.Models
{
    public class User
    {
    public const string AdminRole = "Администратор";
    public const string ManagerRole = "Менеджер";
    public const string ClientRole = "Пользователь";

    public int Id { get; set; }
    public string LastName { get; set; }
    public string FirstName { get; set; }
    public string Patronymic { get; set; }
    public string Login { get; set; }
    public string Role { get; set; }

    public string FullName => $"{LastName} {FirstName} {Patronymic}".Trim(); 

        public bool IsAdmin => Role == AdminRole;

    public bool IsManagerOrAdmin => Role == ManagerRole || Role == AdminRole;
    } 
} 