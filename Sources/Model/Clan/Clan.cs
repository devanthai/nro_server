using System.Collections.Concurrent;
using System.Collections.Generic;
using NRO_Server.Application.Handlers.Character;
using NRO_Server.Application.Interfaces.Character;
using NRO_Server.Application.IO;
using NRO_Server.Model.Info;
using NRO_Server.Model.Clan;

namespace NRO_Server.Model.Clan
{
    public class Clan
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Slogan { get; set; }
        public int ImgId { get; set; }
        public long Power { get; set; }
        public string LeaderName { get; set; }
        public int CurrMember { get; set; }
        public int MaxMember { get; set; }
        public int Level { get; set; }
        public int Point { get; set; }
        public int Date { get; set; }
        public List<ClanMember> Members { get; set; }
        public List<ClanMessage> Messages { get; set; }
        public IClanHandler ClanHandler { get; set; }
        public List<CharacterPea> CharacterPeas { get; set; }
        public long DelayUpdate { get; set; }
        public bool IsSave { get; set; }

        public Clan()
        {
            Slogan = "";
            Level = 1;
            Point = 0;
            Members = new List<ClanMember>();
            Messages = new List<ClanMessage>();
            CharacterPeas = new List<CharacterPea>();
            ClanHandler = new ClanHandler(this);
            DelayUpdate = 300000 + ServerUtils.CurrentTimeMillis();
            IsSave = true;
        }
    }
}