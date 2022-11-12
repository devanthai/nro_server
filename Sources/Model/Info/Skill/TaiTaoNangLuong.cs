namespace NRO_Server.Model.Info.Skill
{
    public class TaiTaoNangLuong
    {
        public bool IsTTNL { get; set; }
        public long DelayTTNL { get; set; }
        public int Crit { get; set; }

        public TaiTaoNangLuong()
        {
            IsTTNL = false;
            DelayTTNL = -1;
            Crit = 0;
        }
    }
}