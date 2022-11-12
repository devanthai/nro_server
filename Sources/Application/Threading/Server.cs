using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.Extensions.Configuration;
using NRO_Server.Application.Constants;
using NRO_Server.Application.Manager;
using NRO_Server.Application.IO;
using NRO_Server.Logging;
using NRO_Server.Model.BangXepHang;
using InitData = NRO_Server.DatabaseManager.InitData;
using Task = System.Threading.Tasks.Task;

namespace NRO_Server.Application.Threading
{
    public class Server
    {
        private static Server Instance { get; set; } = null;
        public static readonly object SQLLOCK = new object();
        public static readonly object IPLOCK = new object();
        private IPAddress IpAddress { get; set; }
        private TcpListener Listener { get; set; }
        public bool IsRunning { get; set; }
        public bool IsSaving { get; set; }
        private Thread RunServer { get; set; }
        public IServerLogger Logger { get; set; }
        public IConfiguration Config { get; set; }

        private DatabaseManager.InitData _initData;
        private Thread _serverRun;
        private ClanRunTime _clanRun;
        private MagicTreeRunTime _magicTreeRun;
        private BossRunTime _bossRun;
        public BxhRunTime BangXepHang;
        public long DelayLogin { get; set; }

        public long StartServerTime { get; set; }
        public int CountLogin { get; set; }

        public bool LockCloneGiaoDich { get; set; }

        public readonly string DROP_KEY = "dropsuperdrop";

        public static Server Gi()
        {
            return Instance ??= new Server();
        }

        public Server()
        {
            IpAddress = IPAddress.Parse(DatabaseManager.Manager.gI().ServerHost);
            Listener = new TcpListener(IpAddress, DatabaseManager.Manager.gI().ServerPort);
            RunServer = null;
        }

        private void InitServer()
        {
            _initData = new DatabaseManager.InitData();
            if (_clanRun == null)
            {
                _clanRun = new ClanRunTime();
            }

            if (_magicTreeRun == null)
            {
                _magicTreeRun = new MagicTreeRunTime();
            }

            if (_bossRun == null)
            {
                _bossRun = new BossRunTime();
            }
            if (BangXepHang == null)
            {
                BangXepHang = new BxhRunTime();
            }
        }

        public void StartServer(bool running, IServerLogger logger, IConfiguration config, bool isRestart)
        {
            Logger = logger;
            Config = config;
            IsRunning = running;
            DelayLogin = ServerUtils.CurrentTimeMillis();
            StartServerTime = ServerUtils.CurrentTimeMillis();
            LockCloneGiaoDich = true;
            CountLogin = 0;
            if (!IsRunning) return;
            InitServer();
            Logger.Print($"Loading Database Successful");
            Logger.Print($"Server Start Open Port: {DatabaseManager.Manager.gI().ServerPort}");
            Listener.Start();
            //Start Magic tree
            _serverRun = new Thread(() =>
            {
                while (IsRunning)
                {
                    try
                    {
                        lock (IPLOCK)
                        {
                            var client = Listener.AcceptTcpClient();
                            if (!client.Connected) continue;
                            var ipv4 = client.Client.RemoteEndPoint?.ToString()?.Split(':')[0];

                            var session = new Session_ME(client, ipv4);
                            session.StartSession();
                            ClientManager.Gi().Add(session);
                            Logger.Info($"Accpet Session: {session.Id} Successful");
                        }
                        /*Thread.Sleep(50);*/
                    }
                    catch (Exception)
                    {
                        IsRunning = false;
                    }
                }
                SaveData();
                IsSaving = false;
                Task.Run(() =>
                {
                    while (!MagicTreeRunTime.IsStop || !ClanRunTime.IsStop || !BossRunTime.IsStop)
                    {
                        //Ignore
                    }
                    Logger.Print("Server Shutdown Success!");
                });
            });
            _serverRun.Start();
            _clanRun.StartClan();
            _magicTreeRun.StartMagicTree();
            _bossRun.StartBossRunTime();
            BangXepHang.Start();
        }

        public void StopServer()
        {
            Listener.Stop();
            IsSaving = true;
        }

        private void SaveData()
        {
            ClientManager.Gi().Clear();
            Logger.Print("Save DATA Player Server Sucess!!!");
        }

        public void RestartServer()
        {
            StopServer();
            Task.Run(() =>
            {
                while (IsSaving || !MagicTreeRunTime.IsStop || !ClanRunTime.IsStop)
                {
                    continue;
                }
                StartServer(true, Logger, Config, true);
                Logger.Print("Server Restart Success!");
            });
        }
    }
}