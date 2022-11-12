namespace NRO_Server.Model.Item
{
    public class LockInventory
    {
        public bool IsLock { get; set; }
        public int Pass { get; set; }
        public int PassTemp { get; set; }

        public LockInventory()
        {
            IsLock = false;
            Pass = -1;
            PassTemp = -1;
        }
    }
}