using System;
using System.Collections.Generic;
using System.Linq;
using Linq.Extras;
using NRO_Server.Application.Constants;
using NRO_Server.Application.Handlers.Skill;
using NRO_Server.Application.Interfaces.Character;
using NRO_Server.Application.Interfaces.Monster;
using NRO_Server.Application.IO;
using NRO_Server.Application.Main;
using NRO_Server.Application.Handlers.Item;
using NRO_Server.Application.Threading;
using NRO_Server.DatabaseManager;
using NRO_Server.DatabaseManager.Player;
using NRO_Server.Model;
using NRO_Server.Model.Info;
using NRO_Server.Model.Character;
using NRO_Server.Model.Map;
using NRO_Server.Model.SkillCharacter;
using Org.BouncyCastle.Math.Field;
using static System.GC;

namespace NRO_Server.Application.Handlers.Character
{
    public class PetHandler : ICharacterHandler
    {
        public Pet Pet { get; set; }

        public PetHandler(Pet pet)
        {
            Pet = pet;
        }

        public void Dispose()
        {
            SuppressFinalize(this);
        }

        public void SendZoneMessage(Message message)
        {
            Pet?.Zone?.ZoneHandler.SendMessage(message);
        }

        public void Update()
        {
            lock (Pet)
            {
                var timeServer = ServerUtils.CurrentTimeMillis();
                AutoPet(timeServer);
            }
        }

        private void AutoPet(long timeServer)
        {
            if(Pet.IsDontMove()) return;
            if (Math.Abs(Pet.InfoChar.X - Pet.Character.InfoChar.X) >= 120)
            {
                SetUpPosition(isRandom:true);
                SendZoneMessage(Service.PlayerMove(Pet.Id, Pet.InfoChar.X, Pet.InfoChar.Y));
            }
            AutoMoveMap(timeServer);
        }

        private void AutoMoveMap(long timeServer, bool isForce = false)
        {
            if ((Pet.DelayAutoMove <= timeServer) || isForce)
            {
                Pet.InfoChar.X = (short)ServerUtils.RandomNumber(Pet.Character.InfoChar.X - 30,
                    Pet.Character.InfoChar.X + 30);
                SendZoneMessage(Service.PlayerMove(Pet.Id, Pet.InfoChar.X, Pet.InfoChar.Y));
                Pet.DelayAutoMove = timeServer + ServerUtils.RandomNumber(10000, 20000);
            }
        }

        private void SendChatForSp(string text)
        {
            Pet.Character.CharacterHandler.SendMessage(Service.PublicChat(Pet.Id, text));
        }

        public void Close()
        {
            Clear();
        }

        public void Clear()
        {
            SuppressFinalize(this);
        }

        public void UpdateInfo()
        {
            SendZoneMessage(Service.UpdateBody(Pet));
        }

        public void SetUpPosition(int mapOld = -1, int mapNew = -1, bool isRandom = false)
        {
            if (isRandom)
            {
                Pet.InfoChar.X = (short) (Pet.Character.InfoChar.X + 15);
            }
            else
            {
                Pet.InfoChar.X = Pet.Character.InfoChar.X;
            }
            Pet.InfoChar.Y = Pet.Character.InfoChar.Y;
        }
        

        public void PlusHp(int hp)
        {
            lock (Pet.InfoChar)
            {
                if(Pet.InfoChar.IsDie) return;
                Pet.InfoChar.Hp += hp;
                if (Pet.InfoChar.Hp >= Pet.HpFull) Pet.InfoChar.Hp = Pet.HpFull;
            }
        }

        public void MineHp(long hp)
        {
            lock (Pet.InfoChar)
            {
                if(Pet.InfoChar.IsDie || hp <= 0) return;
                if (hp > Pet.InfoChar.Hp)
                {
                    Pet.InfoChar.Hp = 0;
                }
                else 
                {
                    Pet.InfoChar.Hp -= hp;
                }

                if (Pet.InfoChar.Hp <= 0)
                {
                    Pet.InfoChar.IsDie = true;
                    Pet.InfoChar.Hp = 0;
                }
            }
        }

        
        public void MoveMap(short toX, short toY, int type = 0)
        {
            var timeServer = ServerUtils.CurrentTimeMillis();
            if(Pet.IsDontMove()) return;

            var compare = Math.Abs(Pet.InfoChar.X - toX);
            if (compare >= 50)
            {
                if (Pet.InfoChar.X < toX)
                {
                    Pet.InfoChar.X = compare switch
                    {
                        >= 150 => (short) (toX - 50),
                        _ => (short) (toX - 30)
                    };
                }
                else
                {
                    Pet.InfoChar.X = compare switch
                    {
                        >= 150 => (short) (toX + 50),
                        _ => (short) (toX + 30)
                    };
                }

                if (toY != Pet.InfoChar.Y)
                {
                    Pet.InfoChar.Y = toY;
                }

                SendZoneMessage(Service.PlayerMove(Pet.Id, Pet.InfoChar.X, Pet.InfoChar.Y));
                if (Pet.InfoSkill.MeTroi.IsMeTroi && Pet.InfoSkill.MeTroi.DelayStart <= timeServer)
                {
                    SkillHandler.RemoveTroi(Pet);
                }
            }
        }


