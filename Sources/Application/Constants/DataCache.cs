using System.Collections.Generic;
using Newtonsoft.Json;
using NRO_Server.Model.Map;
using NRO_Server.Model.SkillCharacter;
using NRO_Server.Model.Template;

namespace NRO_Server.Application.Constants
{
    public class DataCache
    {
        public static JsonSerializerSettings SettingNull = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore
        };

        public static readonly long _1HOUR = 3660000L;
        public static readonly long _8HOURS = 28860000L;
        public static readonly long _1MONTH = 2629860000L;

        public static readonly long MAX_LIMIT_GOLD = 5000000000;
        public static readonly long PREMIUM_LIMIT_UP_POWER = 80000000000;

        public static readonly long TRUNG_MA_BU_TIME = 86400000;
        public static readonly int GIA_NO_TRUNG_MA_BU = 1100000000; //1.1ty

        public static readonly short SHOP_ID_NAPTHE = 3000; //1.1ty

        public static readonly int LIMIT_TRAIN_MANH_VO_BONG_TAI_NGAY = 10000; //1 ngày chỉ train 400 mảnh
        public static readonly int LIMIT_TRAIN_MANH_HON_BONG_TAI_NGAY = 396;

        public static readonly int LIMIT_SLOT_RUONG_PHU_THUONG_DE = 100;

        public static List<short> LIMIT_SO_LAN_GOI_RONG = new List<short> { 2, 10 };//tài khoản thường, tài khoản premium

        public static readonly int LIMIT_NOT_PREMIUM_TRADE_DAY = 5;//ngày giao dịch 5 lần

        public static readonly long LIMIT_NOT_PREMIUM_TRADE_TIME = 600000;//10p giao dich 1 lan
        // Max cấp bậc sức mạnh
        public static readonly int MAX_LIMIT_POWER_LEVEL = 17;
        // Mở khóa sức mạnh mặc định
        public static readonly int DEFAULT_LIMIT_POWER_LEVEL = 16;

        //Giá 1 ngọc trong vòng quay
        public static readonly int CRACK_BALL_PRICE = 25000000;

        //Chưa có VIP thì chỉ giao dịch max 100tr 1 lần
        public static readonly long LIMIT_NOT_PREMIUM_TRADE_GOLD_AMOUNT = 100000000;

        // Giá mở nội tại
        public static readonly int PRICE_UNLOCK_SPECIAL_SKILL = 200; //ngọc
        public static readonly int PRICE_UNLOCK_SPECIAL_SKILL_VIP = 50000000; //vàng

        public static readonly long MAX_PLUS_BAG = 40;

        public static readonly int GiaBanThoiVang = 500000000;

        public static List<int> TypeNewBoss_1 = new List<int> { 76 };
        public static List<int> TypeNewBoss_2 = new List<int> { 77 };

        public static List<short> IdCrackBall = new List<short> { 419, 420, 421, 422, 423, 424, 425 };
        public static List<int> IdMapCustom = new List<int> { 21, 22, 23, 39, 40, 41, 45, 46, 47, 48, 111 };

        public static List<int> IdMapCold = new List<int> { 105, 106, 107, 108, 109, 110 };

        public static List<int> IdMapThanhDia = new List<int> { 156, 157, 158, 159 };

        public static List<int> IdMapThucVat = new List<int> { 160, 161, 162, 163 };


        public static List<int> DefaultHair = new List<int> { 64, 30, 31, 6, 27, 28, 9, 29, 32 };

        public static List<List<short>> IdMob = new List<List<short>> {
           new List<short>() {281, 361, 351},
           new List<short>() {512, 513, 536},
           new List<short>() {514, 515, 537},
        };

        public static List<int> AvatarNotPart = new List<int> { 126, 127, 128, 273, 274, 275, 276, 277, 278, 279, 280, 281, 303, 304, 305, 389, 390, 560, 561, 562, 668, 769, 770, 771, 775, 776, 777, 825, 826, 827, 853, 854, 855, 856, 857, 900, 901, 902 };
        public static List<int> AvatarNotHead = new List<int> { 0, 6, 9, 27, 28, 29, 30, 31, 32, 64, 101, 102, 103, 104, 105, 107, 776, 112, 111, 113, 106, 108, 109, 110 };

