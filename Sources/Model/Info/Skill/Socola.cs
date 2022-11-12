namespace NRO_Server.Model.Info.Skill
{
    public class Socola
    {
        public bool IsSocola { get; set; }
        public bool IsCarot { get; set; }
        public long Time { get; set; }
        public int CharacterId { get; set; }
        public int Fight { get; set; }
        public int Percent { get; set; }

        public Socola()
        {
            IsSocola = false;
            IsCarot = false;
            Time = -1;
            CharacterId = -1;
            Fight = -1;
            Percent = 0;
        }
    }
}