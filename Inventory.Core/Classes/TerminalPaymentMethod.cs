namespace Inventory.Core.Classes
{
    public class TerminalPaymentMethod
    {
        public int Id { get; set; }
        public int TerminalId { get; set; }
        public Terminal? Terminal { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}