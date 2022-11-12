using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NRO_Server.Application.IO;
using NRO_Server.Application.Manager;
using Org.BouncyCastle.Math.Field;

namespace NRO_Server.Application.Threading
{
    public class MagicTreeRunTime
    {
        public static bool IsStop = false;
        public static int RunTimeUpdate1 = -1;
        public static bool IsRunTimeSave = true;

        public MagicTreeRunTime()
        {
            
        }
        public void StartMagicTree()
        {
            new Thread(new ThreadStart(() =>
            {
                while (Server.Gi().IsRunning)
                {
                    var now = ServerUtils.TimeNow();

                    if (now.Hour == 1 && now.Minute == 0 && IsRunTimeSave) 
                    {
                        IsRunTimeSave = false;
                        Parallel.ForEach(MagicTreeManager.Entrys.Values.ToList(), tree => tree.MagicTreeHandler.Update(0));
                    }
                    else if(RunTimeUpdate1 != now.Minute)
                    {
                        RunTimeUpdate1 = now.Minute;
                        Parallel.ForEach(MagicTreeManager.Entrys.Values.ToList(), tree => tree.MagicTreeHandler.Update(1));
                        if (now.Hour != 1 && !IsRunTimeSave) IsRunTimeSave = true;
                    }
                    Thread.Sleep(1000);
                }
                MagicTreeManager.Entrys.Values.ToList().ForEach(tree => tree.MagicTreeHandler.Flush());
                Server.Gi().Logger.Print("MagicTree Manager is close Success...");
                IsStop = true;
            })).Start();
        }
    }
}