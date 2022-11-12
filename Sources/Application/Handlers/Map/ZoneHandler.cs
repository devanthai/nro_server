using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NRO_Server.Application.Constants;
using NRO_Server.Application.Handlers.Skill;
using NRO_Server.Application.Interfaces.Character;
using NRO_Server.Application.Interfaces.Map;
using NRO_Server.Application.IO;
using NRO_Server.Application.Main;
using NRO_Server.Application.Manager;
using NRO_Server.Application.Threading;
using NRO_Server.DatabaseManager;
using NRO_Server.Model.Character;
using NRO_Server.Model.Item;
using NRO_Server.Model.Map;
using NRO_Server.Model.Monster;
using NRO_Server.Model.Template;
using NRO_Server.Model;
using static System.GC;

namespace NRO_Server.Application.Handlers.Map
{
    public class ZoneHandler : IZoneHandler
    {
        public Zone Zone { get; set; }

        public ZoneHandler(Zone zone)
        {
            Zone = zone;
        }

        public void JoinZone(Model.Character.Character character, bool isDefault, bool isTeleport, int typeTeleport)
        {

            lock (Zone.Characters)
            {
                if(character == null || character.GetType() != typeof(Model.Character.Character)) return;
                character.CharacterHandler.SendMessage(Service.SendImageBag(character.Id, character.GetBag()));
                character.TypeTeleport = typeTeleport;
                character.InfoChar.MapId = Zone.Map.Id;
                character.InfoChar.ZoneId = Zone.Id;
                character.CharacterHandler.SendMessage(Service.MapClear());
                character.CharacterHandler.SendMessage(Service.SendStamina(character.InfoChar.Stamina));

                if (isDefault && !isTeleport)
                {
                    character.InfoChar.X = Zone.Map.TileMap.X0;
                    character.InfoChar.Y = Zone.Map.TileMap.Y0;
                } else if (isTeleport)
                {
                    character.InfoChar.X = (short)ServerUtils.RandomNumber(250,450);
                    character.InfoChar.Y = 0;
                }
                character.UpdateOldMap();

                if (Zone.Characters.TryAdd(character.Id, character))
                {
                    var disciple = character.Disciple;
                    var checkHaveDisciple = false;
                    if (disciple is {Status: < 3} && character.InfoChar.IsHavePet && !character.InfoChar.Fusion.IsFusion && !DataCache.IdMapCustom.Contains(Zone.Map.Id))
                    {
                        if (Zone.Disciples.TryAdd(disciple.Id, disciple))
                        {
                            checkHaveDisciple = true;
                            disciple.Zone = Zone;
                            disciple.CharacterHandler.SetUpPosition(isRandom:true);
                            disciple.PlusPoint.RandomPoint(disciple);
                        }
                    }

                    var pet = character.Pet;
                    var checkHavePet = false;
                    if (pet != null && !DataCache.IdMapCustom.Contains(Zone.Map.Id))
                    {
                        if (Zone.Pets.TryAdd(pet.Id, pet))
                        {
                            checkHavePet = true;
                            pet.Zone = Zone;
                            pet.CharacterHandler.SetUpPosition(isRandom:true);
                        }
                    }

                    if (character.InfoSkill.Egg.Monster != null)
                    {
                        if (Zone.MonsterPets.TryAdd(character.InfoSkill.Egg.Monster.IdMap, character.InfoSkill.Egg.Monster))
                        {
                            character.InfoSkill.Egg.Monster.Zone = Zone;
                            SendMessage(Service.UpdateMonsterMe0(character.InfoSkill.Egg.Monster));
                        }
                        else
                        {
                            SkillHandler.RemoveMonsterPet(character);
                        }
                    }
                    character.IsNextMap = true;
                    foreach (var @char in Zone.Characters.Values.Where(c => c.Id != character.Id))
                    {
                        @char.CharacterHandler.SendMessage(Service.PlayerAdd(character));
                        if(checkHaveDisciple) @char.CharacterHandler.SendMessage(Service.PlayerAdd(disciple, "#"));
                        if(checkHavePet) @char.CharacterHandler.SendMessage(Service.PlayerAdd(pet, "#"));
                    }

                    character.Zone = Zone;
                    //Map custom
                    if (DataCache.IdMapCustom.Contains(Zone.Map.Id))
                    {
                        character.InfoChar.MapCustomId = Zone.Map.MapCustom.Id;
                        Zone.Map.MapCustom?.Characters.Add(character);
                    }
                    
                    character.CharacterHandler.SendMessage(Service.MapInfo(Zone, character));
                    character.CharacterHandler.SetUpInfo();
                    character.CharacterHandler.SendMessage(Service.UpdateBody(character));
                    //UpdateBag
                    character.CharacterHandler.SendMessage(Service.PlayerLoadSpeed(character));
                    character.CharacterHandler.SendMessage(Service.ChangeFlag2(character.Flag));
                    UpdateCharacter(character);
                    //Plus Hp, Mp when teleport = 3 // end
                    if (!isTeleport || typeTeleport != 3) return;
                    character.InfoChar.Hp = character.HpFull;
                    character.InfoChar.Mp = character.MpFull;
                    character.CharacterHandler.SendMessage(Service.SendHp((int)character.InfoChar.Hp));
                    character.CharacterHandler.SendMessage(Service.SendMp((int)character.InfoChar.Mp));
                    SendMessage(Service.PlayerLevel(character));
                }
                else
                {
                    ClientManager.Gi().KickSession(character.Player.Session);
                }
            }
        }

