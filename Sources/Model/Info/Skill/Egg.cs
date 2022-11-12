using NRO_Server.Model.Monster;

namespace NRO_Server.Model.Info.Skill
{
    public class Egg
    {
        public MonsterPet Monster{ get; set; }
        public long Time { get; set; }

        public Egg()
        {
            Monster = null;
            Time = -1;
        }
    }
}