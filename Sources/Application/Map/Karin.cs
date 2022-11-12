using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NRO_Server.Application.Interfaces.Character;
using NRO_Server.Application.Interfaces.Map;
using NRO_Server.Application.Manager;
using NRO_Server.Application.Threading;
using NRO_Server.Application.Helper;

namespace NRO_Server.Application.Map
{
    public class Karin : IMapCustom
    {
        public int Id { get; set; }
        public IList<Threading.Map> Maps { get; set; }
        public IList<ICharacter> Characters { get; set; }
        public bool IsClear { get; set; }
        public bool IsOutMap { get; set; }
        public long Time { get; set; }
        public object LOCK { get; set; }

        public Karin()
        {
            Id = MapManager.IdBase++;
            Time = -1;
            IsClear = false;
            IsOutMap = false;
            Maps = new List<Threading.Map>(1);
            Characters = new List<ICharacter>(1);
            InitMap();
            LOCK = new object();
            MapManager.Enrtys.TryAdd(Id, this);
        }
        public void Update()
        {
            if (IsOutMap && Characters.Count < 1)
            {
                Clear();
            }
        }

        public void InitMap()
        {
            Maps.Add(new Threading.Map(47, tileMap:null, mapCustom:this));
            Maps.Add(new Threading.Map(46, tileMap:null, mapCustom:this));
            Maps.Add(new Threading.Map(45, tileMap:null, mapCustom:this));
            Maps.Add(new Threading.Map(48, tileMap:null, mapCustom:this));
            Maps.Add(new Threading.Map(111, tileMap:null, mapCustom:this));
        }

        public void Clear()
        {
            lock (LOCK)
            {
                if (IsClear) return;
                IsClear = true;
                try
                {
                    while (Characters.Count > 0)
                    {
                        var character = Characters.RemoveAndGetItem(0);
                        if (character != null)
                        {
                            //TODO logic remove character in map
                        }
                    }
                }
                catch (Exception e)
                {
                    Server.Gi().Logger.Error($"Error Clear in Karin.cs: {e.Message} \n {e.StackTrace}", e);
                }
                finally
                {
                    foreach (var map in Maps)
                    {
                        map.Close();
                    }
                }
            }
        }

        public void AddCharacter(ICharacter character)
        {
            lock (Characters)
            {
                if(Characters.FirstOrDefault(x => x.Id == character.Id) == null) Characters.Add(character);
            }
        }

        public void RemoveCharacter(ICharacter character)
        {
            lock (Characters)
            {
                var indexRemove = Characters.IndexOf(character);
                if (indexRemove == -1) return;
                try
                {
                    Characters.RemoveAt(indexRemove);
                }
                catch (Exception e)
                {
                    Server.Gi().Logger.Error($"Error RemoveCharacter in Karin.cs: {Characters.IndexOf(character)} {e.Message} \n {e.StackTrace}", e);
                }
                // Characters.RemoveAt(Characters.IndexOf(character));
            }
        }

        public bool IsClose()
        {
            if (IsOutMap && Characters.Count < 1) return true;
            return false;
        }

        public Threading.Map GetMapById(int id)
        {
            return Maps.FirstOrDefault(map => map.Id == id);
        }

        public Threading.Map GetMapByIndex(int index)
        {
            return Maps.ElementAt(index);
        }
    }
}