using System;
using System.Collections.Generic;
using NRO_Server.Model;
using NRO_Server.Model.Info;
using NRO_Server.Model.Map;
using NRO_Server.Model.SkillCharacter;

namespace NRO_Server.Application.Interfaces.Character
{
    public interface ICharacter : IDisposable
    {
         ICharacterHandler CharacterHandler { get; set; }
         InfoSkill InfoSkill { get; set; }
         Zone Zone { get; set; }
         int Id { get; set; }
         int ClanId { get; set; }
         Player Player { get; set; }
         string Name { get; set; }
         InfoChar InfoChar { get; set; }
         int TypeTeleport { get; set; }
         sbyte Flag { get; set; }
         long HpFull { get; set; }
         long MpFull { get; set; }
         int DamageFull { get; set; }
         int DefenceFull { get; set; }
         int CritFull { get; set; }
         int HpPlusFromDamage { get; set; }
         int MpPlusFromDamage { get; set; }
         int HpPlusFromDamageMonster { get; set; }
         bool IsGetHpFull { get; set; }
         bool IsGetMpFull { get; set; }
         bool IsGetDamageFull { get; set; }
         bool IsGetDefenceFull { get; set; }
         bool IsGetCritFull { get; set; }
         bool IsHpPlusFromDamage { get; set; }
         bool IsMpPlusFromDamage { get; set; }
         int DiemSuKien { get; set; }
         List<SkillCharacter> Skills { get; set; }
         List<Model.Item.Item> ItemBody { get; set; }
         List<Model.Item.Item> ItemBag { get; set; }
         List<Model.Item.Item> ItemBox { get; set; }
         InfoOption InfoOption { get; set; }
         InfoSet InfoSet { get; set; }
         bool IsInvisible();
         short GetHead(bool isMonkey = true);
         short GetBody(bool isMonkey = true);
         short GetLeg(bool isMonkey = true);
         short GetBag();
         void SetGetFull(bool isGet);

         int LengthBagNull();
         int LengthBoxNull();

         int BagLength();
         int BoxLength();
         int BodyLength();
         bool CheckLockInventory();

         bool IsDontMove();


    }
}