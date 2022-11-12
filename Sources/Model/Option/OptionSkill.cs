using System;
using NRO_Server.Model.ModelBase;

namespace NRO_Server.Model.Option
{
    public class OptionSkill : OptionBase
    {
        public override object Clone()
        {
            return new OptionSkill()
            {
                Id = Id,
                Param = Param
            };
        }
    }
}