        #region Ignored Function
        private void HandleUseSkill(bool isAuto = true, int charId = -1, int modId = -1)
        {
            
        }

        public void SendDie()
        {

        }

        public void SendInfo()
        {
        }

        public int GetParamItem(int id)
        {
            return 0;
        }

        public List<int> GetListParamItem(int id)
        {
            return null;
        }

        public void SetUpInfo()
        {
        }

        public void SetInfoSet()
        {
        }

        public void LeaveFromDead(bool isHeal = false)
        {

        }

        public void SetEnhancedOption()
        {
        }

        public void SetHpFull()
        {
        }

        public void SetMpFull()
        {
        }

        public void SetDamageFull()
        {
        }

        public void SetDefenceFull()
        {
        }

        public void SetCritFull()
        {
        }

        public void SetHpPlusFromDamage()
        {
        }

        public void SetMpPlusFromDamage()
        {
        }

        public void SetSpeed()
        {
        }

        public void SetBuffHp30s()
        {
        }

        public void SetBuffMp1s()
        {
        }
        
        public void SetBuffHp5s()
        {
            //TODO set buff 5s
        }

        public void SetBuffHp10s()
        {
            //TODO set buff 10s
        }
        public void PlusMp(int mp)
        {
        }

        public void MineMp(int mp)
        {
        }

        public void PlusStamina(int stamina)
        {
        }

        public void MineStamina(int stamina)
        {
        }

        public void PlusPower(long power)
        {
        }

        public void PlusPotential(long potential)
        {
        }

        public Model.Item.Item RemoveItemBody(int index)
        {
            return null;
        }

        public void AddItemToBody(Model.Item.Item item, int index)
        {
        }

        public void RemoveMonsterMe()
        {
        }
        public void PlusPoint(IMonster monster, int damage)
        {
            
        }

        public void PlusPoint(long power, long potential, bool isAll)
        {

        }

        public void RemoveSkill(long timeServer, bool globalReset = false)
        {
            // ignore
        }

        public void UpdateEffect(long timeServer)
        {
            // ignore
        }

        public void UpdateMask(long timeServer)
        {
            // ignore
        }

        public void UpdateAutoPlay(long timeServer)
        {
            
        }

        public void UpdateLuyenTap()
        {
            
        }

        public void RemoveTroi(int charId)
        {
            // ignore
        }

        public void SetPlayer(Player player)
        {
            //Set player
        }

        public void SendMessage(Message message)
        {
            //ignore
        }
        
        public void SendMeMessage(Message message)
        {
            //ignore
        }
        public void HandleJoinMap(Zone zone)
        {
            //Pet join map
        }

        public void BagSort()
        {
            //ignore
        }

        public void BoxSort()
        {
            //ignore
        }
        public void Upindex(int index)
        {
            //ignore
        }
        public bool AddItemToBag(bool isUpToUp, Model.Item.Item item, string reason = "")
        {
            //ignore
            return false;
        }

        public bool AddItemToBox(bool isUpToUp, Model.Item.Item item)
        {
            //ignore
            return false;
        }
        
        public void ClearTest()
        {
            //Clear test
        }
        
        public void DropItemBody(int index)
        {
            //ignore
        }

        public void DropItemBag(int index)
        {
            //ignore
        }

        public void PickItemMap(short id)
        {
            //ignore
        }

        public void UpdateMountId()
        {
            //ignore
        }
        public void UpdatePhukien()
        {
            //ignore
        }
        public Model.Item.Item GetItemBagByIndex(int index)
        {
            //ignore
            return null;
        }

        public Model.Item.Item GetItemBagById(int id)
        {
            //ignore
            return null;
        }

        public Model.Item.Item GetItemBoxByIndex(int index)
        {
            //ignore
            return null;
        }
        public Model.Item.Item GetItemLuckyBoxByIndex(int index)
        {
            //ignore
            return null;
        }
        public Model.Item.Item GetItemBoxById(int id)
        {
            //ignore
            return null;
        }

        
        public void BackHome()
        {
            //Ignore
        }
        
        public void RemoveItemBagById(short id, int quantity, string reason = "")
        {
            //ignore
        }

        public void RemoveItemBagByIndex(int index, int quantity, bool reset = true, string reason = "")
        {
            //ignore
        }

        public void RemoveItemBoxByIndex(int index, int quantity, bool reset = true)
        {
            //ignore
        }

        public Model.Item.Item RemoveItemBag(int index, bool isReset = true, string reason = "")
        {
            return null;
        }

                
        
        public Model.Item.Item ItemBagNotMaxQuantity(short id)
        {
            //ignore
            return null;
        }
        
        public Model.Item.Item RemoveItemBox(int index, bool isReset = true)
        {
            return null;
        }
        public Model.Item.Item RemoveItemLuckyBox(int index, bool isReset = true)
        {
            return null;
        }

        public void SetUpFriend()
        {
            //Ignore
        }

        public void LeaveItem(ICharacter character)
        {
            // Ignore
        }

        #endregion
    }
}