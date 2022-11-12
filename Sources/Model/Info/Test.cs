namespace NRO_Server.Model.Info
{
    public class Test
    {
        public long DelayTest { get; set; }
        public bool IsTest { get; set; }
        public int TestCharacterId { get; set; }
        public int CheckId { get; set; }
        public int GoldTest { get; set; }

        public Test()
        {
            DelayTest = -1;
            IsTest = false;
            TestCharacterId = 1;
            GoldTest = 0;
            CheckId = -1;
        }
    }
}