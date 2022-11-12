using System.Collections.Concurrent;
using System.Collections.Generic;
using NRO_Server.Application.Handlers.Map;
using NRO_Server.Application.Interfaces.Character;
using NRO_Server.Model.Character;
using NRO_Server.Model.Item;
using NRO_Server.Model.Monster;

namespace NRO_Server.Model.Map
{
    public class Zone
    {
        public int Id { get; set; }
        public int ItemMapId { get; set; }
        public Application.Threading.Map Map { get; set; }
        public ConcurrentDictionary<int, Character.Character> Characters { get; set; }
        public ConcurrentDictionary<int, Disciple> Disciples { get; set; }
        public ConcurrentDictionary<int, Boss> Bosses { get; set; }
        public ConcurrentDictionary<int, Pet> Pets { get; set; }
        public List<MonsterMap> MonsterMaps { get; set; } 
        public ConcurrentDictionary<int, MonsterPet> MonsterPets { get; set; }
        public ConcurrentDictionary<int, ItemMap> ItemMaps { get; set; }
        public ZoneHandler ZoneHandler { get; set; }

        public Zone(int id, Application.Threading.Map map)
        {
            Id = id;
            ItemMapId = 0;
            Map = map;
            Characters = new ConcurrentDictionary<int, Character.Character>();
            Disciples = new ConcurrentDictionary<int, Disciple>();
            Pets = new ConcurrentDictionary<int, Pet>();
            Bosses = new ConcurrentDictionary<int, Boss>();
            MonsterMaps = new List<MonsterMap>();
            ItemMaps = new ConcurrentDictionary<int, ItemMap>();
            MonsterPets = new ConcurrentDictionary<int, MonsterPet>();
            ZoneHandler = new ZoneHandler(this);
            ZoneHandler.InitMob();
        }
    }
}