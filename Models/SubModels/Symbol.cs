namespace SkisporetUpdates.Models
{
    public class Symbol
    {
        public int DestinationId { get; set; }
        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int PrepStatus { get; set; }
        public string LabelDirection { get; set; }
    }
}
