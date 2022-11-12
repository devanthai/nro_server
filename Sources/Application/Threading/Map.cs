using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NRO_Server.Application.Constants;
using NRO_Server.Application.Interfaces.Character;
using NRO_Server.Application.Interfaces.Map;
using NRO_Server.Application.IO;
using NRO_Server.Application.Manager;
using NRO_Server.DatabaseManager;
using NRO_Server.Model.Character;
using NRO_Server.Model.Item;
using NRO_Server.Model.Map;
using static System.GC;

namespace NRO_Server.Application.Threading
{
    public class Map
    {
        public int Id { get; set; }
        public List<Zone> Zones { get; set; }
        public TileMap TileMap { get; set; }
        public long TimeMap { get; set; }
        public bool IsRunning { get; set; }
        public bool IsStop { get; set; }
        public IMapCustom MapCustom { get; set; }
        public Task HandleZone { get; set; }

        public Map()
        {
            
        }

        public Map(int id, TileMap tileMap, IMapCustom mapCustom, bool isStart = true)
        {
            Id = id;
            Zones = new List<Zone>();
            TimeMap = -1;
            IsRunning = false;
            IsStop = false;
            MapCustom = mapCustom;
            TileMap = tileMap ?? Cache.Gi().TILE_MAPS.FirstOrDefault(t => t.Id == Id);
            SetZone();
            if(isStart) Start();
        }

        public void Start()
        {
            if (IsRunning) return;
            IsRunning = true;
            StartHanleZone();
        }

        private void StartHanleZone()
        {
            // Console.WriteLine("Handle Map " + TileMap.Name + " Zone count: " + Zones.Count);
            async void Action()
            {
                long t1;
                long t2;
                while (IsRunning)
                {
                    t1 = ServerUtils.CurrentTimeMillis();
                    await Update();
                    t2 = ServerUtils.CurrentTimeMillis() - t1;
                    await Task.Delay((int)Math.Abs(1000-t2));
                }
                Zones.ForEach(zone => zone.ZoneHandler.Close());
                Zones.Clear();
                Zones = null;
                TileMap = null;
                HandleZone = null;
                if (MapCustom == null) return;
                if (MapManager.Enrtys.TryRemove(MapCustom.Id, out var mapRemove))
                {
                    MapCustom = null;
                }
                SuppressFinalize(this);
            }
            HandleZone = new Task(Action);
            HandleZone.Start();
        }
        
        public void Close()
        {
            IsRunning = false;
        }

        public void SetZone()
        {
            for (var i = 0; i < TileMap.ZoneNumbers; i++)
            {
                Zones.Add(new Zone(i, this));
            }

            if (Id is not (21 or 22 or 23)) return;
            var item = ItemCache.GetItemDefault(74);
            short x = 0;
            short y = 0;
            switch (Id)
            {
                case 22:
                {
                    x = 55;
                    y = 325;
                    break;
                }
                case 21:
                case 23:
                {
                    x = 632;
                    y = 325;
                    break;
                }
            }
            Zones[0].ItemMaps.TryAdd(0, new ItemMap(-1)
            {
                Id = 0,
                PlayerId = -1,
                Item = item,
                X = x,
                Y = y
            });
        }

        private async Task Update()
        {
            MapCustom?.Update();
            Parallel.ForEach(Zones, ZoneUpdate);
            await Task.Delay(10);
        }

        private async void ZoneUpdate(Zone zone)
        {
            if (zone.Characters.Count > 0)
            {
                await zone.ZoneHandler.Update();
            }
        }

        public void JoinZone(Character character, int id, bool isDefault = false, bool isTeleport = false, int typeTeleport = 0)
        {
            if (id == -1)
            { 
                GetZoneNotMaxPlayer()?.ZoneHandler.JoinZone(character, isDefault, isTeleport, typeTeleport);
            }
            else
            {
                GetZoneById(id)?.ZoneHandler.JoinZone(character, isDefault, isTeleport, typeTeleport);
            }
        }

        public Zone GetZoneNotMaxPlayer()
        {
            return Zones.FirstOrDefault(x => x.Characters.Count < TileMap.MaxPlayers);
        }

        public Zone GetZonePlayer()
        {
            return Zones.FirstOrDefault(x => x.Characters.Count >= 1);//4
        }

        public Character GetChar(int id)
        {
            var zonn = Zones.FirstOrDefault(x => x.Id == id);
            return zonn == null ? null : zonn.Characters.FirstOrDefault(c => c.Key > 0).Value;
        }

        public Zone GetZoneNotMaxPlayer(int id)
        {
            return Zones.FirstOrDefault(x => x.Id == id && x.Characters.Count < TileMap.MaxPlayers);
        }
        
        public Zone GetZoneById(int id)
        {
            return Zones.FirstOrDefault(x => x.Id == id);
        }
            

        public void OutZone(ICharacter character, int mapNextId)
        {
            var isOutZone = mapNextId != Id && MapCustom?.GetMapById(mapNextId) == null && DataCache.IdMapCustom.Contains(Id);
            Zones.FirstOrDefault(x => x.Id == character.InfoChar.ZoneId)?.ZoneHandler?.OutZone(character, isOutZone);
        }
        
        public bool IsMapCustom()
        {
            return MapCustom != null || DataCache.IdMapCustom.Contains(Id);
        }
    }
}