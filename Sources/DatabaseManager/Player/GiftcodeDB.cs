using System;
using System.Data.Common;
using Newtonsoft.Json;
using NRO_Server.Application.Threading;
using NRO_Server.Model.Character;

namespace NRO_Server.DatabaseManager.Player
{
    public class GiftcodeDB
    {
        public static int CheckCodeValidType(string code)
        {
            lock (Server.SQLLOCK)
            {
                try
                {
                    DbContext.gI()?.ConnectToAccount();
                    using DbCommand command = DbContext.gI()?.Connection.CreateCommand();
                    if (command == null) return -1;
                    command.CommandText = $"SELECT `type` FROM `giftcode` WHERE `code` = '{code}' AND count > 0 AND time_expire >= CURRENT_TIMESTAMP";
                    using var reader = command.ExecuteReader();
                    if (reader.HasRows && reader.Read())
                    {
                        return reader.GetInt32(0);
                    }
                    return -1;
                }
                catch (Exception e)
                {
                    Server.Gi().Logger.Error($"Check CheckCodeValid: {e.Message}\n{e.StackTrace}");
                    return -1;
                }
                finally
                {
                    DbContext.gI()?.CloseConnect(); 
                }
                
            }
        }

        public static bool CheckCharacterAlreadyUsedCode(string code, string character, int codeType)
        {
            lock (Server.SQLLOCK)
            {
                try
                {
                    DbContext.gI()?.ConnectToAccount();
                    using DbCommand command = DbContext.gI()?.Connection.CreateCommand();
                    if (command == null) return false;
                    command.CommandText = $"SELECT `code` FROM `giftcode_used` WHERE `type` = '{codeType}' AND `character`= '{character}'";
                    using var reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        return true;
                    }
                    return false;
                }
                catch (Exception e)
                {
                    Server.Gi().Logger.Error($"CheckCharacterAlreadyUsedCode: {e.Message}\n{e.StackTrace}");
                    return false;
                }
                finally
                {
                    DbContext.gI()?.CloseConnect(); 
                }
                
            }
        }

        public static bool UsedCode(string code, string character, int codeType)
        {
            lock (Server.SQLLOCK)
            {
                try
                {
                    DbContext.gI()?.ConnectToAccount();
                    using DbCommand command = DbContext.gI()?.Connection.CreateCommand();
                    if (command == null) return false;
                    command.CommandText =
                        $"UPDATE `giftcode` SET count=count-1 WHERE `code`='{code}'";
                    command.ExecuteNonQuery();
                    // 
                    using DbCommand command2 = DbContext.gI()?.Connection.CreateCommand();
                    if (command2 == null) return false;
                    command2.CommandText =
                        $"INSERT INTO `giftcode_used` (`code`, `character`, `time_used`, `type`) VALUES ('{code}', '{character}', CURRENT_TIMESTAMP, {codeType})";

                    var reader = int.Parse(command2.ExecuteScalar()?.ToString() ?? "0");
                    return reader == 0;
                }
                catch (Exception e)
                {
                    throw new Exception($"UsedCode error: {e.Message}\n{e.StackTrace}");
                }
                finally
                {
                    DbContext.gI()?.CloseConnect();
                }
            }
        }
    }
}