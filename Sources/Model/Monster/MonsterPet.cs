
using System.Security.Policy;
using NRO_Server.Application.Handlers.Monster;
using NRO_Server.Application.Interfaces.Character;
using NRO_Server.Model.ModelBase;
using Zone = NRO_Server.Model.Map.Zone;

namespace NRO_Server.Model.Monster
{
    public class MonsterPet : MonsterBase
    {
        public MonsterPet(ICharacter character, Map.Zone zone, short template, long hp, int damage)
        {
            IdMap = (short)character.Id;
            Id = template;
            X = character.InfoChar.X;
            Y = character.InfoChar.Y;
            Zone = zone;
            Character = character;
            IsMobMe = true;
            HpMax = hp;
            OriginalHp = hp;
            Hp = hp;
            Damage = damage;
            IsDie = false;
            MonsterHandler = new MonsterPetHandler(this);
        }
    }
}