using LIM.EntityServices.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LIM.Models
{
    public class InventoryLogItem : IEntity
    {
        [MsListColumn("Artikelbezeichnung")]
        public string InventoryItemDescription { get; set; }

        [MsListColumn("ArtikelId")]
        public string InventoryItemId { get; set; }

        [MsListColumn("Timestamp")]
        public DateTime Timestamp { get; set; }

        [MsListColumn("Benutzer")]
        public string Username { get; set; }

        [MsListColumn("Lagerbestands_x00e4_nderung")]
        public decimal StockChange  { get; set; }

        [MsListColumn("ID")]
        public string Id { get; set; }
        public string WebUrl { get; set; }
    }
}
