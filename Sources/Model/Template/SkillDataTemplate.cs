using System;

namespace NRO_Server.Model.Template
{
    public class SkillDataTemplate
    {
        public int SkillId { get; set;}
        public byte Point { get; set; }
        public long PowRequire { get; set; }
        public int ManaUse { get; set; }
        public int CoolDown { get; set; }
        public short Dx { get; set; }
        public short Dy { get; set; }
        public byte MaxFight { get; set; }
        public int Damage { get; set; }
        public int Price { get; set; }
        public string MoreInfo { get; set; }

        public SkillDataTemplate()
        {
            
        }

        public SkillDataTemplate Clone() { return (SkillDataTemplate)this.MemberwiseClone(); }
    }
}