namespace NRO_Server.Model.Template
{
    public class GachTheResponse
    {
        public int status { get; set; }
        public string message { get; set; }

        public GachTheResponse()
        {
            status = -1;
            message = "";
        }
    }
}