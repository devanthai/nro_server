using System.Collections.Generic;

namespace NRO_Server.Model.Template
{
    public class SpecialSkillTemplate
    {
        public int Id { get; set; }
        public string Info { get; set; }
        public string InfoFormat { get; set;}
        public short Gender { get; set; }
        public short Img { get; set; }
        public short SkillId { get; set; }
        public short Min { get; set; }
        public short Max { get; set; }
        public int Vip { get; set; }

        public SpecialSkillTemplate()
        {

        }
    }
}