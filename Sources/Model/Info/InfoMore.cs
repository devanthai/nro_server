namespace NRO_Server.Model.Info
{
    public class InfoMore
    {
        public Revenge Revenge { get; set; }

        public int LastGiapLuyenTapItemId { get; set; }

        public int TransportMapId { get; set; }

        // Bùa
        public bool BuaTriTue { get; set; }
        public long BuaTriTueTime { get; set; }

        public bool BuaManhMe { get; set; }
        public long BuaManhMeTime { get; set; }

        public bool BuaDaTrau { get; set; }
        public long BuaDaTrauTime { get; set; }

        public bool BuaOaiHung { get; set; }
        public long BuaOaiHungTime { get; set; }

        public bool BuaBatTu { get; set; }
        public long BuaBatTuTime { get; set; }

        public bool BuaDeoDai { get; set; }
        public long BuaDeoDaiTime { get; set; }

        public bool BuaThuHut { get; set; }
        public long BuaThuHutTime { get; set; }

        public bool BuaDeTu { get; set; }
        public long BuaDeTuTime { get; set; }

        public bool BuaTriTueX3 { get; set; }
        public long BuaTriTueX3Time { get; set; }

        public bool BuaTriTueX4 { get; set; }
        public long BuaTriTueX4Time { get; set; }

        // Vệ tinh
        public bool IsNearAuraTriLucItem { get; set; }

        public bool IsNearAuraTriTueItem { get; set; }
        public long AuraTriTueTime { get; set; }

        public bool IsNearAuraPhongThuItem { get; set; }
        public long AuraPhongThuTime { get; set; }

        public bool IsNearAuraSinhLucItem { get; set; }

        public int PetItemIndex { get; set; }

        public bool VuaGoiRong { get; set; }

        public InfoMore()
        {
            Revenge = new Revenge();

            LastGiapLuyenTapItemId = 0;

            TransportMapId = -1;

            // Bùa
            BuaTriTue = false;
            BuaTriTueTime = 0;

            BuaManhMe = false;
            BuaManhMeTime = 0;

            BuaDaTrau = false;
            BuaDaTrauTime = 0;

            BuaOaiHung = false;
            BuaOaiHungTime = 0;

            BuaBatTu = false;
            BuaBatTuTime = 0;

            BuaDeoDai = false;
            BuaDeoDaiTime = 0;

            BuaThuHut = false;
            BuaThuHutTime = 0;

            BuaDeTu = false;
            BuaDeTuTime = 0;

            BuaTriTueX3 = false;
            BuaTriTueX3Time = 0;

            BuaTriTueX4 = false;
            BuaTriTueX4Time = 0;

            IsNearAuraTriLucItem = false;

            IsNearAuraTriTueItem = false;
            AuraTriTueTime = -1;

            IsNearAuraPhongThuItem = false;
            AuraPhongThuTime = -1;

            IsNearAuraSinhLucItem = false;

            PetItemIndex = -1;

            VuaGoiRong = true;
        }
    }
}