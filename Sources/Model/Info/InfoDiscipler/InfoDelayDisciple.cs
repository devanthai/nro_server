using NRO_Server.Application.IO;

namespace NRO_Server.Model.Info
{
    public class InfoDelayDisciple
    {
        public long Fire { get; set; }
        public long LeaveDead { get; set; }
        public long AutoMove { get; set; }
        public long AutoChat { get; set; }
        public long AutoPlusPoint { get; set; }
        public long TTNL { get; set; }
        public long SaveData { get; set; }

        public InfoDelayDisciple()
        {
            AutoMove = Fire = ServerUtils.CurrentTimeMillis() + 1500;
            LeaveDead = -1;
            AutoChat = ServerUtils.CurrentTimeMillis() + 5000;
            AutoPlusPoint = -1;
            TTNL = ServerUtils.CurrentTimeMillis() + 1500;
            SaveData = ServerUtils.CurrentTimeMillis() + 600000;
        }
    }
}