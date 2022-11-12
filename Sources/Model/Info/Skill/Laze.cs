namespace NRO_Server.Model.Info.Skill
{
    public class Laze
    {
        public bool Hold { get; set; }
        public long Time { get; set; }

        public Laze()
        {
            Hold = false;
            Time = -1;
        }
    }
}