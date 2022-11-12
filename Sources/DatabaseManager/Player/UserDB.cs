using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Data;
using System.Data.Common;
using System.IO;
using NRO_Server.Application.Threading;
using NRO_Server.Application.Manager;
using NRO_Server.Application.Interfaces.Map;
using NRO_Server.Application.IO;
using NRO_Server.Model.BangXepHang;

namespace NRO_Server.DatabaseManager.Player
{
    public class UserDB
    {
        public static Model.Player Login(string username, string password)
        {
            lock (Server.SQLLOCK)
            {
                try
                {
                    DbContext.gI()?.ConnectToAccount();
                    using (DbCommand command = DbContext.gI()?.Connection.CreateCommand())
                    {
                        command.CommandText =
                            $"SELECT * FROM `user` WHERE (`username` = '{username}' AND `password` = '{password}');";
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    int id = reader.GetInt32(0);
                                    int characterId = reader.GetInt32(3);
                                    bool isLock = reader.GetInt32(4) == 0 ? false : true;
                                    int role = reader.GetInt32(5);
                                    byte ban = reader.GetByte(6);
                                    bool isOnline = reader.GetInt32(7) == 0 ? false : true;
                                    int tongVND = reader.GetInt32(12);
                                    int tichnap = reader.GetInt32(14);
                                    bool isLogin = reader.GetInt32(18) == 0 ? false : true;
                                    if (isOnline) return null;
                                    if (isLogin) return null;
                                    Model.Player player = new Model.Player()
                                    {
                                        Id = id,
                                        Username = username,
                                        CharId = characterId,
                                        IsLock = isLock,
                                        Role = role,
                                        Ban = ban,
                                        IsOnline = isOnline,
                                        TongVND = tongVND,
                                        DiemTichNap = tichnap
                                    };
                                    return player;
                                }
                            }
                            else
                            {
                                return null;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Server.Gi().Logger.Error($"Error Login User: {e.Message}\n{e.StackTrace}");
                    return null;
                }
                finally
                {
                    DbContext.gI()?.ConnectToAccount();
                }
            }
            return null;
        }

