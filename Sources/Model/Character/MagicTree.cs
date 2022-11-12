using System.Collections.Generic;
using NRO_Server.Application.Handlers.Character;
using NRO_Server.Application.Interfaces.Character;
using NRO_Server.DatabaseManager;

namespace NRO_Server.Model.Character
{
    public class MagicTree
    {
       public int Id { get; set; }
       public short NpcId { get; set; }
       public short X { get; set; }
       public short Y { get; set; }
       public int Level { get; set; }
       public int Peas { get; set; }
       public int MaxPea { get; set; }
       public long Seconds { get; set; }
       public bool IsUpdate { get; set; }
       public int Diamond { get; set; }

       public IMagicTreeHandler MagicTreeHandler { get; set; }

       public MagicTree()
       {
           MagicTreeHandler = new MagicTreeHandler(this);
       }


       public MagicTree(int charId, int gender)
       {
           Id = charId;
           Level = 1;
           Peas = 5;
           MaxPea = 5;
           IsUpdate = false;
           Diamond = 0;
           Seconds = 0;
           SetUpData(gender);
           MagicTreeHandler = new MagicTreeHandler(this);
       }

       private void SetUpData(int gender)
       {
           switch (gender)
           {
               case 0:
               {
                   NpcId = 84;
                   X = 348;
                   Y = 336;
                   break;
               }
               case 1:
               {
                   NpcId = 371;
                   X = 372;
                   Y = 336;
                   break;
               }
               case 2:
               {
                   NpcId = 378;
                   X = 372;
                   Y = 336;
                   break;
               }
           }
       }
    }
}