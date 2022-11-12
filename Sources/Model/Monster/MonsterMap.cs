
using NRO_Server.Application.Handlers.Monster;
using NRO_Server.Model.ModelBase;

namespace NRO_Server.Model.Monster
{
    public class MonsterMap : MonsterBase
    {
        public MonsterMap()
        {
            IsMobMe = false;
            IsDie = false;
            MonsterHandler = new MonsterMapHanlder(this);
        }
    }
}