        public static void UpdatePort(int playerId, int port)
        {
            lock (Server.SQLLOCK)
            {
                try
                {
                    DbContext.gI()?.ConnectToAccount();
                    using (DbCommand command = DbContext.gI()?.Connection.CreateCommand())
                    {
                        // int port = DatabaseManager.Manager.gI().ServerPort;
                        command.CommandText = $"UPDATE `user` SET `sv_port`= {port} WHERE `id`=" + playerId + " LIMIT 1;";
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception e)
                {
                    Server.Gi().Logger.Error($"Error Update Port User: {e.Message}\n{e.StackTrace}");
                }
                finally
                {
                    DbContext.gI()?.CloseConnect();
                }
            }
        }

        public static bool Update(Model.Player player, string ipV4 = "", bool isCreateChar = false)
        {
            if (ipV4 != "")
            {
                var timeServer = ServerUtils.CurrentTimeMillis();
                ServerUtils.WriteLog("login", $"Tên tài khoản {player.Username} (ID:{player.Id}) ipV4 {ipV4}");
                // Chỉ có đăng nhập mới có IP V4
                // Kiểm tra thời gian đăng nhập gần nhất
                // Dưới 20 giây là khóa tài khoản
                long thoiGianDangNhap = 0;
                if (ClientManager.Gi().UserLoginTime.TryGetValue(player.Id, out thoiGianDangNhap) && !isCreateChar)
                {
                    var difTime = (timeServer-thoiGianDangNhap);
                    if (difTime < 10000)
                    {
                        // ban tài khoản
                        UserDB.BanUser(player.Id);
                        ClientManager.Gi().KickSession(player.Session);
                        ServerUtils.WriteLog("bandn", $"Tên tài khoản {player.Username} (ID:{player.Id}) Dif time: {difTime} ms");

                        var temp = ClientManager.Gi().GetPlayer(player.Id);
                        if (temp != null)
                        {
                            ClientManager.Gi().KickSession(temp.Session);
                        }
                        return false;
                    }
                    else 
                    {
                        ClientManager.Gi().UserLoginTime.TryUpdate(player.Id, timeServer, thoiGianDangNhap);
                    }
                }
                else 
                {
                    ClientManager.Gi().UserLoginTime.TryAdd(player.Id, timeServer);
                }
            }
            else 
            {
                ServerUtils.WriteLog("login", $"(THOÁT) Tên tài khoản {player.Username} (ID:{player.Id})");
            }
            lock (Server.SQLLOCK)
            {
                try
                {
                    var timeServer = ServerUtils.CurrentTimeSecond() + 30;
                    DbContext.gI()?.ConnectToAccount();
                    using (DbCommand command = DbContext.gI()?.Connection.CreateCommand())
                    {
                        int online = player.IsOnline ? 1 : 0;
                        if (ipV4 != "")
                        {
                            command.CommandText = $"UPDATE `user` SET `online`= {online}, `logout_time`= {timeServer}, `last_ip` = '{ipV4}', `is_login`=0, `character` = {player.CharId} WHERE `id`=" + player.Id + " LIMIT 1;";
                        }
                        else 
                        {
                            command.CommandText = $"UPDATE `user` SET `online`= {online}, `logout_time`= {timeServer}, `character` = {player.CharId} WHERE `id`=" + player.Id + " LIMIT 1;";
                        }
                        command.ExecuteNonQuery();
                        return true;
                    }
                }
                catch (Exception e)
                {
                    Server.Gi().Logger.Error($"Error Update User: {e.Message}\n{e.StackTrace}");
                    return false;
                }
                finally
                {
                    DbContext.gI()?.CloseConnect();
                }
            }
        }
        
        public static bool UpdateLogin(int userId, int login)
        {
            lock (Server.SQLLOCK)
            {
                try
                {
                    DbContext.gI()?.ConnectToAccount();
                    using (DbCommand command = DbContext.gI()?.Connection.CreateCommand())
                    {
                        command.CommandText = $"UPDATE `user` SET `is_login`= {login} WHERE `id`=" + userId + " LIMIT 1;";
                        command.ExecuteNonQuery();
                        return true;
                    }
                }
                catch (Exception e)
                {
                    Server.Gi().Logger.Error($"Error UpdateLogin User: {e.Message}\n{e.StackTrace}");
                    return false;
                }
                finally
                {
                    DbContext.gI()?.CloseConnect();
                }
            }
        }
        
        public static bool BanUser(int id)
        {
            lock (Server.SQLLOCK)
            {
                try
                {
                    DbContext.gI()?.ConnectToAccount();
                    using (DbCommand command = DbContext.gI()?.Connection.CreateCommand())
                    {
                        command.CommandText = $"UPDATE `user` SET `ban`= 1 WHERE `id`=" + id + " LIMIT 1;";
                        command.ExecuteNonQuery();
                        return true;
                    }
                }
                catch (Exception e)
                {
                    Server.Gi().Logger.Error($"Error Update User: {e.Message}\n{e.StackTrace}");
                    return false;
                }
                finally
                {
                    DbContext.gI()?.CloseConnect();
                }
            }
        }

        public static int GetVND(Model.Player player)
        {
            lock (Server.SQLLOCK)
            {
                try
                {
                    DbContext.gI()?.ConnectToAccount();
                    using (DbCommand command = DbContext.gI()?.Connection.CreateCommand())
                    {
                        command.CommandText =
                            $"SELECT vnd FROM `user` WHERE `id` = '{player.Id}' LIMIT 1;";
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    int vnd = reader.GetInt32(0);
                                    return vnd;
                                }
                            }
                            else
                            {
                                return 0;
                            }
                        }
                    }
                    return 0;
                }
                catch (Exception e)
                {
                    Server.Gi().Logger.Error($"Error GetVND User: {e.Message}\n{e.StackTrace}");
                    return 0;
                }
                finally
                {
                    DbContext.gI()?.ConnectToAccount();
                }
            }
        }

        public static int GetTongVND(Model.Player player)
        {
            lock (Server.SQLLOCK)
            {
                try
                {
                    DbContext.gI()?.ConnectToAccount();
                    using (DbCommand command = DbContext.gI()?.Connection.CreateCommand())
                    {
                        command.CommandText =
                            $"SELECT tongnap FROM `user` WHERE `id` = '{player.Id}' LIMIT 1;";
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    int tongnap = reader.GetInt32(0);
                                    return tongnap;
                                }
                            }
                            else
                            {
                                return 0;
                            }
                        }
                    }
                    return 0;
                }
                catch (Exception e)
                {
                    Server.Gi().Logger.Error($"Error GetTongVND User: {e.Message}\n{e.StackTrace}");
                    return 0;
                }
                finally
                {
                    DbContext.gI()?.ConnectToAccount();
                }
            }
        }

        public static bool MineVND(Model.Player player, int value)
        {
            lock (Server.SQLLOCK)
            {
                try
                {
                    DbContext.gI()?.ConnectToAccount();
                    using (DbCommand command = DbContext.gI()?.Connection.CreateCommand())
                    {
                        command.CommandText = $"UPDATE `user` SET `vnd`= vnd-{value} WHERE `id`=" + player.Id + " LIMIT 1;";
                        command.ExecuteNonQuery();
                        return true;
                    }
                }
                catch (Exception e)
                {
                    Server.Gi().Logger.Error($"Error MineVND User: {e.Message}\n{e.StackTrace}");
                    return false;
                }
                finally
                {
                    DbContext.gI()?.CloseConnect();
                }
            }
        }

        public static bool CheckBeforeChangePass(int userId, string oldPass)
        {
            lock (Server.SQLLOCK)
            {
                try
                {
                    DbContext.gI()?.ConnectToAccount();
                    using DbCommand command = DbContext.gI()?.Connection.CreateCommand();
                    if (command == null) return false;
                    command.CommandText = $"SELECT `id` FROM `user` WHERE `password` = '{oldPass}' AND `id`=" + userId + " LIMIT 1;";
                    using var reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        return true;
                    }
                    return false;
                }
                catch (Exception e)
                {
                    Server.Gi().Logger.Error($"CheckBeforeChangePass: {e.Message}\n{e.StackTrace}");
                    return false;
                }
                finally
                {
                    DbContext.gI()?.CloseConnect(); 
                }
                
            }
        }

        public static bool DoiMatKhau(int userId, string newPass)
        {
            lock (Server.SQLLOCK)
            {
                try
                {
                    DbContext.gI()?.ConnectToAccount();
                    using DbCommand command = DbContext.gI()?.Connection.CreateCommand();
                    if (command == null) return false;
                    command.CommandText = $"UPDATE `user` SET `password`='{newPass}' WHERE `id`=" + userId + " LIMIT 1;";
                    command.ExecuteNonQuery();
                    return true;
                }
                catch (Exception e)
                {
                    Server.Gi().Logger.Error($"DoiMatKhau: {e.Message}\n{e.StackTrace}");
                    return false;
                }
                finally
                {
                    DbContext.gI()?.CloseConnect(); 
                }
                
            }
        }

        public static void SelectBXHTopNap(int limit)
        {
            lock (Server.SQLLOCK)
            {
                try
                {
                    DbContext.gI()?.ConnectToAccount();
                    var bxh = Server.Gi().BangXepHang;
                    using DbCommand command = DbContext.gI()?.Connection.CreateCommand();
                    if (command == null) return;
                    command.CommandText = $"SELECT `username`,`tongnap` FROM `user` WHERE (`tongnap` > 0) ORDER BY `tongnap` DESC LIMIT {limit};";
                    using var reader = command.ExecuteReader();
                    if (!reader.HasRows) return;
                    int i = 1;
                    while (reader.Read())
                    {
                        bxh.TopNap.Add(new BangXepHang()
                        {
                            I = i,
                            Name =  reader.GetString(0),
                            Diem = reader.GetInt32(1),
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

        public static bool CheckInvalidPortServer(int playerId)
        {
            lock (Server.SQLLOCK)
            {
                try
                {
                    DbContext.gI()?.ConnectToAccount();
                    using DbCommand command = DbContext.gI()?.Connection.CreateCommand();
                    if (command == null) return false;
                    command.CommandText = $"SELECT `sv_port` FROM `user` WHERE `id` = {playerId};";
                    using var reader = command.ExecuteReader();
                    if (reader.HasRows && reader.Read())
                    {
                        var sv_port = reader.GetInt32(0);
                        return (sv_port != DatabaseManager.Manager.gI().ServerPort);
                    }
                    return false;
                }
                catch (Exception e)
                {
                    Server.Gi().Logger.Error($"Update CheckPortBeforeUpdate error: {e.Message}\n{e.StackTrace}");
                    return false;
                }
                finally
                {
                    DbContext.gI()?.CloseConnect();
                }
            }
        }

        public static bool CheckInvalidPortServer(string username, ref int thoiGianDangNhap, ref bool isOnline)
        {
            lock (Server.SQLLOCK)
            {
                try
                {
                    DbContext.gI()?.ConnectToAccount();
                    using DbCommand command = DbContext.gI()?.Connection.CreateCommand();
                    if (command == null) return false;
                    command.CommandText = $"SELECT `sv_port`, `logout_time`, `online` FROM `user` WHERE `username` = '{username}';";
                    using var reader = command.ExecuteReader();
                    if (reader.HasRows && reader.Read())
                    {
                        var sv_port = reader.GetInt32(0);
                        thoiGianDangNhap = reader.GetInt32(1);
                        isOnline = reader.GetInt32(2) == 0 ? false : true;
                        return (sv_port != DatabaseManager.Manager.gI().ServerPort);
                    }
                    return false;
                }
                catch (Exception e)
                {
                    Server.Gi().Logger.Error($"Update CheckPortBeforeUpdate name error: {e.Message}\n{e.StackTrace}");
                    return false;
                }
                finally
                {
                    DbContext.gI()?.CloseConnect();
                }
            }
        }
    }
}