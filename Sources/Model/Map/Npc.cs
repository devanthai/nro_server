namespace NRO_Server.Model.Map
{
    public class Npc
    {
        public short Id { get; set; }
        public byte Status { get; set; }
        public short X { get; set; }
        public short Y { get; set; }
        public short Avatar { get; set; }

        public Npc()
        {
            
        }
    }
}