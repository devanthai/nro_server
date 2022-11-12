using System.Collections.Generic;
using System.Linq;
using NRO_Server.Model.Option;
using NRO_Server.Model.Item;

namespace NRO_Server.Model.ModelBase
{
    public class ItemBase
    {
        public short Id { get; set; }
        public int BuyCoin { get; set; } = 0;
        public int BuyGold { get; set; } = 0;
        public int Quantity { get; set; } = 1;
        public string Reason { get; set; }
        public List<OptionItem> Options { get; set; }

        public ItemBase()
        {
            Reason = "";
            Options = new List<OptionItem>();
        }

        public int GetParamOption(int id)
        {
            var option = Options.FirstOrDefault(op => op.Id == id);
            return option != null ? option.Param : 0;
        }
    }
}