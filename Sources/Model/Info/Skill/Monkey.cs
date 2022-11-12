namespace NRO_Server.Model.Info.Skill
{
    public class Monkey
    {
        public bool IsStart { get; set; }
        public short MonkeyId { get; set; }
        public int Hp { get; set; }
        public int Damage { get; set; }
        public long Delay { get; set; }
        public short HeadMonkey { get; set; }
        public short BodyMonkey { get; set; }
        public short LegMonkey { get; set; }
        public long TimeMonkey { get; set; }

        public Monkey()
        {
            IsStart = false;
            MonkeyId = 0;
            Damage = 0;
            Delay = -1;
            HeadMonkey = -1;
            BodyMonkey = -1;
            LegMonkey = -1;
            TimeMonkey = -1;
        }
    }
}