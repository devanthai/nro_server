using System.Threading.Tasks;
using NRO_Server.Application.Constants;
using NRO_Server.Application.Main;
using NRO_Server.Application.Manager;

namespace NRO_Server.Application.Threading
{
    public class Maintenance
    {
        private static Maintenance _instance;
        public int TimeCount { get; set; }
        public bool IsStart { get; set; }

        private Maintenance()
        {
            TimeCount = 3;
            IsStart = false;
        }

        public static Maintenance Gi()
        {
            return _instance ??= new Maintenance();
        }

        public void Start(int time)
        {
            TimeCount = time;
            IsStart = true;
            var task = new Task(Action);
            task.Start();
        }

        private async void Action()
        {
            while (IsStart)
            {
                var text = string.Format(TextServer.gI().MAINTENANCE, TimeCount);
                ClientManager.Gi().SendMessageCharacter(Service.WorldChat(null, text, 0));
                ClientManager.Gi().SendMessageCharacter(Service.ServerChat(text));
                TimeCount--;
                if (TimeCount <= 0) IsStart = false;
                await Task.Delay(60000);
            }

            Server.Gi().StopServer();
        }
    }
}