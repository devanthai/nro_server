using System.Collections.Generic;
using NRO_Server.Model;
using NRO_Server.Model.Data;
using NRO_Server.Model.Map;
using NRO_Server.Model.Template;
using NRO_Server.Model.Character;
using NRO_Server.Model.Item;

namespace NRO_Server.DatabaseManager
{
    public class Cache
    {
        private static Cache Instance;
        public readonly List<string> RegexText = new List<string>();

        public readonly List<Arrow> NRArrows = new List<Arrow>();
        public readonly List<Dart> NRDarts = new List<Dart>();
        public readonly List<Effect> NREffects = new List<Effect>();
        public readonly List<Image> NRImages = new List<Image>();
        public readonly List<Part> NRParts = new List<Part>();
        public readonly List<Skill> NRSkills = new List<Skill>();

        public readonly Dictionary<int, int[]> TASKS = new Dictionary<int, int[]>();
        public readonly Dictionary<int, int[]> MAPTASKS = new Dictionary<int, int[]>();
        public readonly Dictionary<short, TaskTemplate> TASK_TEMPLATES = new Dictionary<short, TaskTemplate>();
        public readonly List<long> EXPS = new List<long>();
        public readonly Dictionary<int, LimitPower> LIMIT_POWERS = new Dictionary<int, LimitPower>();
        public readonly List<Level> LEVELS = new List<Level>();
        public readonly Dictionary<int, int> AVATAR = new Dictionary<int, int>();
        public readonly List<BackgroundItemTemplate> BACKGROUND_ITEM_TEMPLATES = new List<BackgroundItemTemplate>();
        public readonly List<MonsterTemplate> MONSTER_TEMPLATES = new List<MonsterTemplate>();
        public readonly List<NpcTemplate> NPC_TEMPLATES = new List<NpcTemplate>();
        public readonly List<BossTemplate> BOSS_TEMPLATES = new List<BossTemplate>();
        public readonly List<ItemOptionTemplate> ITEM_OPTION_TEMPLATES = new List<ItemOptionTemplate>();
        public readonly Dictionary<short, ItemTemplate> ITEM_TEMPLATES = new Dictionary<short, ItemTemplate>();
        public readonly List<SkillTemplate> SKILL_TEMPLATES = new List<SkillTemplate>();
        public readonly List<SkillOption> SKILL_OPTIONS = new List<SkillOption>();
        public readonly List<TileMap> TILE_MAPS = new List<TileMap>();
        public readonly Dictionary<int, List<ShopTemplate>> SHOP_TEMPLATES = new Dictionary<int, List<ShopTemplate>>();
        public readonly Dictionary<int, List<SpecialSkillTemplate>> SPECIAL_SKILL_TEMPLATES = new Dictionary<int, List<SpecialSkillTemplate>>();
        public readonly List<GameInfoTemplate> GAME_INFO_TEMPLATES = new List<GameInfoTemplate>();
        public readonly List<NClass> NClasses = new List<NClass>();
        public readonly Dictionary<short, short> PARTS = new Dictionary<short, short>();
        public readonly Dictionary<int, List<byte>> DATA_MONSTERS_X1 = new Dictionary<int, List<byte>>();
        public readonly Dictionary<int, List<byte>> DATA_MONSTERS_x2 = new Dictionary<int, List<byte>>();
        public readonly Dictionary<int, List<byte>> DATA_MONSTERS_x3 = new Dictionary<int, List<byte>>();
        public readonly Dictionary<int, List<byte>> DATA_MONSTERS_x4 = new Dictionary<int, List<byte>>();
        
        public readonly Dictionary<int, List<byte>> DATA_ICON_X1 = new Dictionary<int, List<byte>>();
        public readonly Dictionary<int, List<byte>> DATA_ICON_X2 = new Dictionary<int, List<byte>>();
        public readonly Dictionary<int, List<byte>> DATA_ICON_X3 = new Dictionary<int, List<byte>>();
        public readonly Dictionary<int, List<byte>> DATA_ICON_X4 = new Dictionary<int, List<byte>>();
        
        public readonly Dictionary<int, List<List<int>>> DATA_MAGICTREE = new Dictionary<int, List<List<int>>>();
        public readonly List<ClanImage> CLAN_IMAGES = new List<ClanImage>();
        public readonly List<RadarTemplate> RADAR_TEMPLATE = new List<RadarTemplate>();

        public int[][] TILE_TYPES;
        public int[][][] TILE_INDEXS;

        public readonly List<sbyte> NR_DART = new List<sbyte>();
        public readonly List<sbyte> NR_ARROW= new List<sbyte>();
        public readonly List<sbyte> NR_IMAGE = new List<sbyte>();
        public readonly List<sbyte> NR_EFFECT = new List<sbyte>();
        public readonly List<sbyte> NR_PART = new List<sbyte>();
        public readonly List<sbyte> NR_SKILL = new List<sbyte>();
        public readonly List<sbyte> NRMAP = new List<sbyte>();
        public readonly List<sbyte> NRSKILL = new List<sbyte>();
        public readonly List<sbyte> DataItemTemplateOld = new List<sbyte>();
        public readonly List<sbyte> DataItemTemplateNew = new List<sbyte>();
        public readonly List<sbyte> VersionIconX1 = new List<sbyte>();
        public readonly List<sbyte> VersionIconX2 = new List<sbyte>();
        public readonly List<sbyte> VersionIconX3 = new List<sbyte>();
        public readonly List<sbyte> VersionIconX4 = new List<sbyte>();
        
        public static Cache Gi()
        {
            return Instance ??= new Cache();
        }

        public static void ClearCache()
        {
            Instance = null;
        }
    }
}