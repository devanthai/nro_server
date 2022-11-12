using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using Newtonsoft.Json;
using NRO_Server.Application.Constants;
using NRO_Server.Application.IO;
using NRO_Server.Application.Threading;
using NRO_Server.Model.BangXepHang;
using NRO_Server.Model.Character;
using NRO_Server.Model.Info;
using NRO_Server.Model.Item;
using NRO_Server.Model.SkillCharacter;

namespace NRO_Server.DatabaseManager.Player
{
    public class CharacterDB
    {
        public static bool IsAlreadyExist(string nameCheck)
        {
            lock (Server.SQLLOCK)
            {
                try
                {
                    DbContext.gI()?.ConnectToAccount();
                    using DbCommand command = DbContext.gI()?.Connection.CreateCommand();
                    if (command == null) return false;
                    command.CommandText = $"SELECT `id` FROM `character` WHERE `name` = '{nameCheck}'";
                    using var reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        return true;
                    }
                    return false;
                }
                catch (Exception e)
                {
                    Server.Gi().Logger.Error($"Check Already Exist Name Character: {e.Message}\n{e.StackTrace}");
                    return false;
                }
                finally
                {
                    DbContext.gI()?.CloseConnect(); 
                }
                
            }
        }
        
        public static int Create(Character character)
        {
            lock (Server.SQLLOCK)
            {
                try
                {
                    DbContext.gI()?.ConnectToAccount();
                    using DbCommand command = DbContext.gI()?.Connection.CreateCommand();
                    if (command != null)
                    {
                        var createDate = ServerUtils.TimeNow();
                        command.CommandText =
                            $"INSERT INTO `character` (`Name`, `Skills`, `ItemBody`, `ItemBag`, `ItemBox`, `InfoChar`, `BoughtSkill`, `LuckyBox`, `LastLogin`, `CreateDate`, `SpecialSkill`, `InfoBuff`) VALUES ('{character.Name}', '{JsonConvert.SerializeObject(character.Skills)}', '{JsonConvert.SerializeObject(character.ItemBody)}' , '{JsonConvert.SerializeObject(character.ItemBag)}', '{JsonConvert.SerializeObject(character.ItemBox)}', '{JsonConvert.SerializeObject(character.InfoChar)}', '{JsonConvert.SerializeObject(character.BoughtSkill)}', '{JsonConvert.SerializeObject(character.LuckyBox)}', '{character.LastLogin:yyyy-MM-dd HH:mm:ss}' , '{createDate:yyyy-MM-dd HH:mm:ss}', '{JsonConvert.SerializeObject(character.SpecialSkill)}', '{JsonConvert.SerializeObject(character.InfoBuff)}'); SELECT LAST_INSERT_ID();";
                        var reader = int.Parse(command.ExecuteScalar()?.ToString() ?? "0");
                        return reader;
                    }
                }
                catch (Exception e)
                {
                    Server.Gi().Logger.Error($"Create new character error: {e.Message}\n{e.StackTrace}");
                    return 0;
                }
                finally
                {
                    DbContext.gI()?.CloseConnect();
                }
                return 0;
            }
        }

        public static void Delete(int idChar)
        {
            lock (Server.SQLLOCK)
            {
                try
                {
                    DbContext.gI()?.ConnectToAccount();
                    using DbCommand command = DbContext.gI()?.Connection.CreateCommand();
                    if (command == null) return;
                    command.CommandText =
                        $"DELETE FROM `character` WHERE `id` = {idChar};";
                    command.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    Server.Gi().Logger.Error($"Create new character error: {e.Message}\n{e.StackTrace}");
                }
                finally
                {
                    DbContext.gI()?.CloseConnect();
                }
            }
        }

        public static bool SaveInventory(Character character, bool saveBody = false, bool saveBox = false, bool saveLuckyBox = false)
        {
            lock (Server.SQLLOCK)
            {
                try
                {

                    var text = $"`ItemBag` = '{JsonConvert.SerializeObject(character.ItemBag)}'";
                    character.Delay.NeedToSaveBag = false;
                    if (saveBody)
                    {
                        text += $", `ItemBody` = '{JsonConvert.SerializeObject(character.ItemBody)}'";
                        character.Delay.NeedToSaveBody = false;
                    }

                    if (saveBox)
                    {
                        text += $", `ItemBox` = '{JsonConvert.SerializeObject(character.ItemBox)}'";
                        character.Delay.NeedToSaveBox = false;
                    }

                    // if (saveLuckyBox)
                    // {
                    //     text += $", `LuckyBox` = '{JsonConvert.SerializeObject(character.LuckyBox)}'";
                    //     character.Delay.NeedToSaveLucky = false;
                    // }

                    DbContext.gI()?.ConnectToAccount();
                    using DbCommand command = DbContext.gI()?.Connection.CreateCommand();
                    if (command == null) return false;
                    command.CommandText = $"UPDATE `character` SET {text} WHERE `id` = {character.Id};";
                    command.ExecuteNonQuery();
                    return true;
                }
                catch (Exception e)
                {
                    Server.Gi().Logger.Error($"Update SaveInventory error: {e.Message}\n{e.StackTrace}");
                    return false;
                }
                finally
                {
                    DbContext.gI()?.CloseConnect();
                }
            }
        }

