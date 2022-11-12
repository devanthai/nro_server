using System.Threading.Tasks;
using NRO_Server.Application.Interfaces.Map;
using NRO_Server.Application.IO;

namespace NRO_Server.Application.Interfaces.Client
{
    public interface IMessageHandler
    {
        void OnConnectionFail(ISession_ME client, bool isMain);

        void OnConnectOK(ISession_ME client, bool isMain);

        void OnDisconnected(ISession_ME client, bool isMain);

        Task OnMessage(Message message);
    }
}