using NRO_Server.Application.IO;
using NRO_Server.Model;

namespace NRO_Server.Application.Interfaces.Map
{
    public interface ISession_ME
    {
        int Id { get; set; }  
        string IpV4 { get; set; }
        bool IsNewVersion { get; set; }  
        int TimeConnected { get; set; }
        
        Player Player { get; set; }
        sbyte ZoomLevel { get; set; }
        void CloseMessage();
        bool IsConnected();
        bool IsLogin { get; set; }
        void SendMessage(Message message);
        void HansakeMessage();     
        void SetConnect(Message message);
        bool LoginGame(string c_username, string c_password, string c_version, sbyte c_type, Message message);
        void Disconnect();
    }
}