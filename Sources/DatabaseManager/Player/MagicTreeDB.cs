using System;
using System.Data.Common;
using Newtonsoft.Json;
using NRO_Server.Application.Threading;
using NRO_Server.Model.Character;

namespace NRO_Server.DatabaseManager.Player
{
    public class MagicTreeDB
    {
        public static void Create(MagicTree @magicTree)
        {
            lock (Server.SQLLOCK)
            {
                try
                {
                    DbContext.gI()?.ConnectToAccount();
                    using DbCommand command = DbContext.gI()?.Connection.CreateCommand();
                    if (command == null) return;
                    command.CommandText =
                        $"INSERT INTO `magictree` (`Id`, `idNpc`, `X`, `Y`) VALUES ('{magicTree.Id}', '{magicTree.NpcId}', '{magicTree.X}' , '{magicTree.Y}');";
                    command.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    throw new Exception($"Create new Magic Tree error: {e.Message}\n{e.StackTrace}");
                }
                finally
                {
                    DbContext.gI()?.CloseConnect();
                }
            }
        }

        public static void Update(MagicTree magicTree)
        {
            lock (Server.SQLLOCK)
            {
                try
                {
                    var isUpdate = magicTree.IsUpdate ? 1 : 0; 
                    var text = $"`level` = '{magicTree.Level}'";
                    text += $", `peas` = '{magicTree.Peas}'";
                    text += $", `idNpc` = '{magicTree.NpcId}'";
                    text += $", `maxPea` = '{magicTree.MaxPea}'";
                    text += $", `seconds` = '{magicTree.Seconds}'";
                    text += $", `isUpdating` = '{isUpdate}'";
                    text += $", `Diamond` = '{magicTree.Diamond}'";
                    DbContext.gI()?.ConnectToAccount();
                    using DbCommand command = DbContext.gI()?.Connection.CreateCommand();
                    if (command == null) return;
                    command.CommandText = $"UPDATE `magictree` SET {text}  WHERE `id` = {magicTree.Id};";
                    command.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    Server.Gi().Logger.Error($"Update magic tree error: {e.Message}\n{e.StackTrace}");
                }
                finally
                {
                    DbContext.gI()?.CloseConnect();
                }
            }
        }
    }
}