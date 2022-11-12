using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NRO_Server.Application.Interfaces.Character;
using NRO_Server.Application.Interfaces.Map;
using NRO_Server.Application.Manager;
using NRO_Server.Application.Threading;
using NRO_Server.Application.Helper;
using NRO_Server.Application.Constants;
using NRO_Server.DatabaseManager;

namespace NRO_Server.Application.Map
{
    public class NewGame : IMapCustom
    {
        
        
        public int Id { get; set; }
        public IList<Threading.Map> Maps { get; set; }
        public IList<ICharacter> Characters { get; set; }
        public bool IsClear { get; set; }
        public bool IsOutMap { get; set; }
        public long Time { get; set; }
        public int Gender { get; set; }
        public object LOCK { get; set; }

        public NewGame(int gender)
        {
            Id = MapManager.IdBase++;
            Gender = gender;
            Time = -1;
            IsClear = false;
            IsOutMap = false;
            Maps = new List<Threading.Map>(1);
            Characters = new List<ICharacter>(1);
            InitMap();
            LOCK = new object();
            MapManager.Enrtys.TryAdd(Id, this);
        }
        
        public void InitMap()
        {
            Maps.Add(new Threading.Map(Gender+39, tileMap:null, mapCustom:this));
        }
        
        public void Update()
        {
            if (IsOutMap && Characters.Count < 1)
            {
                Clear();
            }
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
                    Server.Gi().Logger.Error($"Error Clear in NewGame.cs: {e.Message} \n {e.StackTrace}", e);
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
                    Server.Gi().Logger.Error($"Error RemoveCharacter in NewGame.cs: {Characters.IndexOf(character)} {e.Message} \n {e.StackTrace}", e);
                }
            }
        }
        
        public bool IsClose()
        {
            return IsOutMap && Characters.Count < 1;
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