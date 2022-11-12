using System.Collections.Generic;

namespace NRO_Server.Model.Template
{
    public class ClanImage
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Gold { get; set; }
        public int Diamond { get; set; }
        public List<short> Data { get; set; }

        public ClanImage()
        {
            Gold = -1;
            Diamond = -1;
            Id = -1;
            Data = new List<short>();
        }
    }
}