namespace Inventory.Core.Classes
{
    public class CustomerAccountUser
    {
        public int Id { get; set; }
        public int CustomerAccountId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public CustomerAccount? Account { get; set; }
    }
}
