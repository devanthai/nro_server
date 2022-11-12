using System.Collections.Generic;
using NRO_Server.Model.SkillCharacter;
namespace NRO_Server.Model.Template
{
    public class BossTemplate
    {
        public int Id { get; set; }
        public int Type { get; set; }
        public string Name { get; set; }
        public int Hp { get; set; }
        public int Mp { get; set; }
        public short Stamina { get; set; }
        public List<SkillCharacter.SkillCharacter> Skills { get; set; }
        public int Damage { get; set; }
        public int Defence { get; set; }
        public int CritChance { get; set; }
        public short Hair { get; set; }
        public bool KhangTroi { get; set; }

        public BossTemplate()
        {
            
        }
    }
}