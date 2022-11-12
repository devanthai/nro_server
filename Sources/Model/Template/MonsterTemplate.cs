namespace NRO_Server.Model.Template
{
    public class MonsterTemplate
    {
        public int Id { get; set; }
        public byte RangeMove { get; set; }
        public byte Speed { get; set; }
        public byte Type { get; set; }
        public int Hp { get; set; }
        public string Name { get; set; }
        public byte DartType { get; set; }
        public int LeaveItemType { get; set; }

        public MonsterTemplate()
        {
            
        }
    }
}