        public void AddDisciple(Disciple disciple)
        {
            if (Zone.Map.IsMapCustom())
            {
                return;
            }
            if (!Zone.Disciples.TryAdd(disciple.Id, disciple)) return;
            disciple.Zone = Zone;
            lock (Zone.Characters)
            {
                foreach (var character in Zone.Characters.Values)
                {
                    var text = "#";
                    if (character.Id + disciple.Id == 0) text = "$";
                    character?.CharacterHandler.SendMessage(Service.PlayerAdd(disciple, text)); 
                }
            }
        }

        public void RemoveDisciple(Disciple disciple)
        {
            lock(Zone.Disciples)
            {
                try
                {
                    if (!Zone.Disciples.TryRemove(disciple.Id, out _)) return;
                    SendMessage(Service.PlayerRemove(disciple.Id));
                    if (disciple.InfoSkill.Egg.Monster == null) return;
                    RemoveMonsterMe(disciple.InfoSkill.Egg.Monster.IdMap);
                    SendMessage(Service.UpdateMonsterMe7(disciple.InfoSkill.Egg.Monster.IdMap));
                }
                catch (Exception e)
                {
                    Server.Gi().Logger.Error($"Error RemoveDisciple in ZoneHandler.cs {e.Message} \n {e.StackTrace}", e);
                }
            }
        }

        public void AddPet(Pet pet)
        {
            if (Zone.Map.IsMapCustom())
            {
                return;
            }
            if (!Zone.Pets.TryAdd(pet.Id, pet)) return;
            pet.Zone = Zone;
            lock (Zone.Characters)
            {
                foreach (var character in Zone.Characters.Values)
                {
                    var text = "#";
                    if (character.Id + pet.Id == 0) text = "$";
                    character?.CharacterHandler.SendMessage(Service.PlayerAdd(pet, text)); 
                }
            }
        }

        public void RemovePet(Pet pet)
        {
            lock(Zone.Pets)
            {
                try
                {
                    if (!Zone.Pets.TryRemove(pet.Id, out _)) return;
                    SendMessage(Service.PlayerRemove(pet.Id));
                }
                catch (Exception e)
                {
                    Server.Gi().Logger.Error($"Error RemovePet in ZoneHandler.cs {e.Message} \n {e.StackTrace}", e);
                }
            }
        }

        public void AddBoss(Boss boss)
        {
            if (Zone.Map.IsMapCustom())
            {
                return;
            }
            if (!Zone.Bosses.TryAdd(boss.Id, boss)) return;
            boss.Zone = Zone;
            boss.CharacterHandler.SetUpPosition(mapNew:Zone.Map.Id);
            lock (Zone.Characters)
            {
                foreach (var character in Zone.Characters.Values)
                {
                    character?.CharacterHandler.SendMessage(Service.PlayerAdd(boss)); 
                }
            }
        }

        public void RemoveBoss(Boss boss)
        {
            lock(Zone.Bosses)
            {
                try
                {
                    if (!Zone.Bosses.TryRemove(boss.Id, out _)) return;
                    SendMessage(Service.PlayerRemove(boss.Id));
                    if (boss.InfoSkill.Egg.Monster == null) return;
                    RemoveMonsterMe(boss.InfoSkill.Egg.Monster.IdMap);
                    SendMessage(Service.UpdateMonsterMe7(boss.InfoSkill.Egg.Monster.IdMap));
                }
                catch (Exception e)
                {
                    Server.Gi().Logger.Error($"Error RemoveBoss in ZoneHandler.cs: {e.Message} \n {e.StackTrace}", e);
                }
            }
        }

