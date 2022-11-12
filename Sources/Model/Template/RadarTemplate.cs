using System.Collections.Generic;
using NRO_Server.Model.Option;

namespace NRO_Server.Model.Template
{
    public class RadarTemplate
    {
        public short Id { get; set; }
        public short IconId { get; set; }
        public int Rank { get; set; }
        public int Max { get; set; }
        public int Type { get; set; }
        public int Template { get; set; }
        public List<short> Data { get; set; }
        public string Name { get; set; }
        public string Info { get; set; }
        public List<OptionRadar> Options { get; set; }
        public short Require { get; set; }
        public short RequireLevel { get; set; }
        public short AuraId { get; set; }

        public RadarTemplate()
        {
            Id = -1;
            IconId = -1;
            Rank = 0;
            Max = 0;
            Type = 0;
            Template = 1;
            Data = new List<short>();
            Name = "";
            Info = "";
            Options = new List<OptionRadar>();
            Require = -1;
            RequireLevel = 0;
            AuraId = -1;
        }
    }
}