namespace NRO_Server.Model.BangXepHang
{
    public class BangXepHang
    {
        public int I { get; set; }
        public string Name { get; set; }
        public long Diem { get; set; }
        public BangXepHang()
        {
            I = 1;
            Diem = 0;
        }
    }
}