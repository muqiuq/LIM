using LIM.EntityServices.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LIM.Models
{
    public class InventoryItem : IEntity
    {
        [MsListColumn("ID")]
        public string Id { get; set; }

        [MsListColumn("Typ")]
        public string Type { get; set; }

        [MsListColumn("Title")]
        public string Description { get; set; }

        [MsListColumn("Soll")]
        public decimal TargetInventory { get; set; }


        [MsListColumn("Einheit")]
        public string Unit { get; set; }

        [MsListColumn("Ist")]
        public decimal ActualInventory { get; set; }

        [MsListColumn("EANs")]
        public List<string> EANs { get; set; }

        [MsListColumn("Price")]
        public decimal Price { get; set; }

        [MsListColumn("Lieferant1")]
        public string SupplierLink1 { get; set; }

        [MsListColumn("Lieferant2")]
        public string SupplierLink2 { get; set; }
    }
}
