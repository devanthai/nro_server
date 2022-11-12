using System.Threading.Tasks;
using NRO_Server.Application.Interfaces.Character;
using NRO_Server.Application.IO;
using NRO_Server.Model.Character;
using NRO_Server.Model.Item;
using NRO_Server.Model.Map;
using NRO_Server.Model.Monster;
using NRO_Server.Model;

namespace NRO_Server.Application.Interfaces.Map
{
    public interface IZoneHandler
    {
        Zone Zone { get; set; }
        void JoinZone(Model.Character.Character _char, bool isDefault, bool isTeleport, int typeTeleport);
        void OutZone(ICharacter _char, bool isOutZone = false);
        void InitMob();
        Task Update();
        void Close();
        void SendMessage(Message message, bool isSkillMessage);
        void SendMessage(Message message, int id);
        void LeaveItemMap(ItemMap itemMap);
        void LeaveItemMap(ItemMap itemMap, MonsterMap monster);
        void RemoveItemMap(int id);
        int GetItemMapNotId();
        void RemoveCharacter();
        void AddDisciple(Disciple disciple);
        void RemoveDisciple(Disciple disciple);
        void AddPet(Pet pet);
        void RemovePet(Pet pet);
        void RemoveMonsterMe(int id);
        ICharacter GetCharacter(int id);
        MonsterMap GetMonsterMap(int id);
    }
}