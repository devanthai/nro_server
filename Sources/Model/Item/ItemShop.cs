using Newtonsoft.Json;
using NRO_Server.Model.ModelBase;

namespace NRO_Server.Model.Item
{
    public class ItemShop : ItemBase
    {
        public long Power { get; set; } = 0;
        public int BuySpec { get; set; } = 0;
        public bool IsNewItem { get; set; } = false;
        public short HeadTemp { get; set; } = -1;
        public short BagTemp { get; set; } = -1;
        public short BodyTemp { get; set; } = -1;
        public short LegTemp { get; set; } = -1;

        public ItemShop() : base()
        {
            
        }
    }
}