        private void UpdateCharacter(Model.Character.Character character)
        {
            var today = ServerUtils.TimeNow().Date;
            var loginDay = character.LastLogin.Date;
            int daydiff = (int)((today - loginDay).TotalDays);
            if (daydiff <= 0) return;
            character.LastLogin = ServerUtils.TimeNow();
            character.InfoChar.IsNhanBua = true;
            character.InfoChar.SoLanGiaoDich = 0; 
            character.InfoChar.ThoiGianGiaoDich = 0; 
            character.InfoChar.CountGoiRong = 0;
            character.InfoChar.TrainManhVo = 0;
            character.InfoChar.TrainManhHon = 0;
        }

        public void OutZone(ICharacter character, bool isOutZone = false)
        {
            
            if(character == null) return;
            if(Zone == null || Zone.Map == null) return;
            lock (Zone.Characters)
            {
                if(Zone.Map.IsMapCustom() && Zone.Map.MapCustom != null)
                {
                    Zone.Map.MapCustom.IsOutMap = isOutZone;
                    Zone.Map.MapCustom.RemoveCharacter(character);
                    character.InfoChar.MapCustomId = -1;
                }

                var @charReal= (Model.Character.Character)character;
                if (Zone.Characters.TryRemove(character.Id, out _))
                {
                    SendMessage(Service.PlayerRemove(character.Id), character.Id);
                    
                    if (character.InfoSkill.Egg.Monster != null)
                    {
                        RemoveMonsterMe(character.InfoSkill.Egg.Monster.IdMap);
                        SendMessage(Service.UpdateMonsterMe7(character.InfoSkill.Egg.Monster.IdMap));
                    }

                    var disciple = @charReal.Disciple;
                    if (disciple != null && (disciple.Status < 3 || disciple.InfoChar.IsDie) && character.InfoChar.IsHavePet && !character.InfoChar.Fusion.IsFusion)
                    {
                        if (Zone.Disciples.TryRemove(disciple.Id, out _))
                        {
                            disciple.MonsterFocus = null;
                            SendMessage(Service.PlayerRemove(disciple.Id), character.Id);
                            if (disciple.InfoSkill.Egg.Monster != null)
                            {
                                RemoveMonsterMe(disciple.InfoSkill.Egg.Monster.IdMap);
                                SendMessage(Service.UpdateMonsterMe7(disciple.InfoSkill.Egg.Monster.IdMap));
                            }
                        }
                    }

                    var pet = @charReal.Pet;
                    if (pet != null)
                    {
                        if (Zone.Pets.TryRemove(pet.Id, out _))
                        {
                            SendMessage(Service.PlayerRemove(pet.Id), character.Id);
                        }
                    }
                }

                if (@charReal.Trade.IsTrade)
                {
                    var charTemp = (Model.Character.Character) GetCharacter(@charReal.Trade.CharacterId);
                    if (charTemp != null && charTemp.Trade.CharacterId == character.Id)
                    {
                        charTemp.CloseTrade(true);
                        charTemp.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().CLOSE_TRADE));
                    }
                    @charReal.CloseTrade(false);
                }

