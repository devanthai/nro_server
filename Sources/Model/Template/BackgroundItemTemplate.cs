using System.Collections;
using System.Collections.Generic;

namespace NRO_Server.Model.Template
{
    public class BackgroundItemTemplate
    {
        public int Id { get; set; }
        public int BackgroundId { get; set; }
        public int Layer { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public BackgroundItemTemplate() {}
    }
}