namespace NRO_Server.Model.Info
{
    public class LearnSkill
    {
        public Item.Item ItemSkill { get; set; }
        public long Time { get; set; }
        public short ItemTemplateSkillId { get; set; }
        public int Potential { get; set; }

        public LearnSkill()
        {
            ItemSkill = null;
            Time = -1;
            ItemTemplateSkillId = -1;
            Potential = 0;
        }
    }
}