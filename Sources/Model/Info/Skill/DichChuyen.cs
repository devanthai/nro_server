namespace NRO_Server.Model.Info.Skill
{
    public class DichChuyen
    {
        public bool IsStun { get; set; }
        public long Time { get; set; }

        public DichChuyen()
        {
            IsStun = false;
            Time = -1;
        }
    }
}