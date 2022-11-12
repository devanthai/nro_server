using System;
using System.Collections.Generic;
using NRO_Server.Application.Interfaces.Monster;
using NRO_Server.Application.IO;
using NRO_Server.Model;
using NRO_Server.Model.Map;

namespace NRO_Server.Application.Interfaces.Character
{
    public interface ICharacterHandler : IDisposable
    {
        void SetPlayer(Player player);
        void SendMessage(Message message);
        void SendZoneMessage(Message message);
        void SendMeMessage(Message message);
        void Update();
        void Close();
        void UpdateInfo();
        void SetUpPosition(int mapOld = -1, int mapNew = -1, bool isRandom = false);

        void SendInfo();
        void SendDie();
        public int GetParamItem(int id);

        public List<int> GetListParamItem(int id);

        public void SetUpFriend();
        public void SetUpInfo();

        public void SetHpFull();

        public void SetMpFull();

        public void SetDamageFull();
        public void SetDefenceFull();
        public void SetCritFull();
        public void SetHpPlusFromDamage();
        public void SetMpPlusFromDamage();

        public void HandleJoinMap(Zone zone);

        public void SetSpeed();

        public void BagSort();
        public void BoxSort();
        public void Upindex(int index);
        bool AddItemToBag(bool isUpToUp, Model.Item.Item item, string reason = "");
        
        void AddItemToBody(Model.Item.Item item, int index);

        bool AddItemToBox(bool isUpToUp, Model.Item.Item item);
        Model.Item.Item ItemBagNotMaxQuantity(short id);
        void RemoveItemBagById(short id, int quantity, string reason = "");
        void RemoveItemBagByIndex(int index, int quantity, bool reset = true, string reason = "");
        void RemoveItemBoxByIndex(int index, int quantity, bool reset = true);
        Model.Item.Item RemoveItemBag(int index, bool isReset = true, string reason = "");
        Model.Item.Item RemoveItemBox(int index, bool isReset = true);
        Model.Item.Item RemoveItemLuckyBox(int index, bool isReset = true);
        void MoveMap(short toX, short toY, int type = 0);
        void PlusHp(int hp);
        void MineHp(long hp);
        void PlusMp(int mp);
        void MineMp(int mp);
        void PlusStamina(int stamina);
        void MineStamina(int stamina);
        void PlusPower(long power);
        void PlusPotential(long potential);
        Model.Item.Item RemoveItemBody(int index);
        void DropItemBody(int index);
        void DropItemBag(int index);
        void PickItemMap(short id);
        void UpdateMountId();
        void UpdatePhukien();
        Model.Item.Item GetItemBagByIndex(int index);
        Model.Item.Item GetItemBagById(int id);
        Model.Item.Item GetItemBoxByIndex(int index);
        Model.Item.Item GetItemLuckyBoxByIndex(int index);
        Model.Item.Item GetItemBoxById(int id);
        void RemoveMonsterMe();
        void PlusPoint(IMonster monster, int damage);
        void PlusPoint(long power, long potential, bool isAll);
        void LeaveFromDead(bool isHeal = false);
        void LeaveItem(ICharacter character);
        void BackHome();

        void RemoveSkill(long timeServer, bool globalReset = false);
        void UpdateEffect(long timeServer);
        void UpdateMask(long timeServer);
        void UpdateAutoPlay(long timeServer);
        void UpdateLuyenTap();

        void ClearTest();
        void RemoveTroi(int charId);
        void SetBuffHp30s();
        void SetBuffMp1s();
        void SetBuffHp5s();
        void SetBuffHp10s();
        void Clear();

    }
}