using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NRO_Server.Model.Character;

namespace NRO_Server.Application.Manager
{
    public class MagicTreeManager
    {
        public static readonly ConcurrentDictionary<int, MagicTree> Entrys = new ConcurrentDictionary<int, MagicTree>();

        public static MagicTree Get(int id)
        {
            return Entrys.Values.FirstOrDefault(entry => entry.Id == id);
        }

        public static void Add(MagicTree magicTree)
        {
            if(magicTree == null) return;
            Entrys.TryAdd(magicTree.Id, magicTree);
        }

        public static void Remove(MagicTree magicTree)
        {
            if(magicTree == null) return;
            Entrys.TryRemove(new KeyValuePair<int, MagicTree>(magicTree.Id, magicTree));
        }

        public static void Remove(int id)
        {
            var magicTree = Get(id);
            if(magicTree == null) return;
            Entrys.TryRemove(new KeyValuePair<int, MagicTree>(magicTree.Id, magicTree));
        }

        public static void Clear()
        {
            lock (Entrys)
            {
                Entrys.Values.ToList().ForEach(tree => tree.MagicTreeHandler.Flush());
            } 
            Entrys.Clear();
        }
    }
}