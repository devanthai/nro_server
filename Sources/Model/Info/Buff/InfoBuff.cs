namespace NRO_Server.Model.Info
{
    public class InfoBuff
    {
        public short ThucAnId { get; set; }
        public long ThucAnTime { get; set; }

        public bool CuongNo { get; set; }
        public long CuongNoTime { get; set; }

        public bool BoHuyet { get; set; }
        public long BoHuyetTime { get; set; }

        public bool BoKhi { get; set; }
        public long BoKhiTime { get; set; }

        public bool GiapXen { get; set; }
        public long GiapXenTime { get; set; }

        public bool AnDanh { get; set; }
        public long AnDanhTime { get; set; }

        public bool MayDoCSKB { get; set; }
        public long MayDoCSKBTime { get; set; }

        public bool CuCarot { get; set; }
        public long CuCarotTime { get; set; }

        public short BanhTrungThuId { get; set; }
        public long BanhTrungThuTime { get; set; }

        public InfoBuff()
        {
            ThucAnId = -1;
            ThucAnTime = 0;

            CuongNo = false;
            CuongNoTime = 0;

            BoHuyet = false;
            BoHuyetTime = 0;

            BoKhi = false;
            BoKhiTime = 0;

            GiapXen = false;
            GiapXenTime = 0;

            AnDanh = false;
            AnDanhTime = 0;

            CuCarot = false;
            CuCarotTime = 0;

            BanhTrungThuId = -1;
            BanhTrungThuTime = 0;
        }
    }
}