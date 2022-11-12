namespace NRO_Server.Model.SkillCharacter
{
    public class SpecialSkill
    {
        public int Id { get; set; }
        public string Info { get; set;}
        public int Img { get; set; }
        public int SkillId { get; set; }
        public int Value { get; set; }
        // Temp
        public int nextAttackDmgPercent { get; set; }
        public bool isCrit { get; set; }

        public SpecialSkill()
        {
            Id = -1;
            Info = "Chưa có Nội Tại\nBấm vào để xem chi tiết";
            SkillId = -1;
            Value = 0;
            Img = 5223;
            nextAttackDmgPercent = 0;
            isCrit = false;
        }

        public void ClearTemp()
        {
            nextAttackDmgPercent = 0;
            isCrit = false;
        }
    }
}