                if (@charReal.Test is {IsTest: true})
                {
                    var playerReal = (Model.Character.Character)GetCharacter(charReal.Test.TestCharacterId);
                    if (playerReal != null && playerReal.Test.IsTest && playerReal.Test.TestCharacterId == character.Id)
                    {
                        var gold = playerReal.Test.GoldTest * 190 / 100;
                        playerReal.PlusGold(gold);
                        playerReal.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().WON_TEST));
                        charReal.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().LOST_TEST2));
                        playerReal.CharacterHandler.ClearTest();
                    }
                    charReal.CharacterHandler.ClearTest();
                }
            }
        }

        public void InitMob()
        {
            var tileMap = Zone.Map.TileMap;
            MonsterTemplate monsterTemplate;
            var i = 0;
            tileMap.MonsterMaps.ForEach(monster =>
            {
                monsterTemplate = Cache.Gi().MONSTER_TEMPLATES.FirstOrDefault(x => x.Id == monster.Id);
                if(monsterTemplate != null)
                {
                    Zone.MonsterMaps.Add(new MonsterMap()
                    {
                        IdMap = i++,
                        Id = monster.Id,
                        X = monster.X,
                        Y = monster.Y,
                        Status = 5,
                        Level = monster.Level,
                        LvBoss = 0,
                        IsBoss = false,
                        Zone = Zone,
                        OriginalHp = monsterTemplate.Hp,//GetMonsterHP(Zone.Map.Id, monster.Id, monsterTemplate),
                        LeaveItemType = monsterTemplate.LeaveItemType,
                    });
                }
            });
            Zone.MonsterMaps.ForEach(monster => monster.MonsterHandler.SetUpMonster());
        }

        public async Task Update()
        {
            // Console.WriteLine("Update Map " + Zone.Map.TileMap.Name + " Zone " + Zone.Id);
            await Task.WhenAll(UpdateMonsterMap(), UpdateMonsterPet(), UpdateItemMap(), UpdatePlayer(), UpdateDisciple(), UpdatePet(), UpdateBoss());
        }

        #region Update Zone
        private async Task UpdateMonsterMap()
        {
            try
            {
                if (Zone.MonsterMaps.Count > 0) 
                {
                    // Zone.MonsterMaps.RemoveAll(monster => monster == null || monster.MonsterHandler == null);

                    Parallel.ForEach(Zone.MonsterMaps, monster =>
                    {
                        monster.MonsterHandler.Update();
                    });
                }
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error UpdateMonsterMap in ZoneHandler.cs: {e.Message} \n {e.StackTrace}", e);
            }

            await Task.Delay(50);
        }
        
        private async Task UpdateMonsterPet()
        {
            try
            {
                Parallel.ForEach(Zone.MonsterPets, monster =>
                {
                    monster.Value.MonsterHandler.Update();
                }); 
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error UpdateMonsterPet in Service.cs: {e.Message} \n {e.StackTrace}", e);
            }
            await Task.Delay(50);
        }

        private async Task UpdateItemMap()
        {
            try
            {
                Parallel.ForEach(Zone.ItemMaps, item =>
                {
                    item.Value.ItemMapHandler.Update(Zone);
                });
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error UpdateItemMap in Service.cs: {e.Message} \n {e.StackTrace}", e);
            }
            await Task.Delay(50);
        }

        private async Task UpdatePlayer()
        {
            try
            {
                if (Zone.Characters.Count > 0)
                {
                    Parallel.ForEach(Zone.Characters, character =>
                    {
                        character.Value.CharacterHandler.Update();
                    });
                }
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error UpdatePlayer in Service.cs: {e.Message} \n {e.StackTrace}", e);
            }
            await Task.Delay(50);
        }
        
        private async Task UpdateDisciple()
        {
            try
            {
                if (Zone.Disciples.Count > 0) 
                {
                    Parallel.ForEach(Zone.Disciples, disciple =>
                    {
                        disciple.Value.CharacterHandler.Update();
                    });
                }
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error UpdateDisciple in Service.cs: {e.Message} \n {e.StackTrace}", e);
            }
            await Task.Delay(50);
        }

        private async Task UpdatePet()
        {
            try
            {
                if (Zone.Pets.Count > 0) 
                {
                    Parallel.ForEach(Zone.Pets, pet =>
                    {
                        pet.Value.CharacterHandler.Update();
                    });
                }
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error UpdatePet in Service.cs: {e.Message} \n {e.StackTrace}", e);
            }
            await Task.Delay(50);
        }

        private async Task UpdateBoss()
        {
            try
            {
                if (Zone.Bosses.Count > 0)
                {
                    Parallel.ForEach(Zone.Bosses, boss =>
                    {
                        boss.Value.CharacterHandler.Update();
                    });
                }
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error UpdateBoss in ZoneHandler.cs: {e.Message} \n {e.StackTrace}", e);
            }
            await Task.Delay(50);
        }

        #endregion

        public void Close()
        {
            Zone.ItemMaps.Clear();
            Zone.MonsterPets.Clear();
            Zone.MonsterMaps.Clear();
            Zone.Bosses.Clear();
            Zone.Disciples.Clear();
            Zone.Pets.Clear();
            Zone.Map = null;
            SuppressFinalize(this);
        }

        public void SendMessage(Message message, bool isSkillMessage = false)
        {
            lock (Zone.Characters)
            {
                foreach (var character in Zone.Characters.Values)
                {
                    if (isSkillMessage && !character.InfoChar.HieuUngDonDanh) continue;
                    character?.CharacterHandler.SendMessage(message);
                }
            }
        }
        public void SendMessage(Message message, int id)
        {
            lock (Zone.Characters)
            {
                foreach (var character in Zone.Characters.Values)
                {
                    character?.CharacterHandler.SendMessage(message);
                }
            }
        }

        public void LeaveItemMap(ItemMap itemMap)
        {
            if(itemMap == null) return;
            try
            {
                lock (Zone.ItemMaps)
                {
                    if(itemMap?.Item == null) return;
                    if (Zone.ItemMaps.Count > 500) RemoveItemMap(0);
                    itemMap.Id = GetItemMapNotId();
                    itemMap.X = (short)ServerUtils.RandomNumber(itemMap.X - 15, itemMap.X + 15);
                    itemMap.Y = Zone.Map.TileMap.TouchY(itemMap.X, itemMap.Y);
                    if (Zone.ItemMaps.TryAdd(itemMap.Id, itemMap)) SendMessage(Service.ItemMapAdd(itemMap));
                }
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error LeaveItemMap in ZoneHandler.cs: {e.Message} \n {e.StackTrace}", e);
            }
        }

        public void LeaveItemMap(ItemMap itemMap, MonsterMap monster)
        {
            if(itemMap == null) return;
            try
            {
                lock (Zone.ItemMaps)
                {
                    if(itemMap?.Item == null) return;
                    if (Zone.ItemMaps.Count > 500) RemoveItemMap(0);
                    itemMap.Id = GetItemMapNotId();
                    itemMap.X = (short)ServerUtils.RandomNumber(itemMap.X - 30, itemMap.X + 30);
                    itemMap.Y = Zone.Map.TileMap.TouchY(itemMap.X, itemMap.Y);
                    if (Zone.ItemMaps.TryAdd(itemMap.Id, itemMap))
                    {
                        if (Cache.Gi().MONSTER_TEMPLATES[monster.Id].Type == 4)
                        {
                            SendMessage(Service.MonsterFlyLeaveItem(monster.IdMap, itemMap));
                        }
                        else
                        {
                            SendMessage(Service.ItemMapAdd(itemMap));
                        }   
                    }
                }
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error LeaveItemMap in ZoneHandler.cs: {e.Message} \n {e.StackTrace}", e);
            }
        }

        public void RemoveItemMap(int id)
        {
            lock (Zone.ItemMaps)
            {
                if (Zone.ItemMaps.TryRemove(id, out var it))
                {
                    SendMessage(Service.ItemMapRemove(id));
                    it.Dispose();
                }
            }
        }

        public int GetItemMapNotId()
        {
            if (Zone.ItemMapId > 500) Zone.ItemMapId = 0;
            return Zone.ItemMapId++;
        }

        public void RemoveCharacter()
        {
            lock (Zone.Characters)
            {
                //Remove character
            }
        }

        public void RemoveMonsterMe(int id)
        {
            lock (Zone.MonsterPets)
            {
                Zone.MonsterPets.TryRemove(id, out _);
            }
        }

        public bool AddMonsterPet(MonsterPet monsterPet)
        {
            lock (Zone.MonsterPets)
            {
                return Zone.MonsterPets.TryAdd(monsterPet.IdMap, monsterPet);
            }
        }

        public ICharacter GetCharacter(int id)
        {
            return GetCharacterKeyValue(id).Value;
        }

        public ICharacter GetDisciple(int id)
        {
            return GetDiscipleKeyValue(id).Value;
        }

        public ICharacter GetPet(int id)
        {
            return GetPetKeyValue(id).Value;
        }

        public ICharacter GetBoss(int id)
        {
            return GetBossKeyValue(id).Value;
        }

        public MonsterMap GetMonsterMap(int id)
        {
            lock (Zone.MonsterMaps)
            {
                return Zone.MonsterMaps.FirstOrDefault(m => m.IdMap == id);
            }
        }

        public MonsterPet GetMonsterPet(int id)
        {
            lock (Zone.MonsterPets)
            {
                return Zone.MonsterPets.GetValueOrDefault((short)id);
            }
        }

        public KeyValuePair<int, Model.Character.Character> GetCharacterKeyValue(int id)
        {
            lock (Zone.Characters)
            {
                return Zone.Characters.FirstOrDefault(c => c.Key == id);
            }
        }

        public KeyValuePair<int, Disciple> GetDiscipleKeyValue(int id)
        {
            lock (Zone.Disciples)
            {
                return Zone.Disciples.FirstOrDefault(c => c.Key == id);
            }
        }

        public KeyValuePair<int, Pet> GetPetKeyValue(int id)
        {
            lock (Zone.Pets)
            {
                return Zone.Pets.FirstOrDefault(c => c.Key == id);
            }
        }

        public KeyValuePair<int, Boss> GetBossKeyValue(int id)
        {
            lock (Zone.Bosses)
            {
                return Zone.Bosses.FirstOrDefault(c => c.Key == id);
            }
        }
    }
}