        public static bool IsIdItemNotTrade(int id)
        {
            return id >= 650 && id <= 667;
        }
        public static List<int> ItemPremiumTrade = new List<int> { 457, 934, 886, 887, 888, 889, 987, 1048 };
        public static List<int> ItemNormalTrade = new List<int> { 379, 381, 382, 383, 384, 385 };
        public static List<int> TypeItemTrade = new List<int> { 0, 1, 2, 3, 4, 12, 14, 30 };
        public static List<int> IdAmulet = new List<int> { 213, 214, 215, 216, 217, 218, 219 };
        public static List<short> IdFlag = new List<short> { 363, 364, 365, 366, 367, 368, 369, 370, 371, 519, 520, 747 };
        public static List<int> IdDauThan = new List<int> { 13, 60, 61, 62, 63, 64, 65, 352, 523, 595 };
        public static List<int> IdDauThanx30 = new List<int> { 293, 294, 295, 296, 297, 298, 299, 596, 597 };
        public static List<long> UpgradeDauThanTime = new List<long> { 600000, 1800000, 3600000, 10800000, 43200000, 129600000, 432000000, 1296000000, 3456000000, 4456000000 };
        public static List<int> UpgradeDauThanGold = new List<int> { 5000, 25000, 75000, 450000, 1500000, 9000000, 45000000, 125000000, 300000000, 400000000 };

        public static List<int> TypeItemRemove = new List<int> { 6, 7, 5, 8, 9, 10, 11, 12, 27, 13, 15, 16, 22, 23, 24, 25, 28, 29, 30, 31, 33 };
        public static List<short> IdMount = new List<short> { 349, 350, 351, 396, 532, 346, 347, 348, 733, 734, 735, 743, 744, 746, 795, 849, 897, 920 };

        public static List<MapTranspot> GMapTranspots = new List<MapTranspot>()
        {
            null,   //0
            new MapTranspot() {Id = 47, Info = "Rừng Karin", Name = "Trái Đất"},
            new MapTranspot() {Id = 45, Info = "Thần Điện", Name = "Trái Đất"},
            new MapTranspot() {Id = 0, Info = "Làng Aru", Name = "Trái Đất"},
            new MapTranspot() {Id = 7, Info = "Làng Moori", Name = "Namếc"},
            new MapTranspot() {Id = 14, Info = "Làng Kakarot", Name = "Xay da"},
            new MapTranspot() {Id = 5, Info = "Đảo Kamê", Name = "Trái Đất"},
            new MapTranspot() {Id = 20, Info = "Vách núi đen", Name = "Xay da"},
            new MapTranspot() {Id = 13, Info = "Đảo Guru", Name = "Namếc"},
            null,    //9
            new MapTranspot() {Id = 27, Info = "Rừng Bamboo", Name = "Trái Đất"},
            new MapTranspot() {Id = 19, Info = "Thành phố Vegeta", Name = "Xay da"},
            new MapTranspot() {Id = 79, Info = "Núi khỉ đỏ", Name = "Fide"},
            new MapTranspot() {Id = 84, Info = "Siêu thị", Name = "Xay da"},
        };

        public static List<long> TimeUseSkill = new List<long>() { 900000, 1800000, 3600000, 86400000, 259200000, 604800000, 1296000000 };

        public static List<short> ListDaNangCap = new List<short>() { 220, 221, 222, 223, 224 };
        public static List<short> ListSaoPhaLe = new List<short>() { 441, 442, 443, 444, 445, 446, 447 };
        public static List<short> ListDoHuyDiet = new List<short>() { 650, 651, 652, 653, 654, 655, 656, 657, 658, 659, 660, 661, 662 };
        public static List<short> ListEventTrungThu = new List<short>() { 886, 887, 888, 889 };
        //562 564 566 561
        public static List<short> ListDoThanLinh = new List<short>() { 555, 556, 557, 558, 559, 560, 561, 563, 565, 567, 562, 566, 555, 556, 564, 557, 558, 559, 560, 563, 565, 567 };
        public static List<short> ListDoHiem = new List<short>() { 241, 253, 265, 277, 233, 245, 257, 269, 237, 249, 261, 273 };
        public static List<short> ListThucAn = new List<short>() { 663, 664, 665, 666, 667 };

        public static List<short> ListPetD = new List<short>(){
                892,//Thỏ xám
                893,//Thỏ trắng
                908,//Ma phong ba
                909,//Thần chết cute
                910,//Bí ngô nhí nhảnh
                892,//Thỏ xám
                893,//Thỏ trắng
                908,//Ma phong ba
                909,//Thần chết cute
                910,//Bí ngô nhí nhảnh
                916,//Lính Tam Giác
                917,//lính vuông
                918,//lính tròn
                919,//búp bê
                936,//tuần lộc nhí
                919,//búp bê
                936,//tuần lộc nhí
                919,//búp bê
                936,//tuần lộc nhí
                942,//hổ mặp vàng
                943,//hổ mặp trắng
                944,//hỏ mặp xanh
                967,//sao la
                1008,//cua đỏ
                967,//sao la
                1008,//cua đỏ
                967,//sao la
                1008,//cua đỏ
                1039,//Thỏ ốm
                1040,//thỏ mặp
                1046//khỉ bong bóng
                };
        
