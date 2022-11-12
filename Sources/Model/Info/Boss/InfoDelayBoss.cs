using NRO_Server.Application.IO;

namespace NRO_Server.Model.Info
{
    public class InfoDelayBoss
    {
        public long LeaveDead { get; set; }
        public long TTNL { get; set; }
        public long AutoMove { get; set; }
        public long AutoChat { get; set; }

        public long AutoChangeMap { get; set; }

        public InfoDelayBoss()
        {
            LeaveDead = -1;
            AutoMove = ServerUtils.CurrentTimeMillis() + 1500;
            AutoChat = ServerUtils.CurrentTimeMillis() + 5000;
            TTNL = ServerUtils.CurrentTimeMillis() + 1500;
            AutoChangeMap = ServerUtils.CurrentTimeMillis() + 500000;
        }
    }
}