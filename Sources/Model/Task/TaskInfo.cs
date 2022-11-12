namespace NRO_Server.Model.Task
{
    public class TaskInfo
    {
       public short Id { get; set; } 
       public sbyte Index { get; set; }     
       public short Count { get; set; }

       public TaskInfo()
       {
           Id = 24;
           Index = 0;
           Count = 0;
       }
    }
}