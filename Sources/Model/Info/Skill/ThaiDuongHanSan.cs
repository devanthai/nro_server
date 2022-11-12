namespace NRO_Server.Model.Info.Skill
{
    public class ThaiDuongHanSan
    {
        public bool IsStun { get; set; }
        public long Time { get; set; }
        public int TimeReal { get; set; }

        public ThaiDuongHanSan()
        {
            IsStun = false;
            Time = -1;
            TimeReal = 0;
        }
    }
}