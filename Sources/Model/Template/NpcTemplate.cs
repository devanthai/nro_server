namespace NRO_Server.Model.Template
{
    public class NpcTemplate
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public short Head { get; set; }
        public short Body { get; set; }
        public short Leg { get; set; }
        public string[][] Menu { get; set; }

        public NpcTemplate()
        {
            
        }
    }
}