        public static List<int> PetIdOptionGoc = new List<int>() { 197,199,201,203,205 };
        
        public static List<int> PetIdOptionPlus = new List<int>() { 198,200,202,204,206 };

        public static List<int> PetTierIndex = new List<int>() { 197,198,199,200,201,202,203,204,205,206 };

        public static List<int> PetMaxChiSoTier = new List<int>() { 0,0,5,8,10,13,16,20,25,30 };
        #region Combinne
        public static readonly long MAX_LIMIT_UPGRADE = 8;
        public static readonly long MAX_LIMIT_SPL = 8;
        public static readonly int DIV_FAKE_PERCENT_PL = 4;//Chia tỉ lệ phần trăm thành công khi đục lổ trang bị, 50%/3 = 15% thành công
        public static readonly int DIV_FAKE_PERCENT_UPGRADE = 3;//

        public static List<string> TextColor = new List<string> { "brown", "green", "blue", "light-red", "light-green", "light-blue", "red" };

        public static List<int> IdOptionGoc = new List<int>() { 0, 6, 7, 22, 23, 14, 27, 28, 47 };
        public static List<int> IdOptionPlus = new List<int>() { 50, 77, 80, 81, 94, 95, 96, 97, 98, 99, 100, 101, 103, 108 };

        public static List<List<int>> OptionBall = new List<List<int>>{
            new List<int> {108, 2},
            new List<int> {94, 2},
            new List<int> {50, 3},
            new List<int> {81, 5},
            new List<int> {80, 5},
            new List<int> {103, 5},
            new List<int> {77, 5},
        };

        public static List<List<int>> OptionSPL = new List<List<int>>(){
            new List<int>() {95, 5},
            new List<int>() {96, 5},
            new List<int>() {97, 5},
            new List<int>() {98, 3},
            new List<int>() {99, 3},
            new List<int>() {100, 3},
            new List<int>() {101, 5},
        };

        public static int[][] PercentPhaLe = new int[][]{
            //vàng * 1tr, 50%, 10 ngọc
            new int[] {5, 50, 10},
            new int[] {10, 30, 20},
            new int[] {20, 20, 30},
            new int[] {40, 10, 40},
            new int[] {60, 6, 50},
            new int[] {90, DIV_FAKE_PERCENT_PL, 60},
            new int[] {120, DIV_FAKE_PERCENT_PL, 70},
            new int[] {180, DIV_FAKE_PERCENT_PL, 140}
        };

        public static List<List<int>> PercentUpgrade = new List<List<int>>{
            //số đá, vàng, %
            new List<int>(){2, 10, 80},
            new List<int>(){3, 20, 50},
            new List<int>(){4, 30, 20},
            new List<int>(){5, 40, 10},
            new List<int>(){6, 50, 5},
            new List<int>(){7, 60, 3},
            new List<int>(){8, 70, 1},
            new List<int>(){9, 80, 1}
        };

        public static List<int> PercentUpgradePorata2 = new List<int> { 100, 500000000, 50 };

        public static List<int> PercentUpgradePorata2Option = new List<int> { 100, 200000000, 40 };

        public static List<List<int>> OptionPorata2 = new List<List<int>>(){
            // option id , min, max
            new List<int>() {50, 7, 20},
            new List<int>() {77, 10, 20},
            new List<int>() {108, 5, 20},//né đòn
            new List<int>() {103, 10, 20},
            new List<int>() {94, 5, 15},
            new List<int>() {14, 2, 10},
        };

        public static List<int> CheckTypeUpgrade = new List<int>() { 223, 222, 224, 221, 220 };
        #endregion


        #region Skill
        public static List<short> TimeProtect = new List<short> { 3784, 150, 200, 250, 300, 350, 400, 450 };
        public static List<short> TimeHuytSao = new List<short> { 3781, 300 };
        public static List<short> TimeTroi = new List<short> { 3779, 50, 100, 150, 200, 250, 300, 360 };
        public static List<short> TimeDichChuyen = new List<short> { 3783, 10, 15, 20, 25, 30, 35, 40 };
        public static List<short> TimeStun = new List<short> { 3783, 3, 4, 5, 6, 7, 8, 9 };
        public static List<short> TimeThoiMien = new List<short> { 3782, 50, 60, 70, 80, 90, 100, 110 };
        public static List<short> IdMonsterPet = new List<short> { 8, 11, 32, 25, 43, 49, 50 };

