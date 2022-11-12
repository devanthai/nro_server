using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NRO_Server.Application.Constants;
using NRO_Server.Application.IO;
using NRO_Server.Application.Threading;
using NRO_Server.Model.Character;
using NRO_Server.Model.Info;
using NRO_Server.Model.Item;
using NRO_Server.Model.SkillCharacter;

namespace NRO_Server.DatabaseManager.Player
{
    public static class DiscipleDB
    {
        public static bool IsAlreadyExist(int discipleId)
        {
            lock (Server.SQLLOCK)
            {
                try
                {
                    DbContext.gI()?.ConnectToAccount();
                    using DbCommand command = DbContext.gI()?.Connection.CreateCommand();
                    if (command == null) return false;
                    command.CommandText = $"SELECT `id` FROM `disciple` WHERE `id` = '{discipleId}'";
                    using var reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        return true;
                    }
                    return false;
                }
                catch (Exception e)
                {
                    Server.Gi().Logger.Error($"Check Already Exist ID Disciple: {e.Message}\n{e.StackTrace}");
                    return false;
                }
                finally
                {
                    DbContext.gI()?.CloseConnect(); 
                }
                
            }
        }

        public static bool Create(Disciple disciple)
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
                            $"INSERT INTO `disciple` (`id`, `Name`, `Status`, `Skills`, `ItemBody`, `InfoChar`, `CreateDate`, `Type`) VALUES ({disciple.Id}, '{disciple.Name}', {disciple.Status}, '{JsonConvert.SerializeObject(disciple.Skills)}', '{JsonConvert.SerializeObject(disciple.ItemBody)}' , '{JsonConvert.SerializeObject(disciple.InfoChar)}', '{createDate:yyyy-MM-dd HH:mm:ss}', '{disciple.Type}'); SELECT LAST_INSERT_ID();";
                        var reader = int.Parse(command.ExecuteScalar()?.ToString() ?? "0");
                        return reader == 0;
                    }
                }
                catch (Exception e)
                {
                    Server.Gi().Logger.Error($"Create new Disciple error: {e.Message}\n{e.StackTrace}");
                    return false;
                }
                finally
                {
                    DbContext.gI()?.CloseConnect();
                }
                return false;
            }
        }

        public static void SaveInventory(Disciple disciple)
        {
            lock (Server.SQLLOCK)
            {
                try
                {
                    var text = $"`ItemBody` = '{JsonConvert.SerializeObject(disciple.ItemBody)}'";
                    DbContext.gI()?.ConnectToAccount();
                    using DbCommand command = DbContext.gI()?.Connection.CreateCommand();
                    if (command == null) return;
                    command.CommandText = $"UPDATE `disciple` SET {text}  WHERE `id` = {disciple.Id};";
                    command.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    Server.Gi().Logger.Error($"Update disciple error: {e.Message}\n{e.StackTrace}");
                }
                finally
                {
                    DbContext.gI()?.CloseConnect();
                }
            }
        }

        public static void Update(Disciple disciple)
        {
            lock (Server.SQLLOCK)
            {
                try
                {
                    var text = $"`name` = '{disciple.Name}'";
                    text += $", `Status` = '{disciple.Status}'";
                    text += $", `ItemBody` = '{JsonConvert.SerializeObject(disciple.ItemBody)}'";
                    text += $", `Skills` = '{JsonConvert.SerializeObject(disciple.Skills)}'";
                    text += $", `InfoChar` = '{JsonConvert.SerializeObject(disciple.InfoChar)}'";
                    text += $", `Type` = '{disciple.Type}'";
                    DbContext.gI()?.ConnectToAccount();
                    using DbCommand command = DbContext.gI()?.Connection.CreateCommand();
                    if (command == null) return;
                    command.CommandText = $"UPDATE `disciple` SET {text}  WHERE `id` = {disciple.Id};";
                    command.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    Server.Gi().Logger.Error($"Update disciple error: {e.Message}\n{e.StackTrace}");
                }
                finally
                {
                    DbContext.gI()?.CloseConnect();
                }
            }
        }


        public static Disciple GetById(int id)
        {
            lock (Server.SQLLOCK)
            {
                try
                {
                    DbContext.gI()?.ConnectToAccount();
                    using DbCommand command = DbContext.gI()?.Connection.CreateCommand();
                    if (command == null) return null;
                    command.CommandText =
                        $"SELECT * FROM `disciple` WHERE `id` = {id};";
                    using var reader = command.ExecuteReader();
                    if (!reader.HasRows)
                    {
                        return null;
                    }
                    while (reader.Read())
                    {
                        var disciple = new Disciple()
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Status = reader.GetInt32(2),
                        };
                        disciple.Skills.AddRange(
                            JsonConvert.DeserializeObject<List<SkillCharacter>>(reader.GetString(3))!);
                        disciple.ItemBody =
                            JsonConvert.DeserializeObject<List<Item>>(reader.GetString(4),
                                DataCache.SettingNull);
                        disciple.InfoChar =
                            JsonConvert.DeserializeObject<InfoChar>(reader.GetString(5),
                                DataCache.SettingNull);
                        disciple.Type = reader.GetInt32(7);
                        disciple.CharacterHandler.SetUpInfo();
                        return disciple;
                    }
                    return null;
                }
                catch (Exception e)
                {
                    Server.Gi().Logger.Error($"GetById disciple error: {e.Message}\n{e.StackTrace}");
                    return null;
                }
                finally
                {
                    DbContext.gI()?.CloseConnect();
                }
            }
        }

        public static bool CheckName(string name)
        {
            lock (Server.SQLLOCK)
            {
                try
                {
                    DbContext.gI()?.ConnectToAccount();
                    using DbCommand command = DbContext.gI()?.Connection.CreateCommand();
                    if (command == null) return false;
                    command.CommandText =
                        $"SELECT * FROM `disciple` WHERE `name` = {name};";
                    using var reader = command.ExecuteReader();
                    if (!reader.HasRows)
                    {
                        return false;
                    }
                    return true;
                }
                catch (Exception e)
                {
                    Server.Gi().Logger.Error($"GetByName disciple error: {e.Message}\n{e.StackTrace}");
                    return false;
                }
                finally
                {
                    DbContext.gI()?.CloseConnect();
                }
            }
        }

        public static bool Delete(int id)
        {
            try
            {
                DbContext.gI()?.ConnectToAccount();
                using DbCommand command = DbContext.gI()?.Connection.CreateCommand();
                if (command == null) return false;
                command.CommandText = $"DELETE FROM `disciple` WHERE `id` = {id};";
                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Create new character error: {e.Message}\n{e.StackTrace}");
                return false;
            }
            finally
            {
                DbContext.gI()?.CloseConnect();
            }
            return false; 
        }
    }
}