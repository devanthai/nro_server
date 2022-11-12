using System;
using System.Collections.Generic;
using NRO_Server.Application.Constants;
using NRO_Server.Application.Interfaces.Character;
using NRO_Server.Application.IO;
using NRO_Server.Application.Main;
using NRO_Server.DatabaseManager;
using NRO_Server.Model.Info;
using NRO_Server.Model.Map;
using static System.GC;

namespace NRO_Server.Model.ModelBase
{
    public class CharacterBase : ICharacter
    {

        public ICharacterHandler CharacterHandler { get; set; }
        public Zone Zone { get; set; }
        public int Id { get; set; }
        public int ClanId { get; set; }
        public Player Player { get; set; }
        public string Name { get; set; }
        public InfoChar InfoChar { get; set; }
        public int TypeTeleport { get; set; }
        public sbyte Flag { get; set; }
        public long HpFull { get; set; }
        public long MpFull { get; set; }
        public int DamageFull { get; set; }
        public int DefenceFull { get; set; }
        public int CritFull { get; set; }
        public int HpPlusFromDamage { get; set; }
        public int MpPlusFromDamage { get; set; }
        public int HpPlusFromDamageMonster { get; set; }
        public bool IsGetHpFull { get; set; }
        public bool IsGetMpFull { get; set; }
        public bool IsGetDamageFull { get; set; }
        public bool IsGetDefenceFull { get; set; }
        public bool IsGetCritFull { get; set; }
        public bool IsHpPlusFromDamage { get; set; }
        public bool IsMpPlusFromDamage { get; set; }
        public int DiemSuKien { get; set; }
        public List<SkillCharacter.SkillCharacter> Skills { get; set; }
        public List<Item.Item> ItemBody { get; set; }
        public List<Item.Item> ItemBag { get; set; }
        public List<Item.Item> ItemBox { get; set; }
        public InfoSkill InfoSkill { get; set; }
        public Effect.Effect Effect { get; set; }
        public InfoOption InfoOption { get; set; }
        public InfoSet InfoSet { get; set; }
        public bool IsInvisible()
        {
            return false;
        }

        public CharacterBase()
        {
            ClanId = -1;
            Flag = 0;
            InfoChar = new InfoChar();
            HpFull = InfoChar.OriginalHp;
            MpFull = InfoChar.OriginalMp;
            DamageFull = InfoChar.OriginalDamage;
            DefenceFull = InfoChar.OriginalDefence;
            CritFull = InfoChar.OriginalCrit;
            Skills = new List<SkillCharacter.SkillCharacter>();
            ItemBody = new List<Item.Item>(7);
            for(var i = 0; i < 7; i++) ItemBody.Add(null);
            ItemBag = new List<Item.Item>();
            ItemBox = new List<Item.Item>();
            InfoSkill = new InfoSkill();
            Effect = new Effect.Effect();
            InfoOption = new InfoOption();
            InfoSet = new InfoSet();
            SetGetFull(true);
        }

        public virtual short GetHead(bool isMonkey = true)
        {
            if(isMonkey && InfoSkill.Socola.IsCarot)  return 406;
            if(isMonkey && InfoSkill.Socola.IsSocola) return 412;
            if (isMonkey && InfoSkill.Monkey.HeadMonkey != -1) return InfoSkill.Monkey.HeadMonkey;

            if (InfoChar.Fusion.IsFusion) {
                if (InfoChar.Gender == 1 && !InfoChar.Fusion.IsPorata2) {
                    return 391;
                }

                if (InfoChar.Fusion.IsPorata2)
                {
                    switch (InfoChar.Gender)
                    {
                        case 0:
                        {
                            return 870;
                        }
                        case 1:
                        {
                            return 873;
                        }
                        case 2:
                        {
                            return 867;
                        }
                    }
                }
                else if (InfoChar.Fusion.IsPorata)
                {
                    return 383;
                }
                else 
                {
                    return 380;
                }
            }
                   
            var item = ItemBody[5];
            if (item == null) return InfoChar.Hair;

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
            return part == -1 ? InfoChar.Hair : part;
        }

        public virtual short GetBody(bool isMonkey = true)
        {
            if(isMonkey && InfoSkill.Socola.IsCarot)  return 407;
            if(isMonkey && InfoSkill.Socola.IsSocola)  return 413;
            if (isMonkey && InfoSkill.Monkey.BodyMonkey != -1) return InfoSkill.Monkey.BodyMonkey;
            var headPart = GetHead();
            if (InfoChar.Fusion.IsFusion) 
            {
                return (short)(headPart + 1);
            }
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
            return InfoChar.Gender == 1 ? (short)59 : (short)57;
        }

        public virtual short GetLeg(bool isMonkey = true)
        {
            if(isMonkey && InfoSkill.Socola.IsCarot)  return 408;
            if(isMonkey && InfoSkill.Socola.IsSocola)  return 414;
            if (isMonkey && InfoSkill.Monkey.LegMonkey != -1) return InfoSkill.Monkey.LegMonkey;        
            var headPart = GetHead();
            if (InfoChar.Fusion.IsFusion) 
            {
                return (short)(headPart + 2);
            }
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
            return InfoChar.Gender == 1 ? (short)60 : (short)58;
        }

        public short GetBag()
        {
            return InfoChar.PhukienPart > 0 ? InfoChar.PhukienPart : InfoChar.Bag;
        }

        public void SetGetFull(bool isGet)
        {
            IsGetHpFull = isGet;
            IsGetMpFull = isGet;
            IsGetDamageFull = isGet;
            IsGetDefenceFull = isGet;
            IsGetCritFull = isGet;
            IsHpPlusFromDamage = isGet;
            IsMpPlusFromDamage = isGet;
        }

        public virtual int LengthBagNull()
        {
            return 20 - ItemBag.Count;
        }

        public virtual int LengthBoxNull()
        {
            return 20 - ItemBox.Count;
        }

        public virtual int BagLength()
        {
            return 20;
        }

        public virtual int BoxLength()
        {
            return 20;
        }
        
        public virtual int BodyLength()
        {
            return 7;
        }

        public bool CheckLockInventory()
        {
            if (InfoChar.LockInventory.IsLock)
            {
                CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().LOCK_INVENTORY));
                return false;
            }
            return true;
        }

        public bool IsDontMove()
        {
            var timeServer = ServerUtils.CurrentTimeMillis();
            if (InfoChar.IsDie || InfoSkill.ThaiDuongHanSan.IsStun || InfoSkill.DichChuyen.IsStun
                || InfoSkill.PlayerTroi.IsPlayerTroi
                || InfoSkill.ThoiMien.IsThoiMien
                || InfoSkill.Monkey.IsStart || InfoSkill.TuSat.Delay > timeServer || InfoSkill.Laze.Time > timeServer || InfoSkill.Qckk.Time > timeServer) return true;
            return false;
        }

        public void Clear() => SuppressFinalize(this);

        public void Dispose()
        {
            SuppressFinalize(this);
        }
    }
}