        public static List<short> IdSkillDisciple1 = new List<short>() { 0, 0, 0, 2, 2, 2, 4, 4, 4 };
        public static List<short> IdSkillDisciple2 = new List<short>() { 1, 1, 3, 3, 3, 5, 5, 5 };
        public static List<short> IdSkillDisciple3 = new List<short>() { 9, 9, 9, 9, 6, 6, 6, 8, 9 };
        public static List<short> IdSkillDisciple4 = new List<short>() { 12, 12, 12, 12, 12, 19, 19, 13, 19 };
        public static List<short> RandomPointDisciple = new List<short>() { 0, 0, 0, 0, 0, 1, 1, 1, 0, 2, 2, 2, 2, 2, 2, 1, 1, 0, 1, 0, 0, 0, 2, 0, 2, 0, 2, 2, 1, 1, 0, 1, 0, 2, 1, 2, 0, 1, 0, 1, 0, 2, 1, 2 };

        public static List<SkillMonkey> SkillMonkeys = new List<SkillMonkey>()
        {
            new SkillMonkey() {Id = 192, Time = 60000, Hp = 40, Damage = 109},
            new SkillMonkey() {Id = 195, Time = 70000, Hp = 50, Damage = 110},
            new SkillMonkey() {Id = 196, Time = 80000, Hp = 60, Damage = 111},
            new SkillMonkey() {Id = 197, Time = 90000, Hp = 70, Damage = 112},
            new SkillMonkey() {Id = 198, Time = 100000, Hp = 80, Damage = 113},
            new SkillMonkey() {Id = 199, Time = 110000, Hp = 90, Damage = 114},
            new SkillMonkey() {Id = 200, Time = 120000, Hp = 100, Damage = 115},
        };

        public static List<int> SkillIdChuong = new List<int>() { 7, 8, 9, 10, 11, 12, 13, 21, 22, 23, 24, 25, 26, 27, 35, 36, 37, 38, 39, 40, 41 };
        public static List<int> SkillIdCanChien = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 28, 29, 30, 31, 32, 33, 34, 63, 64, 65, 66, 67, 68, 69 };
        #endregion

        #region Boss
        // ID BOSS
        public static readonly int BOSS_SUPER_BROLY_TYPE = 1;
        public static readonly int BOSS_BLACK_GOKU_TYPE = 2;
        public static readonly int BOSS_SUPER_BLACK_GOKU_TYPE = 3;

        public static readonly int BOSS_FIDE_01_TYPE = 4;
        public static readonly int BOSS_FIDE_02_TYPE = 5;
        public static readonly int BOSS_FIDE_03_TYPE = 6;

        public static readonly int BOSS_CELL_01_TYPE = 7;
        public static readonly int BOSS_CELL_02_TYPE = 8;
        public static readonly int BOSS_CELL_03_TYPE = 9;

        public static readonly int BOSS_COOLER_01_TYPE = 10;
        public static readonly int BOSS_COOLER_02_TYPE = 11;
        public static readonly int BOSS_THO_PHE_CO_TYPE = 12;
        public static readonly int BOSS_THO_DAI_CA_TYPE = 13;
        public static readonly int BOSS_CHILLED_TYPE = 14;
        public static readonly int BASE_BOSS_ID = 1;
        public static readonly int MAX_BASE_BOSS_ID = 9999;

        public static int CURRENT_BOSS_ID = 1;

        #endregion

        #region Lucky Box
        public static List<short> LuckBoxRare = new List<short>() { 729, 738, 738, 729, 738, 738, 883, 738, 738, 729, 738, 738, 883, 729, 738, 738, 729, 883, 729, 738, 729, 904, 738, 738, 729, 904, 729};
        public static List<short> LuckBoxEpic = new List<short>() { 342, 343, 344, 345, 17, 18, 19, 20 };
        public static List<short> LuckBoxCommon = new List<short>() { 190, 441, 190, 442, 190, 443, 190, 190, 444, 190, 445, 190, 190, 446, 447 };
        #endregion

        #region Special Skill
        public static List<int> SpecialSkillTSD = new List<int>() { 0, 1, 2, 3, 4, 5, 9, 13, 17 };
        public static List<int> SpecialSkillTGHP = new List<int>() { 7, 11, 12, 14, 10, 19, 2 };
        #endregion

        public static List<int> LogTheoDoi = new List<int>() { 23213 };

    }
}