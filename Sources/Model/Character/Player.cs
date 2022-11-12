using NRO_Server.Application.Interfaces.Character;
using NRO_Server.Application.Interfaces.Map;

namespace NRO_Server.Model
{
    public class Player
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int Role { get; set; }
        public bool IsOnline { get; set; }
        public bool IsLock { get; set; }
        public byte Status { get; set; }
        public byte Ban { get; set; }
        public int CharId { get; set; }
        public int TongVND { get; set; }
        public int DiemTichNap { get; set; }
        public ICharacter Character { get; set; }
        public ISession_ME Session { get; set; }

        public Player()
        {
            
        }

        public Player(ISession_ME session)
        {
            Session = session;
        }
    }
}