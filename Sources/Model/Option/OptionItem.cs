using System;
using NRO_Server.Model.ModelBase;

namespace NRO_Server.Model.Option
{
    public class OptionItem : OptionBase
    {
        public OptionItem()
        {
            
        }

        public override object Clone()
        {
            return new OptionItem()
            {
                Id = Id,
                Param = Param
            };
        }
     
    }
}