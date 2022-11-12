using System;
using NRO_Server.Application.Interfaces.Character;
using NRO_Server.Application.Interfaces.Monster;
using NRO_Server.Application.IO;
using NRO_Server.Application.Main;
using NRO_Server.Application.Threading;

namespace NRO_Server.Application.Handlers.Monster
{
    public class MonsterPetHandler : IMonsterHandler
    {
        public IMonster Monster { get; set; }
        public void SetUpMonster()
        {
             //ignore
        }

        public void Recovery()
        {
            //Ignore
        }

        public int UpdateHp(long damage, int charId, bool isMaxHp = false)
        {
            if (Monster.Character.InfoSet.IsFullSetPikkoro)
            {
                return 0;
            }

            if(damage >= Monster.Hp)
            {
                damage = Monster.Hp;
            }
            Monster.Hp -= damage;
            if(Monster.Hp <= 0) StartDie();
            return (int)damage;
        }

        private void StartDie()
        {
            Monster.Zone.ZoneHandler.SendMessage(Service.UpdateMonsterMe6(Monster.IdMap));
            Monster.Hp = 0;
            Monster.IsDie = true;
            Monster.Status = 0;
            Monster.Character?.CharacterHandler.RemoveMonsterMe();
        }

        public void LeaveItem(ICharacter character)
        {
            //ignore
            Server.Gi().Logger.Debug($"Monster Pet Handler Leave Item");
        }

        public int PetAttackMonster(IMonster monster)
        {
            long damage = ServerUtils.RandomNumber(Monster.Damage * 9 / 10, Monster.Damage);
            var damageMonsterAfterHandle = 0;

            if (Monster.Character.InfoSet.IsFullSetPikkoro)
            {
                damage*=2;
            }

            if (ServerUtils.RandomNumber(100) <= 25)
            {
                damageMonsterAfterHandle = 0;
            }
            else
            {
                damageMonsterAfterHandle = monster.MonsterHandler.UpdateHp(damage, Monster.IdMap, true);
            }
            // Monster.Zone.ZoneHandler.SendMessage(Service.UpdateMonsterMe1(Monster.IdMap, monster.IdMap));
            // Hút máu, ki
            if (damageMonsterAfterHandle > 0)
            {
                var hpPlus = damageMonsterAfterHandle * Monster.Character.HpPlusFromDamage / 100;
                var mpPlus = damageMonsterAfterHandle * Monster.Character.MpPlusFromDamage / 100;
                var hpPlusMonster = damageMonsterAfterHandle * Monster.Character.HpPlusFromDamageMonster / 100;

                hpPlus += hpPlusMonster > 0 ? hpPlusMonster : 0;
                
                if(hpPlus > 0) {
                    Monster.Character.CharacterHandler.PlusHp(hpPlus);
                    if (Monster.Character.Id > 0)
                    {
                        Monster.Character.CharacterHandler.SendMessage(Service.SendHp((int)Monster.Character.InfoChar.Hp));
                    }
                    Monster.Character.CharacterHandler.SendZoneMessage(Service.PlayerLevel(Monster.Character));
                }

                if(mpPlus > 0) {
                    Monster.Character.CharacterHandler.PlusMp(mpPlus);
                    if (Monster.Character.Id > 0)
                    {
                        Monster.Character.CharacterHandler.SendMessage(Service.SendMp((int)Monster.Character.InfoChar.Mp));
                    }
                }
            }

            Monster.Zone.ZoneHandler.SendMessage(Service.UpdateMonsterMe3(Monster.IdMap, monster.IdMap, (int)monster.Hp, damageMonsterAfterHandle));
            Monster.Zone.ZoneHandler.SendMessage(Service.MonsterHp(monster, false, damageMonsterAfterHandle, -1));
            return damageMonsterAfterHandle;
        }

        public void PetAttackPlayer(ICharacter character)
        {
            if(character == null) return;
            try
            {
                var damage = Monster.Damage;
                var damageReal = ServerUtils.RandomNumber(damage * 9 / 10, damage);
                damageReal -= character.DefenceFull;
                if (ServerUtils.RandomNumber(100) < 20)
                {
                    damage = 0;
                }
                character.CharacterHandler.MineHp(damageReal);
                Monster.Zone.ZoneHandler.SendMessage(Service.UpdateMonsterMe2(Monster.IdMap, character.Id, damage, (int)character.InfoChar.Hp));;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error HandleTroiMonster in SkillHandler.cs: {e.Message} \n {e.StackTrace}", e);
            }
        }

        public void MonsterAttack(ICharacter temp, ICharacter character)
        {
            //Ignore
        }

        public void Update()
        {
            var timeServer = ServerUtils.CurrentTimeMillis();
            if (!Monster.IsDie)
            {
                if (Monster.Character?.InfoSkill.Egg.Time <= timeServer)
                {
                    StartDie();
                }
            }
        }

        public void AddPlayerAttack(ICharacter character, int damage)
        {
            //Ignore
        }

        public void RemoveTroi(int charId)
        {
            //Ignore
        }

        public MonsterPetHandler(IMonster monster)
        {
            Monster = monster;
        }
    }
}