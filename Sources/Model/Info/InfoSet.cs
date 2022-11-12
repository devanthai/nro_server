namespace NRO_Server.Model.Info
{
    public class InfoSet
    {
        public bool IsFullSetThanLinh { get; set; }

        public bool IsFullSetKirin { get; set; }
        public bool IsFullSetSongoku { get; set; }
        public bool IsFullSetThienXinHang { get; set; }

        public bool IsFullSetNappa { get; set; }
        public bool IsFullSetKakarot { get; set; }
        public bool IsFullSetCadic { get; set; }

        public bool IsFullSetOcTieu { get; set; }
        public bool IsFullSetPicolo { get; set; }
        public bool IsFullSetPikkoro { get; set; }

        public InfoSet()
        {
            IsFullSetThanLinh = false;

            IsFullSetKirin = false;
            IsFullSetSongoku = false;
            IsFullSetThienXinHang = false;

            IsFullSetNappa = false;
            IsFullSetKakarot = false;
            IsFullSetCadic = false;

            IsFullSetOcTieu = false;
            IsFullSetPicolo = false;
            IsFullSetPikkoro = false;
        }

        public void Reset()
        {
            IsFullSetThanLinh = false;

            IsFullSetKirin = false;
            IsFullSetSongoku = false;
            IsFullSetThienXinHang = false;

            IsFullSetNappa = false;
            IsFullSetKakarot = false;
            IsFullSetCadic = false;

            IsFullSetOcTieu = false;
            IsFullSetPicolo = false;
            IsFullSetPikkoro = false;
        }
    }
}