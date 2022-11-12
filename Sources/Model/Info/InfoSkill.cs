using NRO_Server.Model.Info.Skill;

namespace NRO_Server.Model.Info
{
    public class InfoSkill
    {
        public TaiTaoNangLuong TaiTaoNangLuong { get; set; }
        public Monkey Monkey { get; set; }
        public TuSat TuSat { get; set; }
        public Protect Protect { get; set; }
        public HuytSao HuytSao { get; set; }
        public MeTroi MeTroi { get; set; }
        public PlayerTroi PlayerTroi { get; set; }
        public DichChuyen DichChuyen { get; set; }
        public ThoiMien ThoiMien { get; set; }
        public ThaiDuongHanSan ThaiDuongHanSan { get; set; }
        public QCKK Qckk { get; set; }
        public Laze Laze { get; set; }
        public Egg Egg { get; set; }
        public Socola Socola { get; set; }

        public InfoSkill()
        {
            TaiTaoNangLuong = new TaiTaoNangLuong();
            Monkey = new Monkey();
            TuSat = new TuSat();
            Protect = new Protect();
            HuytSao = new HuytSao();
            MeTroi = new MeTroi();
            PlayerTroi = new PlayerTroi();
            DichChuyen = new DichChuyen();
            ThoiMien = new ThoiMien();
            ThaiDuongHanSan = new ThaiDuongHanSan();
            Qckk = new QCKK();
            Laze = new Laze();
            Egg = new Egg();
            Socola = new Socola();
        }
    }
}