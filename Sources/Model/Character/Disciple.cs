using System;
using System.Linq;
using Linq.Extras;
using NRO_Server.Application.Constants;
using NRO_Server.Application.Handlers.Character;
using NRO_Server.Application.Interfaces.Monster;
using NRO_Server.Application.IO;
using NRO_Server.DatabaseManager;
using NRO_Server.Model.Info;
using NRO_Server.Model.Info.InfoDiscipler;
using NRO_Server.Model.ModelBase;
using NRO_Server.Model.Data;

namespace NRO_Server.Model.Character
{
    public class Disciple : CharacterBase
    {
        public Character Character { get; set; }
        public int Status { get; set; }
        public int Type { get; set; }
        public long LevelPercent { get; set; }
        public bool IsFire { get; set; }
        public bool IsBienHinh { get; set; }
        public IMonster MonsterFocus { get; set; }
        public InfoDelayDisciple InfoDelayDisciple { get; set; }
        public PlusPoint PlusPoint { get; set; }

        public Disciple(Character character)
        {
            Status = 0;
            InfoChar.Power = 2000;
            Name = "Đệ tử";
            Type = 1;
            Id = -character.Id;
            Character = character;
            InfoChar.Stamina = 1250;
            InfoChar.MaxStamina = 1250;
            IsFire = true;
            MonsterFocus = null;
            IsBienHinh = false;
            PlusPoint = new PlusPoint();
            InfoDelayDisciple = new InfoDelayDisciple();
            CharacterHandler = new DiscipleHandler(this);
        }

        public Disciple()
        {
            Status = 0;
            InfoChar.Power = 2000;
            Name = "Đệ tử";
            Type = 1;
            InfoChar.Stamina = 1250;
            InfoChar.MaxStamina = 1250;
            IsFire = true;
            IsBienHinh = false;
            MonsterFocus = null;
            PlusPoint = new PlusPoint();
            InfoDelayDisciple = new InfoDelayDisciple();
            CharacterHandler = new DiscipleHandler(this);
        }

        public void CreateNewDisciple(Character character)
        {
            InfoChar.Gender = (sbyte)ServerUtils.RandomNumber(3);
            InfoChar.Power = 2000;
            InfoChar.Level = (sbyte)Cache.Gi().EXPS.Count(exp => exp < InfoChar.Power);
            Status = 0;
            Name = "Đệ tử";
            Type = 1;
            Id = -character.Id;
            Character = character;
            Zone = character.Zone;
            InfoChar.Stamina = 1250;
            InfoChar.MaxStamina = 1250;
            IsFire = true;
            InfoChar.OriginalHp = InfoChar.Hp = ServerUtils.RandomNumber(980, 2200);
            InfoChar.OriginalMp = InfoChar.Mp = ServerUtils.RandomNumber(980, 2200);
            InfoChar.OriginalDamage = ServerUtils.RandomNumber(20, 60);
            InfoChar.OriginalDefence = ServerUtils.RandomNumber(20, 50);
            InfoChar.OriginalCrit = ServerUtils.RandomNumber(100) < 10 ? ServerUtils.RandomNumber(3, 6) : ServerUtils.RandomNumber(1, 4);

            var randomSkill = DataCache.IdSkillDisciple1[ServerUtils.RandomNumber(DataCache.IdSkillDisciple1.Count)];
            Skills.Add(new SkillCharacter.SkillCharacter()
            {
                Id = randomSkill,
                SkillId = GetSkillId(randomSkill),
                Point = 1,
            });
            PlusPoint = new PlusPoint();
            InfoDelayDisciple = new InfoDelayDisciple();
            CharacterHandler = new DiscipleHandler(this);
        }

        public void CreateNewMaBuDisciple(Character character, sbyte gender)
        {
            InfoChar.Gender = gender;
            InfoChar.Power = 1500000;
            InfoChar.Level = (sbyte)Cache.Gi().EXPS.Count(exp => exp < InfoChar.Power);
            Status = 0;
            Name = "Đệ tử Mabư";
            Type = 2;
            Id = -character.Id;
            Character = character;
            Zone = character.Zone;
            InfoChar.Stamina = 2400;
            InfoChar.MaxStamina = 2400;
            IsFire = true;
            InfoChar.OriginalHp = InfoChar.Hp = ServerUtils.RandomNumber(2200, 3200);
            InfoChar.OriginalMp = InfoChar.Mp = ServerUtils.RandomNumber(2200, 3200);
            InfoChar.OriginalDamage = ServerUtils.RandomNumber(20, 60);
            InfoChar.OriginalDefence = ServerUtils.RandomNumber(20, 50);
            InfoChar.OriginalCrit = ServerUtils.RandomNumber(100) < 10 ? ServerUtils.RandomNumber(4, 8) : ServerUtils.RandomNumber(2, 5);
            var randomSkill = DataCache.IdSkillDisciple1[ServerUtils.RandomNumber(DataCache.IdSkillDisciple1.Count)];
            Skills.Add(new SkillCharacter.SkillCharacter()
            {
                Id = randomSkill,
                SkillId = GetSkillId(randomSkill),
                Point = 1,
            });
            PlusPoint = new PlusPoint();
            InfoDelayDisciple = new InfoDelayDisciple();
            CharacterHandler = new DiscipleHandler(this);
        }

