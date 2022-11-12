using System;
using System.Collections.Generic;
using System.Linq;
using Linq.Extras;
using NRO_Server.Application.Constants;
using NRO_Server.Application.Handlers.Character;
using NRO_Server.Application.Interfaces.Character;
using NRO_Server.Application.IO;
using NRO_Server.DatabaseManager;
using NRO_Server.Model.ModelBase;
using NRO_Server.Model.Data;
using NRO_Server.Model.Info;
using NRO_Server.Model.Map;

namespace NRO_Server.Model.Character
{
    public class Boss : CharacterBase
    {
        public int Status { get; set; }
        public int Type { get; set; }
        public ICharacter CharacterFocus { get; set; }
        public InfoDelayBoss InfoDelayBoss { get; set; }
        public short Hair { get; set; }
        public short BasePositionX { get; set; }
        public short BasePositionY { get; set; }
        public short RangeMove { get; set; }
        public int KillerId { get; set; }
        public List<int> CharacterAttack { get; set; }
        public bool KhangTroi { get; set; }
        public Boss()
        {
            Status = 0;
            InfoChar.Power = 2000;
            Name = "Boss";
            ClanId = -1;
            KillerId = -1;
            CharacterAttack = new List<int>();
            CharacterFocus = null;
            InfoChar.Stamina = 12050;
            InfoChar.MaxStamina = 12050;
            InfoChar.Pk = 1;
            InfoChar.TypePk = 3;
            InfoDelayBoss = new InfoDelayBoss();
            CharacterHandler = new BossHandler(this);
        }

        public void CreateBoss(int type, short x = 0, short y = 0)
        {
            var bossTemplate = Cache.Gi().BOSS_TEMPLATES.FirstOrDefault(boss => boss.Type == type);
            if (bossTemplate == null) return;

            InfoChar.Gender = 3;
            InfoChar.Power = 2000;
            InfoChar.Level = (sbyte)Cache.Gi().EXPS.Count(exp => exp < InfoChar.Power);
            Status = 0;
            Name = bossTemplate.Name;
            ClanId = -1;
            KillerId = -1;
            CharacterAttack = new List<int>();
            Id = DataCache.CURRENT_BOSS_ID;
            DataCache.CURRENT_BOSS_ID += 1;
            Type = type;
            InfoChar.Stamina = bossTemplate.Stamina;
            InfoChar.MaxStamina = bossTemplate.Stamina;
            InfoChar.OriginalHp = InfoChar.Hp = bossTemplate.Hp;
            InfoChar.OriginalMp = InfoChar.Mp = bossTemplate.Mp;
            InfoChar.OriginalDamage = bossTemplate.Damage;
            InfoChar.OriginalDefence = bossTemplate.Defence;
            InfoChar.OriginalCrit = bossTemplate.CritChance;
            InfoChar.X = x;
            InfoChar.Y = y;
            BasePositionX = x;
            BasePositionY = y;
            InfoChar.TypePk = 5;
            Skills = bossTemplate.Skills;
            Hair = bossTemplate.Hair;
            KhangTroi = bossTemplate.KhangTroi;
            InfoDelayBoss = new InfoDelayBoss();
            CharacterHandler = new BossHandler(this);
        }

        public override short GetHead(bool isMonkey = true)
        {
            return Hair;
        }

        public override short GetBody(bool isMonkey = true)
        {
            if (Type == DataCache.BOSS_SUPER_BLACK_GOKU_TYPE)
            {
                return (short)551;
            }
            return (short)(Hair + 1);
        }

        public override short GetLeg(bool isMonkey = true)
        {
            if (Type == DataCache.BOSS_SUPER_BLACK_GOKU_TYPE)
            {
                return (short)552;
            }
            return (short)(Hair + 2);
        }
    }
}