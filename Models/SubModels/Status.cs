using System.Collections.Generic;

namespace SkisporetUpdates.Models
{
    public class Status
    {
        public int TailSessionId { get; set; }
        public string StartTime { get; set; }
        public int Level { get; set; }
        public string ColorName { get; set; }
        public LineStyle LineStyle { get; set; }
        public List<LatLng> LatLngs { get; set; }
    }
}