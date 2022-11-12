namespace NRO_Server.Application.Interfaces.Character
{
    public interface IMagicTreeHandler
    {
        void Update(int id);
        void Flush();
        void HandleNgoc();
    }
}