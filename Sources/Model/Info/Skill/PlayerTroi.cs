using System.Collections.Generic;

namespace NRO_Server.Model.Info.Skill
{
    public class PlayerTroi
    {
        public bool IsPlayerTroi { get; set; }
        public long TimeTroi { get; set; }
        public List<int> PlayerId { get; set; }

        public PlayerTroi()
        {
            IsPlayerTroi = false;
            TimeTroi = -1;
            PlayerId = new List<int>();
        }
    }
}