        public static void Update(Character character)
        {
            lock (Server.SQLLOCK)
            {
                try
                {
                    if (character.InfoChar.Fusion.TimeUse > 0)
                    {
                        character.InfoChar.Fusion.TimeUse = (int)(ServerUtils.CurrentTimeMillis() - character.InfoChar.Fusion.TimeStart);
                    }
                    var text = $"`name` = '{character.Name}'";
                    text += $", `Skills` = '{JsonConvert.SerializeObject(character.Skills)}'";
                    
                    if (character.Delay.StartLogout)
                    {
                        text += $", `ItemBody` = '{JsonConvert.SerializeObject(character.ItemBody)}'";
                        text += $", `ItemBag` = '{JsonConvert.SerializeObject(character.ItemBag)}'";
                        text += $", `ItemBox` = '{JsonConvert.SerializeObject(character.ItemBox)}'";
                    }
                    text += $", `InfoChar` = '{JsonConvert.SerializeObject(character.InfoChar)}'";
                    text += $", `BoughtSkill` = '{JsonConvert.SerializeObject(character.BoughtSkill)}'";
                    text += $", `PlusBag` = {character.PlusBag}";
                    text += $", `PlusBox` = {character.PlusBox}";
                    text += $", `Friends` = '{JsonConvert.SerializeObject(character.Friends)}'";
                    text += $", `Enemies` = '{JsonConvert.SerializeObject(character.Enemies)}'";
                    text += $", `Me` = '{JsonConvert.SerializeObject(character.Me)}'";
                    text += $", `ClanId` = {character.ClanId}";
                    text += $", `LuckyBox` = '{JsonConvert.SerializeObject(character.LuckyBox)}'";
                    text += $", `LastLogin` = '{character.LastLogin:yyyy-MM-dd HH:mm:ss}'";
                    text += $", `SpecialSkill` = '{JsonConvert.SerializeObject(character.SpecialSkill)}'";
                    text += $", `InfoBuff` = '{JsonConvert.SerializeObject(character.InfoBuff)}'";
                    text += $", `diemsukien` = '{JsonConvert.SerializeObject(character.DiemSuKien)}'";
                    DbContext.gI()?.ConnectToAccount();
                    using DbCommand command = DbContext.gI()?.Connection.CreateCommand();
                    if (command == null) return;
                    command.CommandText = $"UPDATE `character` SET {text}  WHERE `id` = {character.Id};";
                    command.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    Server.Gi().Logger.Error($"Update character error: {e.Message}\n{e.StackTrace}");
                }
                finally
                {
                    DbContext.gI()?.CloseConnect();
                }
            }
        }

        public static void Update(int charId)
        {
            lock (Server.SQLLOCK)
            {
                try
                {
                    var text = $"`ClanId` = -1";
                    DbContext.gI()?.ConnectToAccount();
                    using DbCommand command = DbContext.gI()?.Connection.CreateCommand();
                    if (command == null) return;
                    command.CommandText = $"UPDATE `character` SET {text}  WHERE `id` = {charId};";
                    command.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    Server.Gi().Logger.Error($"Update character error: {e.Message}\n{e.StackTrace}");
                }
                finally
                {
                    DbContext.gI()?.CloseConnect();
                }
            }
        }

        public Character GetByName(string name)
        {
            return null;
        }
        
