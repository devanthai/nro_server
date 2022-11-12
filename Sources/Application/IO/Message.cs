using System;
using System.IO;
using NRO_Server.Application.Threading;

namespace NRO_Server.Application.IO
{
    public class Message
    {
        public sbyte Command { get; set; }
        public MyReader Reader { get; set; } //dis
        public MyWriter Writer { get; set; } //dos

        public Message(int command)
        {
            Command = (sbyte)command;
            Writer = new MyWriter();
        }

        public Message()
        {
            Writer = new MyWriter();
        }

        public Message(sbyte command) 
        {
            Command = command;
            Writer = new MyWriter();
        }
        
        public Message(sbyte command, sbyte[] data)
        {
            Command = command;
            Reader = new MyReader(data);
        }

        public sbyte[] GetData()
        {
            return Writer.GetData();
        }

        public void CleanUp()
        {
            try
            {
                if (Reader != null)
                {
                    Reader.Close();
                    Reader = null;
                }

                if (Writer != null)
                {
                    Writer.Close();
                    Writer = null;
                }
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error CleanUp Message in Message.cs: {e.Message} \n {e.StackTrace}", e);
            }
        }
    }
}