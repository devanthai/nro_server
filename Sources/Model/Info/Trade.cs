using System.Collections.Generic;

namespace NRO_Server.Model.Info
{
    public class Trade
    {
        public bool IsTrade { get; set; }
        public int CharacterId { get; set; }
        public bool IsHold { get; set; }
        public bool IsLock { get; set; }
        public List<Item.Item> Items { get; set; }

        public Trade()
        {
            IsTrade = false;
            IsHold = false;
            IsLock = false;
            CharacterId = -1;
            Items = new List<Item.Item>();
        }
    }
}