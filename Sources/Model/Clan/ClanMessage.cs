namespace NRO_Server.Model.Clan
{
    public class ClanMessage
    {
        public int Type { get; set; }
        public int Id { get; set; }
        public int PlayerId { get; set; }
        public string PlayerName { get; set; }
        public int Role { get; set; }
        public int Time { get; set; }
        public string Text { get; set; }
        public int Recieve { get; set; }
        public int MaxCap { get; set; }
        public int Color { get; set; }
        public bool NewMessage { get; set; }

        public ClanMessage()
        {
            Role = 2;
            Text = "";
            Recieve = 0;
            MaxCap = 5;
            Color = 0;
            NewMessage = true;
        }
    }
}