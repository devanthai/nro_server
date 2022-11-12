using System;
using NRO_Server.Model.ModelBase;

namespace NRO_Server.Model.Option
{
    public class OptionRadar : OptionBase
    {
        public int ActiveCard { get; set; }

        public OptionRadar()
        {
            Id = 0;
            Param = 0;
            ActiveCard = 0;
        }

        public override object Clone()
        {
            return new OptionRadar()
            {
                Id = Id,
                Param = Param,
                ActiveCard = ActiveCard
            };
        }
    }
}