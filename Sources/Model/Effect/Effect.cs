namespace NRO_Server.Model.Effect
{
    public class Effect
    {
        public Buff BuffHp30S { get; set; }
        public Buff BuffKi30S { get; set; }
        public Buff AuraBuffHp30S { get; set; }
        public Buff AuraBuffKi30S { get; set; }
        
        public Buff BuffKi1s { get; set; }

        public Effect()
        {
            BuffHp30S = new Buff();
            BuffKi30S = new Buff();
            AuraBuffHp30S = new Buff();
            AuraBuffKi30S = new Buff();

            BuffKi1s = new Buff();
        }
    }
}