using System;
using System.Data;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using NRO_Server.Application.Threading;

namespace NRO_Server.DatabaseManager
{
    public class DbContext
    {
        private static DbContext Instance;
        private MySqlConnectionStringBuilder _stringBuilder;
        public MySqlConnection Connection;

        public DbContext()
        {
            _stringBuilder = new MySqlConnectionStringBuilder();
            _stringBuilder["Server"] = Manager.gI().MySqlHost;
            _stringBuilder["Port"] = Manager.gI().MySqlPort;
            _stringBuilder["User Id"] = Manager.gI().MySqlUsername;
            _stringBuilder["Password"] = Manager.gI().MySqlPassword;
            _stringBuilder["charset"] = "utf8mb4";
        }

        public static DbContext gI()
        {
            if (Instance == null) Instance = new DbContext();
            return Instance;
        }

        public void ConnectToData()
        {
            Connection?.Close();
            _stringBuilder["Database"] = Manager.gI().MySqlDBData;
            Connection = new MySqlConnection(_stringBuilder.ToString());
            Connection.Open();
        }

        public void ConnectToAccount()
        {
            Connection?.Close();
            _stringBuilder["Database"] = Manager.gI().MySqlDBAccount;
            Connection = new MySqlConnection(_stringBuilder.ToString());
            Connection.Open();
        }

        public void CloseConnect()
        {
            Connection?.Close();
        }
    }
}