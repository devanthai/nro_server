using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NRO_Server.Application.Threading;
using NRO_Server.Application.Interfaces.Character;
using NRO_Server.Application.Interfaces.Map;
using NRO_Server.Application.IO;
using NRO_Server.DatabaseManager.Player;
using NRO_Server.Model;

namespace NRO_Server.Application.Manager
{
    public class ClientManager : IClientManager
    {
        private static ClientManager Instance { get; set; }
        public int CurrentPlayers { get; set; }
        public ConcurrentDictionary<int, ISession_ME> Sessions { get; set; }
        public ConcurrentDictionary<int, Player> Players { get; set; }
        public ConcurrentDictionary<int, ICharacter> Characters { get; set; }

        public ConcurrentDictionary<int, long> UserLoginTime { get; set; }

        public ClientManager()
        {
            Sessions = new ConcurrentDictionary<int, ISession_ME>();
            Players = new ConcurrentDictionary<int, Player>();
            Characters = new ConcurrentDictionary<int, ICharacter>();
            UserLoginTime = new ConcurrentDictionary<int, long>();
            CurrentPlayers = 0;
            // StartHandleSession();
        }

        public static ClientManager Gi()
        {
            return Instance ??= new ClientManager();
        }

        public ISession_ME GetSession(int id)
        {
            return Sessions.FirstOrDefault(s => s.Key == id).Value;
        }

        public Player GetPlayer(int id)
        {
            return Players.FirstOrDefault(p => p.Key == id).Value;
        }

        public ICharacter GetCharacter(int id)
        {
            return Characters.FirstOrDefault(c => c.Key == id).Value;
        }

        public ICharacter GetRandomCharacter()
        {
            if (Characters.Count <= 0) return null;
            return Characters.ElementAt(ServerUtils.RandomNumber(0, Characters.Count)).Value;
        }

        public ICharacter GetCharacter(string name)
        {
            return Characters.FirstOrDefault(c => c.Value.Name == name).Value;
        }

        public void SendMessage(Message message)
        {
            lock (Sessions)
            {
                Sessions.Values.ToList().ForEach(session => session.SendMessage(message));
            }
        }
        
        public void SendMessageCharacter(Message message)
        {
            lock (Characters)
            {
                Characters.Values.ToList().ForEach(character => character.CharacterHandler.SendMessage(message));
            }
        }

        public void Add(ISession_ME session)
        {
            Sessions.TryAdd(session.Id, session);
        }

        public void Add(Player player)
        {
            CurrentPlayers++;
            Players.TryAdd(player.Id, player);
        }

        public void Add(ICharacter character)
        {
            Characters.TryAdd(character.Id, character);
        }

        public void Remove(ISession_ME session)
        {
            if (session == null) return;
            var oldSession = GetSession(session.Id);
            if (oldSession == null)
            {
                Server.Gi().Logger.Error($"Error Client Remove Session: Khong the tim thay ID {session.Id}");
                return;
            }

            Remove(oldSession.Player);
            Sessions.TryRemove(oldSession.Id, out var sessionMe);
        }

        public void Remove(Player player)
        {
            if (player == null) return;
            var oldPlayer = GetPlayer(player.Id);
            if (oldPlayer == null) 
            {
                Server.Gi().Logger.Error($"Error Client Remove Player: Khong the tim thay ID {player.Id}");
                return;
            }

            CurrentPlayers--;

            try
            {
                ((Model.Character.Character)oldPlayer.Character).Delay.StartLogout = true;
            }
            catch (Exception)
            {
                Server.Gi().Logger.Error($"Error Remove Player, khong the set Start Logout cho User ID: {player.Id}");
            }
            
            Remove(oldPlayer.Character);

            oldPlayer.IsOnline = false;
            UserDB.Update(oldPlayer);
            Players.TryRemove(oldPlayer.Id, out var playerRemove);
        }

        public void Remove(ICharacter character)
        {
            if(character == null) return;
            var oldCharacter = GetCharacter(character.Id);
            if (oldCharacter == null) 
            {
                Server.Gi().Logger.Error($"Error Client Remove character: Khong the tim thay ID {character.Id}");
                return;
            }
            character.CharacterHandler.Close();
            character.CharacterHandler.Dispose();
            Characters.TryRemove(oldCharacter.Id, out var characterRemove);
        }

        public void KickSession(ISession_ME session)
        {
            if(session == null) return;
            Remove(session);     
            session.Disconnect();
        }

        public void Clear()
        {
            lock (Sessions)
            {
                Sessions.Values.ToList().ForEach(session =>
                {
                    KickSession(session);
                    Thread.Sleep(100);
                });   
            }
        }

        private void StartHandleSession()
        {
            async void Action()
            {
                long t1;
                long t2;
                while (true)
                {
                    t1 = ServerUtils.CurrentTimeMillis();
                    await Update();
                    t2 = ServerUtils.CurrentTimeMillis() - t1;
                    await Task.Delay((int)Math.Abs(10000-t2));
                }
            }
            var HandleSession = new Task(Action);
            HandleSession.Start();
        }

        private async Task Update()
        {
            // var timeServer = ServerUtils.CurrentTimeSecond();
            // // Kiểm tra tất cả session chưa đăng nhập thời gian kết nối, nếu quá 60s chưa đăng nhập thì kick
            // Sessions.Values.Where(s => s.IsLogin == false).ToList().ForEach(session => 
            // {
            //     if (timeServer - session.TimeConnected >= 60)
            //     {
            //         KickSession(session);
            //         // Nếu IP này kết nối hơn 7 lần thì ban 30p
            //     }
            // });
            await Task.Delay(10);
        }
    }
}