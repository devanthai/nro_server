using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NRO_Server.Application.Interfaces.Map;
using NRO_Server.DatabaseManager;
using NRO_Server.Model.Character;
using NRO_Server.Application.Interfaces.Character;

namespace NRO_Server.Application.Manager
{
    public class MapManager
    {
        public static int IdBase = 0;
        public static readonly ConcurrentDictionary<int, IMapCustom> Enrtys = new ConcurrentDictionary<int, IMapCustom>();
        private static readonly List<Threading.Map> Maps = new List<Threading.Map>();

        public static bool isDragonHasAppeared = false;
        public static long delayCallDragon = 0;

        public static Threading.Map Get(int id)
        {
            return Maps.FirstOrDefault(x => x.Id == id);
        }

        public static IMapCustom GetMapCustom(int id)
        {
            lock (Enrtys)
            {
                return Enrtys.Values.FirstOrDefault(x => x.Id == id);  
            }
        }

        public static void InitMapServer()
        {
            Cache.Gi().TILE_MAPS.ForEach(x =>
            {
                Maps.Add(new Threading.Map(x.Id, x, null));
            });
        }

        public static void JoinMap(Character @char, int mapId, int zoneId, bool isDefault, bool isTeleport, int typeTeleport)
        {
            var map = Get(mapId);
            map?.JoinZone(@char, zoneId, isDefault, isTeleport, typeTeleport );
        }

        public static void OutMap(Character @char, int mapNextId)
        {
            var map = Get(@char.InfoChar.MapId);
            map?.OutZone(@char, mapNextId);
        }

        // For only once dragon apprea
        public static void SetDragonAppeared(bool toggle)
        {
            isDragonHasAppeared = toggle;
        }

        public static bool IsDragonHasAppeared()
        {
            return isDragonHasAppeared;
        }
    }
}