        public static Character GetById(int id)
        {
            lock (Server.SQLLOCK)
            {
                try
                {
                    DbContext.gI()?.ConnectToAccount();
                    using DbCommand command = DbContext.gI()?.Connection.CreateCommand();
                    if (command == null) return null;
                    command.CommandText =
                        $"SELECT * FROM `character` WHERE `id` = {id};";
                    using var reader = command.ExecuteReader();
                    if (!reader.HasRows)
                    {
                        return null;
                    }
                    while (reader.Read())
                    {
                        var character = new Character
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1)
                        };
                        character.Skills.AddRange(
                            JsonConvert.DeserializeObject<List<SkillCharacter>>(reader.GetString(2)));
                        character.ItemBody =
                            JsonConvert.DeserializeObject<List<Item>>(reader.GetString(3),
                                DataCache.SettingNull);
                        character.ItemBag = JsonConvert.DeserializeObject<List<Item>>(reader.GetString(4));
                        character.ItemBox = JsonConvert.DeserializeObject<List<Item>>(reader.GetString(5));
                        character.InfoChar =
                            JsonConvert.DeserializeObject<InfoChar>(reader.GetString(6),
                                DataCache.SettingNull);
                        character.BoughtSkill.AddRange(
                            JsonConvert.DeserializeObject<List<int>>(reader.GetString(7)));
                        character.PlusBag = reader.GetInt32(8);
                        character.PlusBox = reader.GetInt32(9);
                        character.Friends = JsonConvert.DeserializeObject<List<InfoFriend>>(reader.GetString(10)) ?? new List<InfoFriend>();
                        character.Enemies = JsonConvert.DeserializeObject<List<InfoFriend>>(reader.GetString(11)) ?? new List<InfoFriend>();
                        //12
                        character.ClanId = reader.GetInt32(13);
                        character.LuckyBox = JsonConvert.DeserializeObject<List<Item>>(reader.GetString(14)) ?? new List<Item>();
                        character.LastLogin = reader.GetDateTime(15);
                        // character.CharacterHandler.SetUpInfo();
                        character.SpecialSkill = JsonConvert.DeserializeObject<SpecialSkill>(reader.GetString(17),
                                DataCache.SettingNull);

                        character.InfoBuff = JsonConvert.DeserializeObject<InfoBuff>(reader.GetString(18),
                                DataCache.SettingNull);
                        character.DiemSuKien = reader.GetInt32(19);
                        character.SpecialSkill.ClearTemp();
                        return character;
                    }
                    return null;
                }
                catch (Exception e)
                {
                    Server.Gi().Logger.Error($"GetById character error: {e.Message}\n{e.StackTrace}");
                    return null;
                }
                finally
                {
                    DbContext.gI()?.CloseConnect();
                }
            }
        }

        public static InfoChar GetInfoCharById(int id)
        {
            lock (Server.SQLLOCK)
            {
                try
                {
                    DbContext.gI()?.ConnectToAccount();
                    using DbCommand command = DbContext.gI()?.Connection.CreateCommand();
                    if (command == null) return null;
                    command.CommandText =
                        $"SELECT * FROM `character` WHERE `id` = {id};";
                    using var reader = command.ExecuteReader();
                    if (!reader.HasRows) return null;
                    while (reader.Read())
                    {
                        return JsonConvert.DeserializeObject<InfoChar>(reader.GetString(6),
                            DataCache.SettingNull);
                    }

                    return null;
                }
                catch (Exception e)
                {
                    Server.Gi().Logger.Error($"Get info character error: {e.Message}\n{e.StackTrace}");
                    return null;
                }
                finally
                {
                    DbContext.gI()?.CloseConnect();
                }
            }
        }

        public static void Update(InfoChar info, int id)
        {
            lock (Server.SQLLOCK)
            {
                try
                {
                    var text = $"`InfoChar` = '{JsonConvert.SerializeObject(info)}'";
                    DbContext.gI()?.ConnectToAccount();
                    using DbCommand command = DbContext.gI()?.Connection.CreateCommand();
                    if (command == null) return;
                    command.CommandText = $"UPDATE `character` SET {text}  WHERE `id` = {id};";
                    command.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    Server.Gi().Logger.Error($"Update character Infocharr error: {e.Message}\n{e.StackTrace}");
                }
                finally
                {
                    DbContext.gI()?.CloseConnect();
                }
            }
        }

        public static InfoFriend GetInfoCharacter(int id)
        {
            lock (Server.SQLLOCK)
            {
                try
                {
                    DbContext.gI()?.ConnectToAccount();
                    using DbCommand command = DbContext.gI()?.Connection.CreateCommand();
                    if (command == null) return null;
                    command.CommandText =
                        $"SELECT * FROM `character` WHERE `id` = {id};";
                    using var reader = command.ExecuteReader();
                    if (!reader.HasRows) return null;
                    while (reader.Read())
                    {
                        return JsonConvert.DeserializeObject<InfoFriend>(reader.GetString(12));
                    }
                    return null;
                }
                catch (Exception e)
                {
                    Server.Gi().Logger.Error($"Create new character error: {e.Message}\n{e.StackTrace}");
                    return null;
                }
                finally
                {
                    DbContext.gI()?.CloseConnect();
                }
            }
        }
        
        public static void SelectBXHSuKien(int limit)
        {
            lock (Server.SQLLOCK)
            {
                try
                {
                    DbContext.gI()?.ConnectToAccount();
                    var bxh = Server.Gi().BangXepHang;
                    using DbCommand command = DbContext.gI()?.Connection.CreateCommand();
                    if (command == null) return;
                    command.CommandText = $"SELECT `Name`, JSON_EXTRACT(InfoChar, '$.Power') AS CharPower FROM `character` WHERE (JSON_EXTRACT(InfoChar, '$.Power') > 0) ORDER BY CharPower DESC LIMIT {limit};";
                    // command.CommandText = $"SELECT `Name`,`diemsukien` FROM `character` WHERE (`diemsukien` > 0) ORDER BY `diemsukien` DESC LIMIT {limit};";
                    using var reader = command.ExecuteReader();
                    if (!reader.HasRows) return;
                    int i = 1;
                    while (reader.Read())
                    {
                        bxh.Players.Add(new BangXepHang()
                        {
                            I = i,
                            Name =  reader.GetString(0),
                            Diem = reader.GetInt64(1),
                        });
                        i++;
                    }
                }
                catch (Exception e)
                {
                    Server.Gi().Logger.Error($"Error bxh {e.Message}\n{e.StackTrace}");
                }
                finally
                {
                    DbContext.gI()?.CloseConnect();
                }
            }
        }
    }
}