using System.Collections;
using System.Collections.Generic;
using NRO_Server.Model.Item;

namespace NRO_Server.Model.Template
{
    public class ShopTemplate
    {
        public int Id { get; set; }
        public byte Type { get; set; }
        public string Name { get; set; }
        public List<ItemShop> ItemShops { get; set; }

        public ShopTemplate()
        {
            ItemShops = new List<ItemShop>();
        }
    }
}