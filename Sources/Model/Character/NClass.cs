using System.Collections.Generic;
using NRO_Server.Model.Template;

namespace NRO_Server.Model
{
    public class NClass
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<SkillTemplate> SkillTemplates { get; set; }

        public NClass()
        {
            
        }
    }
}