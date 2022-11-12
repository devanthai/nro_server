using System;
using System.Data.Common;
using Newtonsoft.Json;
using NRO_Server.Application.Threading;
using NRO_Server.Model.Clan;

namespace NRO_Server.DatabaseManager.Player
{
    public class ClanDB
    {
        public static int Create(Clan clan)
        {
            lock (Server.SQLLOCK)
            {
                try
                {
                    DbContext.gI()?.ConnectToAccount();
                    using DbCommand command = DbContext.gI()?.Connection.CreateCommand();
                    if (command != null)
                    {
                        command.CommandText =
                            $"INSERT INTO `clan` (`Name`, `Slogan`, `ImgId`, `Power`, `LeaderName`, `CurrMember`, `MaxMember`, `Date`, `Members`, `Messages`, `CharacterPeas`) VALUES ('{clan.Name}', '{clan.Slogan}', {clan.ImgId}, {clan.Power}, '{clan.LeaderName}', {clan.CurrMember}, {clan.MaxMember}, {clan.Date}, '{JsonConvert.SerializeObject(clan.Members)}', '{JsonConvert.SerializeObject(clan.Messages)}' , '{JsonConvert.SerializeObject(clan.CharacterPeas)}'); SELECT LAST_INSERT_ID();";
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

        public static void Delete(int clanId)
        {
            lock (Server.SQLLOCK)
            {
                try
                {
                    DbContext.gI()?.ConnectToAccount();
                    using DbCommand command = DbContext.gI()?.Connection.CreateCommand();
                    if (command == null) return;
                    command.CommandText =
                        $"DELETE FROM `clan` WHERE `id` = {clanId};";
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

        public static void Update(Clan clan)
        {
            lock (Server.SQLLOCK)
            {
                try
                {
                    var text = $"`Name` = '{clan.Name}'";
                    text += $", `Slogan` = '{clan.Slogan}'";
                    text += $", `ImgId` = {clan.ImgId}";
                    text += $", `Power` = {clan.Power}";
                    text += $", `LeaderName` = '{clan.LeaderName}'";
                    text += $", `CurrMember` = {clan.CurrMember}";
                    text += $", `MaxMember` = {clan.MaxMember}";
                    text += $", `Date` = {clan.Date}";
                    text += $", `Level` = {clan.Level}";
                    text += $", `Point` = {clan.Point}";
                    text += $", `Members` = '{JsonConvert.SerializeObject(clan.Members)}'";
                    text += $", `Messages` = '{JsonConvert.SerializeObject(clan.Messages)}'";
                    text += $", `CharacterPeas` = '{JsonConvert.SerializeObject(clan.CharacterPeas)}'";
                    DbContext.gI()?.ConnectToAccount();
                    using DbCommand command = DbContext.gI()?.Connection.CreateCommand();
                    if (command == null) return;
                    command.CommandText = $"UPDATE `clan` SET {text}  WHERE `id` = {clan.Id};";
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
    }
}