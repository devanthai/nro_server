using System;
using NRO_Server.Model.Map;

namespace NRO_Server.Application.Interfaces.Item
{
    public interface IItemMapHandler : IDisposable
    {
        void Update(Zone zone);
    }
}