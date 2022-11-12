using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NRO_Server.Model.Clan;

namespace NRO_Server.Application.Manager
{
    public class ClanManager
    {
        public static readonly ConcurrentDictionary<int, Clan> Entrys = new ConcurrentDictionary<int, Clan>();

        public static Clan Get(int id)
        {
            return Entrys.FirstOrDefault(entry => entry.Key == id).Value;
        }

        public static Clan Get(string name)
        {
            return Entrys.Values.FirstOrDefault(entry => entry.Name == name);
        }

        public static List<Clan> GetList(string name)
        {
            return Entrys.Values.Where(entry =>
            {
                if (entry.Name == name) return true;
                var arrName = new []{entry.Name};
                var arrCheck = new[] {name};
                return arrName.ToList().FirstOrDefault(i => arrCheck.ToList().Contains(i)) != null;
            }).ToList();
        }

        public static void Add(Clan clan)
        {
            if(clan == null) return;
            Entrys.TryAdd(clan.Id, (Clan) clan);
        }

        public static void Remove(Clan clan)
        {
            if(clan == null) return;
            Entrys.TryRemove(new KeyValuePair<int, Clan>(clan.Id, clan));
        }

        public static void Remove(int id)
        {
            var clan = Get(id);
            if(clan == null) return;
            Entrys.TryRemove(new KeyValuePair<int, Clan>(clan.Id, clan));
        }

        public static void Clear()
        {
            lock (Entrys)
            {
                Entrys.Values.ToList().ForEach(clan => clan.ClanHandler.Flush());
            } 
            Entrys.Clear();
        }
    }
}