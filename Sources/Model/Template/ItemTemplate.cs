using System.Collections.Generic;
using NRO_Server.Model.Option;

namespace NRO_Server.Model.Template
{
    public class ItemTemplate
    {
        public short Id { get; set; }
        public byte Type { get; set; }
        public byte Skill { get; set; }
        public byte Gender { get; set; }
        public string Name { get; set; }
        public string SubName { get; set; }
        public string Description { get; set; }
        public byte Level { get; set; }
        public short IconId { get; set; }
        public short Part { get; set; }
        public long Require { get; set; }
        public bool IsUpToUp { get; set; }
        public bool IsDrop { get; set; }
        public bool IsExpires{ get; set; }
        public long SecondsExpires { get; set; }
        public int SaleCoin { get; set; }

        public readonly List<OptionItem> Options;

        public ItemTemplate()
        {
            Options = new List<OptionItem>();
        }
        
        public bool IsTypeBody()
        {
            return Type >= 0 && Type < 6 || Type == 32;
        }

        public bool IsTypeNRKham()
        {
            return Id >= 0 && Id <= 20;
        }

        public bool IsTypeSPL()
        {
            return Type == 30;
        }
    }
}