using System;
using System.Data.Common;
using Newtonsoft.Json;
using NRO_Server.Application.Threading;
using NRO_Server.Model.Character;

namespace NRO_Server.DatabaseManager.Player
{
    public class GachTheDB
    {
        public static bool IsAlreadyExist(string sign)
        {
            lock (Server.SQLLOCK)
            {
                try
                {
                    DbContext.gI()?.ConnectToAccount();
                    using DbCommand command = DbContext.gI()?.Connection.CreateCommand();
                    if (command == null) return false;
                    command.CommandText = $"SELECT `callback_sign` FROM `napthe` WHERE `callback_sign` = '{sign}'";
                    using var reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        return true;
                    }
                    return false;
                }
                catch (Exception e)
                {
                    Server.Gi().Logger.Error($"Check Already Exist Sign Nap The: {e.Message}\n{e.StackTrace}");
                    return false;
                }
                finally
                {
                    DbContext.gI()?.CloseConnect(); 
                }
                
            }
        }

        public static void Create(string sign, string requestId, string telCo, string soSeri, string maPin, long menhGiaThe, int responseCode)
        {
            lock (Server.SQLLOCK)
            {
                try
                {
                    DbContext.gI()?.ConnectToAccount();
                    using DbCommand command = DbContext.gI()?.Connection.CreateCommand();
                    if (command == null) return;
                    command.CommandText =
                        $"INSERT INTO `napthe` (`callback_sign`, `status`, `request_id`, `telco`, `serial`, `code`, `declared_value`, `response_code`) VALUES ('{sign}', 0, '{requestId}', '{telCo}', '{soSeri}', '{maPin}', '{menhGiaThe}', '{responseCode}');";
                    command.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    throw new Exception($"Create new GachTheDB error: {e.Message}\n{e.StackTrace}");
                }
                finally
                {
                    DbContext.gI()?.CloseConnect();
                }
            }
        }
    }
}