using System.Collections.Generic;

namespace NRO_Server.Model.Template
{
    public class SkillTemplate
    {
        public int Id { get; set; }
        public byte NClass { get; set; }
        public string Name { get; set; }
        public byte MaxPoint { get; set; }
        public byte ManaUseType { get; set; }
        public byte Type { get; set; }
        public short IconId { get; set; }
        public string DamageInfo { get; set; }
        public string Description { get; set; }
        public List<SkillDataTemplate> SkillDataTemplates { get; set; }

        public SkillTemplate()
        {
            SkillDataTemplates = new List<SkillDataTemplate>();
        }
    }
}