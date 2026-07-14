using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inventory.Core.Classes
{
    public class Attribute
    {
        public string Name { get; set; }  // Ej: "Viscosidad", "Marca", "Calibre"
        public string Value { get; set; }   // Ej: "5W-30", "Toyota", "3/8"


        public Attribute(string Name, string Value)
        {
            this.Name = Name;
            this.Value = Value;
        }
    }
}
