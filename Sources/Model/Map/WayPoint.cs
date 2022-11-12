namespace NRO_Server.Model.Map
{
    public class WayPoint
    {
        public short MinX { get; set; }
        public short MinY { get; set; }
        public short MaxX { get; set; }
        public short MaxY { get; set; }
        public bool IsEnter { get; set; }
        public bool IsOffline { get; set; }
        public string Name { get; set; }
        public short MapNextId { get; set; }

        public WayPoint()
        {
            
        }
    }
}