using NRO_Server.Application.IO;

namespace NRO_Server.Model.Info
{
    public class ClanMember
    {
        public int Id { get; set; }
        public short Head { get; set; }
        public short Leg { get; set; }
        public short Body { get; set; }
        public string Name { get; set; }
        public int Role { get; set; }
        public long Power { get; set; }
        public int Donate { get; set; }
        public int ReceiveDonate { get; set; }
        public int ClanPoint { get; set; }
        public int CurClanPoint { get; set; }
        public int JoinTime { get; set; } 
        public int LastRequest { get; set; }  
        public long DelayPea { get; set; }  

        public ClanMember()
        {
            Name = "";
        }

        public ClanMember(Character.Character character)
        {
            Id = character.Id;
            Head = character.GetHead(false);
            Leg = character.GetLeg(false);
            Body = character.GetBody(false);
            Name = character.Name;
            Role = 0;
            Power = character.InfoChar.Power;
            Donate = 0;
            ReceiveDonate = 0;
            ClanPoint = 0;
            CurClanPoint = 0;
            LastRequest = 0;
            JoinTime = (int)ServerUtils.CurrentTimeMillis()/10000;
        }
    }
}