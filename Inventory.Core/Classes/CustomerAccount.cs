using System.Collections.Generic;

namespace Inventory.Core.Classes
{
    public class CustomerAccount
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Document { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public List<CustomerAccountUser> Users { get; set; } = new();
        public List<AccountLocation> Locations { get; set; } = new();

        /// <summary>
        /// Indica si la cuenta tiene logo, sin cargar la imagen. La UI usa esto para
        /// decidir si pide el binario a api/customeraccounts/{id}/logo.
        /// No se persiste: el repositorio lo rellena al listar.
        /// </summary>
        public bool HasLogo { get; set; }
    }
}
