using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using NRO_Server.Application.IO;
using NRO_Server.Application.Threading;

namespace NRO_Server.Application.Main
{
    public static class FireWall
    {
        public static List<string> BlockIp = new List<string>();
        public static Dictionary<string, IpTime> IpTimes = new Dictionary<string, IpTime>();
        public static void BanIp(string ip)
        {
            lock (Server.IPLOCK)
            {
                if (BlockIp.Contains(ip)) return;
                if (DatabaseManager.Manager.gI().Os == 1)
                {
                    var cmd = $"advfirewall firewall add rule name=\"BlockIP_{ip}_{ServerUtils.RandomNumber(5)}\" dir=in interface=any action=block remoteip={ip}";
                    var psi = new ProcessStartInfo();
                    var process = new Process();
                    psi.FileName = "netsh";
                    psi.WindowStyle = ProcessWindowStyle.Normal;
                    psi.Arguments = cmd;
                    process.StartInfo = psi;
                    process.Start();
                    process.WaitForExit();
                    BlockIp.Add(ip);
                    Server.Gi().Logger.Info($"Banned Ipv4: {ip} ------------------------- SUCCESS");
                    File.AppendAllText("log_ip.txt", $"Ban Ip: {ip} at {ServerUtils.TimeNow():yyyy-MM-dd HH:mm:ss}");
                }
                // if (BlockIp.Count > DatabaseManager.Manager.gI().MaxConnectseSocket)
                if (BlockIp.Count > DatabaseManager.Manager.gI().MaxConnectSocket)
                {
                    BlockIp.Clear();
                }
            }
        }
    }

    public class IpTime 
    {
        public string Ip { get; set; }
        public long Time { get; set; }
        public int Count { get; set; }
    }
}