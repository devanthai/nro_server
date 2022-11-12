using System;

namespace NRO_Server.Logging
{
    public interface IServerLogger
    {
        void Debug(string message);
        void Print(string message);
        void Info(string info);
        void Warning(string message, Exception exception);
        void Error(string message, Exception exception);
        void Error(string message);
    }
    
}