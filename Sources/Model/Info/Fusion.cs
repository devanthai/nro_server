using System.Collections.Concurrent;
using System.Collections.Generic;
using NRO_Server.Model.Info.Radar;
using NRO_Server.Model.Item;
using NRO_Server.Model.Task;

namespace NRO_Server.Model.Info
{
    public class Fusion
    {
        public bool IsFusion { get; set; }
        public bool IsPorata { get; set; }
        public bool IsPorata2 { get; set; }
        public long TimeStart { get; set; }
        public long DelayFusion { get; set; }
        public int TimeUse { get; set; }

        public Fusion()
        {
            IsFusion = false;
            IsPorata = false;
            IsPorata2 = false;
            TimeStart = -1;
            DelayFusion = -1;
            TimeUse = 0;
        }

        public void Reset()
        {
            IsFusion = false;
            IsPorata = false;
            IsPorata2 = false;
            TimeStart = -1;
            DelayFusion = -1;
            TimeUse = 0;
        }

    }
}