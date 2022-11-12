using System.Collections.Generic;
using NRO_Server.Model.ModelBase;

namespace NRO_Server.Model.Template
{
    public class TaskTemplate : TaskBase
    {
        public string Name { get; set; }
        public string Detail { get; set; }

        public TaskTemplate() : base()
        {
        }
    }
}