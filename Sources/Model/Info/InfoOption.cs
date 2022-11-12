namespace NRO_Server.Model.Info
{
    public class InfoOption
    {
        public int PhanPercentSatThuong { get; set; }
        public int PhanTramXuyenGiapChuong { get; set; }
        public int PhanTramXuyenGiapCanChien { get; set; }
        public int PhanTramNeDon { get; set; }
        public int PhanTramVangTuQuai { get; set; }
        public int PhanTramTNSM { get; set; }

        public InfoOption()
        {
            PhanPercentSatThuong = 0;
            PhanTramXuyenGiapChuong = 0;
            PhanTramXuyenGiapCanChien = 0;
            PhanTramNeDon = 0;
            PhanTramVangTuQuai = 0;
            PhanTramTNSM = 0;
        }

        public void Reset()
        {
            PhanPercentSatThuong = 0;
            PhanTramXuyenGiapChuong = 0;
            PhanTramXuyenGiapCanChien = 0;
            PhanTramNeDon = 0;
            PhanTramVangTuQuai = 0;
            PhanTramTNSM = 0;
        }
    }
}