        public static int GetSkillId(int id)
        {
            return id switch
            {
                19 => 121,
                _ => id * 7
            };
        }

        private short HeadLevel()
        {
            // Ma bư
            if (Type == 2)
            {
                return 297;
            }

            return InfoChar.Gender switch
            {
                0 => InfoChar.Power <= 1500000 ? (short) 285 : (short) 304,
                1 => InfoChar.Power <= 1500000 ? (short) 288 : (short) 305,
                _ => InfoChar.Power <= 1500000 ? (short) 282 : (short) 303
            };
        }

        private short BodyLevel()
        {
            // Ma bư
            if (Type == 2)
            {
                return 298;
            }

            return InfoChar.Gender switch
            {
                0 => 286,
                1 => 289,
                _ => 283
            };
        }

        private short LegLevel()
        {
            // Ma bư
            if (Type == 2)
            {
                return 299;
            }

            return InfoChar.Gender switch
            {
                0 => 287,
                1 => 290,
                _ => 284
            };
        }

        public override short GetHead(bool isMonkey = true)
        {
            switch (isMonkey)
            {
                case true when InfoSkill.Socola.IsSocola:
                    return 609;
                case true when InfoSkill.Monkey.HeadMonkey != -1:
                    return InfoSkill.Monkey.HeadMonkey;
            }

            if (InfoChar.Fusion.IsFusion) return 383;
            var item = ItemBody[5];
            if (item == null || (Type == 2 && !IsBienHinh)) return HeadLevel();

            var itemTemplate = ItemCache.ItemTemplate(item.Id);
            var part = itemTemplate.Part;
            //Check part #1
            if (part == -1)
            {
                try
                {
                    part = Cache.Gi().PARTS[itemTemplate.IconId];
                }
                catch (Exception)
                {
                    part = ItemCache.PartNotAvatar(item.Id);
                }
            }
            //Check part #2
            return part == -1 ? HeadLevel() : part;
        }

        public override short GetBody(bool isMonkey = true)
        {
            switch (isMonkey)
            {
                case true when InfoSkill.Socola.IsCarot:
                    return 407;
                case true when InfoSkill.Socola.IsSocola:
                    return 413;
                case true when InfoSkill.Monkey.BodyMonkey != -1:
                    return InfoSkill.Monkey.BodyMonkey;
            }

            if (InfoChar.Fusion.IsFusion) return 384;

            if (Type == 2 && !IsBienHinh) return BodyLevel();

            var headPart = GetHead();
            var item = ItemBody[5];
            if (item != null && !ItemCache.IsItemAvtNotHead(headPart))
            {
                return ItemCache.PartHeadToBody(headPart);
            }

            item = ItemBody[0];
            if (item != null)
            {
                return ItemCache.ItemTemplate(item.Id).Part;
            }
            return BodyLevel();
        }

        public override short GetLeg(bool isMonkey = true)
        {
            switch (isMonkey)
            {
                case true when InfoSkill.Socola.IsSocola:
                    return 611;
                case true when InfoSkill.Monkey.LegMonkey != -1:
                    return InfoSkill.Monkey.LegMonkey;
            }

            if (InfoChar.Fusion.IsFusion) return 385;

            if (Type == 2 && !IsBienHinh) return LegLevel();

            var headPart = GetHead();
            var item = ItemBody[5];
            if (item != null && !ItemCache.IsItemAvtNotHead(headPart))
            {
                return ItemCache.PartHeadToLeg(headPart);
            }

            item = ItemBody[1];
            if (item != null)
            {
                return ItemCache.ItemTemplate(item.Id).Part;
            }
            return LegLevel();
        }

        public string CurrStrLevel()
        {
            GetDataLevel();
            var levels = Cache.Gi().LEVELS.Where(x => x.Gender == InfoChar.Gender).ToList();
            return $"{levels[InfoChar.Level].Name} {LevelPercent/100}.{LevelPercent%100}%" ;
        }

        private void GetDataLevel()
        {
            try
            {
                var num = 1L;
                var num2 = 0L;
                var num3 = 0;
                for (var num4 = Cache.Gi().EXPS.Count - 1; num4 >= 0; num4--)
                {
                    if (InfoChar.Power < Cache.Gi().EXPS[num4]) continue;
                    num = ((num4 != Cache.Gi().EXPS.Count - 1) ? (Cache.Gi().EXPS[num4 + 1] - Cache.Gi().EXPS[num4]) : 1);
                    num2 = InfoChar.Power - Cache.Gi().EXPS[num4];
                    num3 = num4;
                    break;
                }
                InfoChar.Level = (sbyte)num3;
                LevelPercent = (int)(num2 * 10000 / num);
            }
            catch (Exception)
            {
                //Ignored
            }
        }
    }
}