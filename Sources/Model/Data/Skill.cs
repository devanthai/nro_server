namespace NRO_Server.Model.Data
{
    public class Skill
    {
        public short Id { get; set; }
        public short EffectHappenOnMob { get; set; }
        public byte NumEff { get; set; }
        public short[][] SkillStand { get; set; }
        public short[][] SkillFly { get; set; }
    }
}