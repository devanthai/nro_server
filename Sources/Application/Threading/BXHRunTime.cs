using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NRO_Server.Application.IO;
using NRO_Server.DatabaseManager.Player;
using NRO_Server.Model.BangXepHang;

namespace NRO_Server.Application.Threading
{
    public class BxhRunTime
    {
        public List<BangXepHang> Players { get; set; }
        public List<BangXepHang> TopNap { get; set; }
        public Task RunTime { get; set; }


        public BxhRunTime()
        {
            Players = new List<BangXepHang>();
            TopNap = new List<BangXepHang>();
        }
        public void Start()
        {
            if (RunTime != null) return;
            RunTime = new Task(Action);
            RunTime.Start();
        }
        private async void Action()
        {
            while (Server.Gi().IsRunning)
            { 
                Players?.Clear();
                TopNap?.Clear();
                CharacterDB.SelectBXHSuKien(10);
                UserDB.SelectBXHTopNap(10);
                Server.Gi().Logger.Print("Load BXH Success");
                await Task.Delay(60000);
            }
        }

        public string GetList()
        {
            var text = $"{ServerUtils.Color("red")}Bảng Xếp Hạng Sức mạnh\b{ServerUtils.Color("blue")}";
            List<BangXepHang> list;
            lock (Players)
            {
                list = Players.ToList();
            }
            for (var i = 1; i < 11; i++)
            {
                string name = null;
                long diem = 0;
                list.ForEach(player =>
                {
                    if (player.I != i) return;
                    name = player.Name;
                    diem = player.Diem;
                });
                if(diem != 0)text += $"TOP {i}: {name}: {diem}\b";
            }
            return text;
        }

        public string GetListTopNap()
        {
            var text = $"{ServerUtils.Color("red")}Bảng Xếp Hạng TOP Nạp\b{ServerUtils.Color("blue")}";
            List<BangXepHang> list;
            lock (TopNap)
            {
                list = TopNap.ToList();
            }
            for (var i = 1; i < 11; i++)
            {
                string name = null;
                long diem = 0;
                list.ForEach(player =>
                {
                    if (player.I != i) return;
                    name = player.Name;
                    diem = player.Diem;
                });
                if(diem != 0)text += $"TOP {i}: {name}: {ServerUtils.GetMoneys((long)diem)} VNĐ\b";
            }
            return text;
        }
    }
}